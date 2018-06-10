 using NsisoLauncher.Core.Net;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Timers;

namespace NsisoLauncher.Utils
{
    public class DownloadProgressChangedArg : EventArgs
    {
        public int TaskCount { get; set; }
        public int LastTaskCount { get; set; }
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
        public int ProcessorSize { get; set; } = 3;
        public bool IsBusy { get; set; } = false;

        public event EventHandler<DownloadProgressChangedArg> DownloadProgressChanged;
        public event EventHandler<DownloadSpeedChangedArg> DownloadSpeedChanged;
        public event EventHandler<DownloadCompletedArg> DownloadCompleted;

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private List<DownloadTask> _downloadTasks;
        private volatile bool _shouldStop = false;
        private Thread[] _threads;
        private int _downloadSizePerSec;
        private Dictionary<DownloadTask, Exception> _errorList = new Dictionary<DownloadTask, Exception>();

        public void SetDownloadTasks(List<DownloadTask> tasks)
        {
            _downloadTasks = tasks;
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void StartDownload()
        {
            if (ProcessorSize == 0)
            {
                throw new ArgumentException("下载器的线程数不能为0");
            }
            if (_downloadTasks.Count == 0)
            {
                return;
            }
            foreach (var item in _downloadTasks)
            {
                TasksObservableCollection.Add(item);
            }
            _threads = new Thread[ProcessorSize];
            var threadTask = Split(_downloadTasks, ProcessorSize);
            _timer.Start();
            for (int i = 0; i < ProcessorSize; i++)
            {
                List<DownloadTask> arg = threadTask[i];
                _threads[i] = new Thread(() => ThreadDownloadWork(arg));
                _threads[i].Start();
            }
        }

        private void ThreadDownloadWork(List<DownloadTask> tasks)
        {
            foreach (var item in tasks)
            {
                HTTPDownload(item);
                TasksObservableCollection.Remove(item);
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArg() {TaskCount = _downloadTasks.Count, LastTaskCount = TasksObservableCollection.Count });
                if (TasksObservableCollection.Count == 0)
                {
                    DownloadCompleted?.Invoke(this, new DownloadCompletedArg() {ErrorList = _errorList });
                    _downloadTasks.Clear();
                    _timer.Stop();
                    _downloadSizePerSec = 0;
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
                    return;
                }
                HttpWebRequest request = WebRequest.Create(task.From) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                task.SetTotalSize(response.ContentLength);
                Stream responseStream = response.GetResponseStream();
                FileStream fs = new FileStream(task.To, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);

                while (size > 0)
                {
                    if (_shouldStop)
                    {
                        return;
                    }
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    _downloadSizePerSec += size;
                    task.IncreaseDownloadSize(size);
                }
                fs.Close();
                responseStream.Close();
            }
            catch (Exception e)
            {
                _errorList.Add(task, e);
            }
        }

        private List<List<DownloadTask>> Split(List<DownloadTask> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.ToList();
            return splits.ToList();
        }
    }
}
