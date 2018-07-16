using NsisoLauncher.Core.Net;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Timers;
using NsisoLauncher.Core.Modules;
using System.Collections.Concurrent;

namespace NsisoLauncher.Utils
{
    public class DownloadProgressChangedArg : EventArgs
    {
        public int TaskCount { get; set; }
        public int LastTaskCount { get; set; }
        public DownloadTask DoneTask { get; set; }
    }

    public class DownloadSpeedChangedArg : EventArgs
    {
        public int SpeedValue { get; set; }
        public string SpeedUnit { get; set; }
    }

    public class DownloadCompletedArg : EventArgs
    {
        public Dictionary<DownloadTask,Exception> ErrorList { get; set; }
    }

    public class MultiThreadDownloader
    {
        private readonly object removeLock = new object();

        public MultiThreadDownloader()
        {
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DownloadSpeedChangedArg arg = new DownloadSpeedChangedArg();
            if (_downloadSizePerSec > 1048576)
            {
                arg.SpeedUnit = "MB/s";
                arg.SpeedValue = _downloadSizePerSec / 1048576;
                DownloadSpeedChanged?.Invoke(this, arg);

            }
            else if (_downloadSizePerSec > 1024)
            {
                arg.SpeedUnit = "KB/s";
                arg.SpeedValue = _downloadSizePerSec / 1024;
                DownloadSpeedChanged?.Invoke(this, arg);
            }
            else
            {
                arg.SpeedUnit = "B/s";
                arg.SpeedValue = _downloadSizePerSec;
                DownloadSpeedChanged?.Invoke(this, arg);
            }
            _downloadSizePerSec = 0;
        }

        public AsyncObservableCollection<DownloadTask> TasksObservableCollection { get; private set; } = new AsyncObservableCollection<DownloadTask>();
        public int ProcessorSize { get; set; } = 5;
        public bool IsBusy { get; private set; } = false;
        public WebProxy Proxy { get; set; }

        public event EventHandler<DownloadProgressChangedArg> DownloadProgressChanged;
        public event EventHandler<DownloadSpeedChangedArg> DownloadSpeedChanged;
        public event EventHandler<DownloadCompletedArg> DownloadCompleted;
        public event EventHandler<Log> DownloadLog;

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private ConcurrentBag<DownloadTask> _downloadTasks;
        private int _taskCount;
        private volatile bool _shouldStop = false;
        private Thread[] _threads;
        private int _downloadSizePerSec;
        private Dictionary<DownloadTask, Exception> _errorList = new Dictionary<DownloadTask, Exception>();
        
        public void SetDownloadTasks(List<DownloadTask> tasks)
        {
            _downloadTasks = new ConcurrentBag<DownloadTask>(tasks);
            _taskCount = tasks.Count;
        }

        public void RequestStop()
        {
            _shouldStop = true;
            CompleteDownload();
            ApendDebugLog("已申请取消下载");
        }

        public void StartDownload()
        {
            if (!IsBusy)
            {
                _shouldStop = false;
                IsBusy = true;
                if (ProcessorSize == 0)
                {
                    IsBusy = false;
                    throw new ArgumentException("下载器的线程数不能为0");
                }
                if (_downloadTasks == null || _downloadTasks.Count == 0)
                {
                    IsBusy = false;
                    return;
                }
                foreach (var item in _downloadTasks.Reverse())
                {
                    TasksObservableCollection.Add(item);
                }
                _threads = new Thread[ProcessorSize];
                _timer.Start();
                for (int i = 0; i < ProcessorSize; i++)
                {
                    _threads[i] = new Thread(() =>
                    {
                        ThreadDownloadWork();
                    });
                    _threads[i].Name = string.Format("下载线程{0}号", i);
                    _threads[i].Start();
                }
                var checkThread = new Thread(() =>
                {
                    while (true)
                    {
                        if (GetAvailableThreadsCount() == 0)
                        {
                            CompleteDownload();
                            DownloadCompleted?.Invoke(this, new DownloadCompletedArg() { ErrorList = _errorList });
                            break;
                        }
                    }
                });
                checkThread.Name = "下载监视线程";
                checkThread.Start();
            }
        }

        private void ThreadDownloadWork()
        {
            DownloadTask item = null;
            while (!_downloadTasks.IsEmpty)
            {
                if (_downloadTasks.TryTake(out item))
                {
                    if (_shouldStop)
                    {
                        CompleteDownload();
                        return;
                    }
                    ApendDebugLog("开始下载:" + item.From);
                    HTTPDownload(item);
                    ApendDebugLog("下载完成:" + item.From);
                    TasksObservableCollection.Remove(item);
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArg() { TaskCount = _taskCount, LastTaskCount = _downloadTasks.Count, DoneTask = item });
                }
            }

        }

        private void HTTPDownload(DownloadTask task)
        {
            try
            {
                if (Path.IsPathRooted(task.To))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(task.To)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(task.To));
                    }
                }
                if (_shouldStop)
                {
                    CompleteDownload();
                    return;
                }
                HttpWebRequest request = WebRequest.Create(task.From) as HttpWebRequest;
                if (Proxy != null)
                {
                    request.Proxy = Proxy;
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                task.SetTotalSize(response.ContentLength);
                Stream responseStream = response.GetResponseStream();
                FileStream fs = new FileStream(task.To, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);

                while (size > 0)
                {
                    try
                    {
                        if (_shouldStop)
                        {
                            fs.Close();
                            responseStream.Close();
                            if (File.Exists(task.To))
                            {
                                File.Delete(task.To);
                            }
                            CompleteDownload();
                            return;
                        }
                        fs.Write(bArr, 0, size);
                        size = responseStream.Read(bArr, 0, (int)bArr.Length);
                        _downloadSizePerSec += size;
                        task.IncreaseDownloadSize(size);
                    }
                    catch (Exception ex)
                    {
                        fs.Close();
                        responseStream.Close();
                        throw ex;
                    }
                }
                fs.Close();
                responseStream.Close();
            }
            catch (Exception e)
            {
                ApendErrorLog(e);
                if (!_errorList.ContainsKey(task))
                {
                    _errorList.Add(task, e);
                }
                if (File.Exists(task.To))
                {
                    File.Delete(task.To);
                }
            }
        }

        private void CompleteDownload()
        {
            TasksObservableCollection.Clear();
            _timer.Stop();
            _taskCount = 0;
            _downloadSizePerSec = 0;
            IsBusy = false;
            ApendDebugLog("全部下载任务已完成");
        }

        private void ApendDebugLog(string msg)
        {
            this.DownloadLog?.Invoke(this, new Log() { LogLevel = LogLevel.DEBUG, Message = msg });
        }

        private void ApendErrorLog(Exception e)
        {
            this.DownloadLog?.Invoke(this, new Log() { LogLevel = LogLevel.ERROR, Message = "下载文件失败:" + e.ToString() });
        }

        private int GetAvailableThreadsCount()
        {
            int num = 0; ;
            foreach (var item in _threads)
            {
                if (item != null && item.IsAlive)
                {
                    num += 1;
                }
            }
            return num;
        }
    }
}
