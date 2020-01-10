using NsisoLauncherCore.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Timers;

namespace NsisoLauncherCore.Net
{
    public class DownloadProgressChangedArg : EventArgs
    {
        public int DoneTaskCount { get; set; }
        public int LeftTasksCount { get; set; }
        public DownloadTask DoneTask { get; set; }
    }

    public class DownloadSpeedChangedArg : EventArgs
    {
        public double SizePerSec { get; set; }
        public decimal SpeedValue { get; set; }
        public string SpeedUnit { get; set; }
    }

    public class DownloadCompletedArg : EventArgs
    {
        public Dictionary<DownloadTask, Exception> ErrorList { get; set; }
    }

    public class MultiThreadDownloader : INotifyPropertyChanged
    {
        /// <summary>
        /// 初始化一个多线程下载器
        /// </summary>
        public MultiThreadDownloader()
        {
            _timer.Elapsed += _timer_Elapsed;
            sc = SynchronizationContext.Current;
        }

        #region 速度计算（每秒触发事件）
        /// <summary>
        /// 每秒触发事件（下载速度计算）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DownloadSpeedChangedArg arg = new DownloadSpeedChangedArg();
            arg.SizePerSec = _downloadSizePerSec;
            if (_downloadSizePerSec > 1048576)
            {
                arg.SpeedUnit = "MB/s";
                arg.SpeedValue = Math.Round((decimal)_downloadSizePerSec / (decimal)1048576, 2);
                DownloadSpeedChanged?.Invoke(this, arg);

            }
            else if (_downloadSizePerSec > 1024)
            {
                arg.SpeedUnit = "KB/s";
                arg.SpeedValue = Math.Round((decimal)_downloadSizePerSec / (decimal)1024, 2);
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
        #endregion

        #region 公共属性
        /// <summary>
        /// 多线程数量
        /// </summary>
        public int ProcessorSize { get; set; } = 5;

        /// <summary>
        /// 网络代理
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// 是否检查文件HASH（如果可用）
        /// </summary>
        public bool CheckFileHash { get; set; }

        public IEnumerable<DownloadTask> DownloadTaskList { get => ViewDownloadTasks.AsEnumerable(); }
        #endregion

        #region 只读数据属性（可绑定NotifyPropertyChanged）
        private int _leftTaskCount;
        /// <summary>
        /// 剩余下载任务数量
        /// </summary>
        public int LeftTaskCount
        {
            get { return _leftTaskCount; }
            private set
            {
                _leftTaskCount = value;
                OnPropertyChanged("LeftTaskCount");
            }
        }

        private int _doneTaskCount;
        /// <summary>
        /// 已下载任务数量
        /// </summary>
        public int DoneTaskCount
        {
            get { return _doneTaskCount; }
            private set
            {
                _doneTaskCount = value;
                OnPropertyChanged("DoneTaskCount");
            }
        }

        private bool _isBusy;
        /// <summary>
        /// 是否在忙碌
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        /// <summary>
        /// 可观测的绑定用下载列表
        /// </summary>
        public ObservableCollection<DownloadTask> ViewDownloadTasks { get; private set; } = new ObservableCollection<DownloadTask>();


        #endregion

        #region 事件
        public event EventHandler<DownloadProgressChangedArg> DownloadProgressChanged;
        public event EventHandler<DownloadSpeedChangedArg> DownloadSpeedChanged;
        public event EventHandler<DownloadCompletedArg> DownloadCompleted;
        public event EventHandler<Log> DownloadLog;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private ConcurrentQueue<DownloadTask> _downloadTasks = new ConcurrentQueue<DownloadTask>();
        private readonly object _viewDownloadLocker = new object();
        private Thread[] _threads;
        private int _downloadSizePerSec;
        private Dictionary<DownloadTask, Exception> _errorList = new Dictionary<DownloadTask, Exception>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private ManualResetEvent _resetEvent = new ManualResetEvent(true);
        private SynchronizationContext sc;

        /// <summary>
        /// 清除全部下载任务
        /// </summary>
        public void ClearAllTasks()
        {
            sc.Send(new SendOrPostCallback((a) =>
            {
                lock (_viewDownloadLocker)
                {
                    ViewDownloadTasks.Clear();
                }
            }), null);
            _errorList.Clear();
            while (!_downloadTasks.IsEmpty)
            {
                _downloadTasks.TryDequeue(out _);
            }

            DoneTaskCount = 0;
            LeftTaskCount = 0;
        }

        #region 添加任务
        /// <summary>
        /// 添加一个下载任务
        /// </summary>
        /// <param name="task"></param>
        public void AddDownloadTask(DownloadTask task)
        {
            _downloadTasks.Enqueue(task);
            sc.Send(new SendOrPostCallback((a) =>
            {
                lock (_viewDownloadLocker)
                {
                    ViewDownloadTasks.Add(task);
                }
            }), null);
            LeftTaskCount += 1;
        }

        /// <summary>
        /// 添加一堆下载任务
        /// </summary>
        /// <param name="tasks"></param>
        public void AddDownloadTask(List<DownloadTask> tasks)
        {
            foreach (var item in tasks)
            {
                AddDownloadTask(item);
            }
        }
        #endregion

        /// <summary>
        /// 申请取消
        /// </summary>
        public void RequestCancel()
        {
            cancellationTokenSource.Cancel();
            ApendDebugLog("已申请取消下载");
        }

        public void RequestPause()
        {
            _resetEvent.Reset();
            ApendDebugLog("已申请暂停下载");
        }

        public void RequestContinue()
        {
            _resetEvent.Set();
            ApendDebugLog("已申请继续下载");
        }

        private void CompletedOneTask(DownloadTask task)
        {
            sc.Send(new SendOrPostCallback((a) =>
            {
                lock (_viewDownloadLocker)
                {
                    ViewDownloadTasks.Remove(task);
                }
            }), null);
            LeftTaskCount -= 1;
            DoneTaskCount += 1;
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArg()
            { LeftTasksCount = this.LeftTaskCount, DoneTaskCount = this.DoneTaskCount, DoneTask = task });
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            try
            {
                if (!IsBusy)
                {
                    if (ProcessorSize == 0)
                    {
                        throw new ArgumentException("下载器的线程数不能为0");
                    }
                    if (_downloadTasks == null || _downloadTasks.Count == 0)
                    {
                        return;
                    }

                    IsBusy = true;
                    cancellationTokenSource = new CancellationTokenSource();
                    _errorList.Clear();
                    DoneTaskCount = 0;

                    _threads = new Thread[ProcessorSize];
                    _timer.Start();

                    #region 新建工作线程
                    for (int i = 0; i < ProcessorSize; i++)
                    {
                        _threads[i] = new Thread(() =>
                        {
                            ThreadDownloadWork(cancellationTokenSource.Token);
                        });
                        _threads[i].Name = string.Format("下载线程{0}号", i);
                        _threads[i].Start();
                    }
                    #endregion

                    #region 监视线程
                    var checkThread = new Thread(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                Thread.Sleep(500);
                                if (GetAvailableThreadsCount() == 0)
                                {
                                    CompleteDownload();
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SendFatalLog(ex, "下载监视线程发生异常");
                        }
                    });
                    checkThread.Name = "下载监视线程";
                    checkThread.Start();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程分配时发生异常");
            }
        }

        private void ThreadDownloadWork(CancellationToken cancelToken)
        {
            try
            {
                while (!_downloadTasks.IsEmpty)
                {
                    if (_downloadTasks.TryDequeue(out DownloadTask item))
                    {
                        ApendDebugLog("开始下载:" + item.From);
                        item.SetState("下载中");

                        HTTPDownload(item, cancelToken);

                        ApendDebugLog("下载完成:" + item.From);

                        #region 校验
                        if (CheckFileHash && item.Checker != null && File.Exists(item.To))
                        {
                            item.SetState("校验中");
                            if (!item.Checker.CheckFilePass())
                            {
                                item.SetState("校验失败");
                                ApendDebugLog(string.Format("{0}校验哈希值失败，目标哈希值:{1}", item.TaskName, item.Checker.CheckSum));
                                File.Delete(item.To);
                                if (!_errorList.ContainsKey(item))
                                {
                                    _errorList.Add(item, new Exception("文件校验失败"));
                                }
                            }
                            else
                            {
                                item.SetState("校验成功");
                                ApendDebugLog(string.Format("{0}校验哈希值成功:{1}", item.TaskName, item.Checker.CheckSum));
                            }
                        }
                        #endregion

                        #region 执行下载后函数
                        if (item.Todo != null)
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                ApendDebugLog(string.Format("开始执行{0}下载后的安装过程", item.TaskName));
                                item.SetState("安装中");
                                var exc = item.Todo();
                                if (exc != null)
                                {
                                    SendDownloadErrLog(item, exc);
                                    if (!_errorList.ContainsKey(item))
                                    {
                                        _errorList.Add(item, exc);
                                    }
                                }
                            }
                            else
                            {
                                ApendDebugLog("放弃安装:" + item.TaskName);
                            }
                        }
                        #endregion

                        item.SetDone();
                        CompletedOneTask(item);
                    }
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程工作时发生异常");
            }

        }


        /// <summary>
        /// http下载主函数
        /// </summary>
        /// <param name="task">下载任务</param>
        /// <param name="cancelToken">取消的token</param>
        private void HTTPDownload(DownloadTask task, CancellationToken cancelToken)
        {
            string realFilename = task.To;
            string buffFilename = realFilename + ".downloadtask";
            try
            {
                if (Path.IsPathRooted(realFilename))
                {
                    string dirName = Path.GetDirectoryName(realFilename);
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                }
                if (File.Exists(realFilename))
                {
                    return;
                }
                if (File.Exists(buffFilename))
                {
                    File.Delete(buffFilename);
                }
                HttpWebRequest request = WebRequest.Create(task.From) as HttpWebRequest;
                request.Timeout = 5000;
                if (Proxy != null)
                {
                    request.Proxy = Proxy;
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                task.SetTotalSize(response.ContentLength);
                Stream responseStream = response.GetResponseStream();
                responseStream.ReadTimeout = 5000;
                FileStream fs = new FileStream(buffFilename, FileMode.Create);

                try
                {
                    byte[] bArr = new byte[1024];
                    int size = responseStream.Read(bArr, 0, (int)bArr.Length);

                    while (size > 0)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            ApendDebugLog("放弃下载:" + task.TaskName);
                            ClearAllTasks();
                            return;
                        }
                        _resetEvent.WaitOne();
                        fs.Write(bArr, 0, size);
                        size = responseStream.Read(bArr, 0, (int)bArr.Length);
                        _downloadSizePerSec += size;
                        task.IncreaseDownloadSize(size);
                    }
                }
                catch (Exception e)
                {
                    SendDownloadErrLog(task, e);
                    if (!_errorList.ContainsKey(task))
                    {
                        _errorList.Add(task, e);
                    }
                }
                finally
                {
                    fs.Close();
                    responseStream.Close();
                }
                File.Move(buffFilename, realFilename);
            }
            catch (Exception e)
            {
                SendDownloadErrLog(task, e);
                if (!_errorList.ContainsKey(task))
                {
                    _errorList.Add(task, e);
                }
            }
        }

        private void CompleteDownload()
        {
            ClearAllTasks();
            _timer.Stop();
            _downloadSizePerSec = 0;
            IsBusy = false;
            DownloadCompleted?.Invoke(this, new DownloadCompletedArg() { ErrorList = _errorList });
            ApendDebugLog("全部下载任务已完成");
        }

        private void ApendDebugLog(string msg)
        {
            this.DownloadLog?.Invoke(this, new Log() { LogLevel = LogLevel.DEBUG, Message = msg });
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

        private void SendLog(Log e)
        {
            DownloadLog?.Invoke(this, e);
        }

        private void SendFatalLog(Exception ex, string msg)
        {
            SendLog(new Log() { Exception = ex, LogLevel = LogLevel.FATAL, Message = msg });
        }

        private void SendDownloadErrLog(DownloadTask task, Exception ex)
        {
            SendLog(new Log() { Exception = ex, LogLevel = LogLevel.ERROR, Message = string.Format("任务{0}下载失败,源地址:{1}原因:{2}", task.TaskName, task.From, ex.Message) });
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
