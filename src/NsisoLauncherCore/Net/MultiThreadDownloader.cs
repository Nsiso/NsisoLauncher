using NsisoLauncherCore.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Timers;
using NsisoLauncherCore.Net;
using System.Threading.Tasks;
using System.Net.Http;

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
            if (SynchronizationContext.Current != null)
            {
                sc = SynchronizationContext.Current;
            }
            else
            {
                sc = new SynchronizationContext();
            }
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
        public int ProcessorSize { get; set; } = 3;

        /// <summary>
        /// 网络代理
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// 是否检查文件HASH（如果可用）
        /// </summary>
        public bool CheckFileHash { get; set; }

        /// <summary>
        /// 重新下载尝试次数
        /// </summary>
        public int RetryTimes { get; set; } = 3;

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
        private Task[] _workers;
        private int _downloadSizePerSec;
        private Dictionary<DownloadTask, Exception> _errorList = new Dictionary<DownloadTask, Exception>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private ManualResetEventSlim _pauseResetEvent = new ManualResetEventSlim(true);
        private SynchronizationContext sc;

        /// <summary>
        /// 清除全部下载任务
        /// </summary>
        public void ClearAllTasks()
        {
            sc.Post(new SendOrPostCallback((a) =>
            {
                ViewDownloadTasks.Clear();
            }), null);
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
            sc.Post(new SendOrPostCallback((a) =>
            {
                ViewDownloadTasks.Add(task);
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
            _pauseResetEvent.Reset();
            ApendDebugLog("已申请暂停下载");
        }

        public void RequestContinue()
        {
            _pauseResetEvent.Set();
            ApendDebugLog("已申请继续下载");
        }

        private void CompletedOneTask(DownloadTask task)
        {
            sc.Post(new SendOrPostCallback((a) =>
            {
                ViewDownloadTasks.Remove(task);
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

                    _workers = new Task[ProcessorSize];
                    _timer.Start();

                    #region 新建工作线程
                    for (int i = 0; i < ProcessorSize; i++)
                    {
                        _workers[i] = Task.Run(() => ThreadDownloadWork(cancellationTokenSource.Token));
                    }
                    #endregion

                    #region 监控线程
                    Task.Run(() =>
                    {
                        try
                        {
                            Task.WaitAll(_workers);
                            CompleteDownload();
                            return;
                        }
                        catch (Exception ex)
                        {
                            SendFatalLog(ex, "下载监视线程发生异常");
                        }
                    });
                    #endregion
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程分配时发生异常");
            }
        }

        private async Task ThreadDownloadWork(CancellationToken cancelToken)
        {
            try
            {
                while ((!cancelToken.IsCancellationRequested) && (!_downloadTasks.IsEmpty))
                {
                    if (_downloadTasks.TryDequeue(out DownloadTask item))
                    {
                        item.SetState("下载中");

                        await HTTPDownload(item, cancelToken);

                        #region 执行下载后函数
                        if (item.Todo != null)
                        {
                            if (!cancelToken.IsCancellationRequested)
                            {
                                ApendDebugLog(string.Format("开始执行{0}下载后的安装过程", item.TaskName));
                                item.SetState("安装中");

                                ProgressCallback callback = new ProgressCallback();
                                callback.ProgressChanged += item.AcceptProgressChangedArg;
                                try
                                {
                                    Exception exc = await Task.Run(() => item.Todo(callback, cancelToken));
                                    if (exc != null)
                                    {
                                        SendDownloadErrLog(item, exc);
                                        if (!_errorList.ContainsKey(item))
                                        {
                                            _errorList.Add(item, exc);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SendDownloadErrLog(item, ex);
                                    if (!_errorList.ContainsKey(item))
                                    {
                                        _errorList.Add(item, ex);
                                    }
                                }
                                finally
                                {
                                    callback.ProgressChanged -= item.AcceptProgressChangedArg;
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
        private async Task HTTPDownload(DownloadTask task, CancellationToken cancelToken)
        {
            #region 检查是否空值
            if (string.IsNullOrWhiteSpace(task.From) || string.IsNullOrWhiteSpace(task.To))
            {
                return;
            }
            #endregion

            string realFilename = task.To;
            string buffFilename = realFilename + ".downloadtask";
            Exception exception = null;

            for (int i = 1; i <= RetryTimes; i++)
            {
                try
                {
                    #region 下载前文件准备
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
                        if (CheckFileHash && task.Checker != null)
                        {
                            task.SetState("校验中");
                            if (await task.Checker.CheckFilePassAsync())
                            {
                                return;
                            }
                        }
                        else
                        {
                            File.Delete(realFilename);
                        }
                    }
                    if (File.Exists(buffFilename))
                    {
                        File.Delete(buffFilename);
                    }
                    #endregion

                    #region 下载流程
                    using (var getResult = await NetRequester.Client.GetAsync(task.From, cancelToken))
                    {
                        getResult.EnsureSuccessStatusCode();
                        task.SetTotalSize(getResult.Content.Headers.ContentLength.GetValueOrDefault());
                        using (Stream responseStream = await getResult.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fs = new FileStream(buffFilename, FileMode.Create))
                            {
                                byte[] bArr = new byte[1024];
                                int size = await responseStream.ReadAsync(bArr, 0, (int)bArr.Length);

                                while (size > 0)
                                {
                                    _pauseResetEvent.Wait(cancelToken);
                                    await fs.WriteAsync(bArr, 0, size);
                                    size = await responseStream.ReadAsync(bArr, 0, (int)bArr.Length);
                                    _downloadSizePerSec += size;
                                    task.IncreaseDownloadSize(size);
                                }
                            }
                        }
                    }
                    #endregion

                    //下载完成后转正
                    File.Move(buffFilename, realFilename);

                    #region 下载后校验
                    if (CheckFileHash && task.Checker != null)
                    {
                        task.SetState("校验中");
                        if (!(await task.Checker.CheckFilePassAsync()))
                        {
                            task.SetState("校验失败");
                            ApendDebugLog(string.Format("{0}校验哈希值失败，目标哈希值:{1}", task.TaskName, task.Checker.CheckSum));
                            throw new Exception(string.Format("{0}校验哈希值失败，目标哈希值:{1}", task.TaskName, task.Checker.CheckSum));
                        }
                        else
                        {
                            task.SetState("校验成功");
                        }
                    }
                    #endregion

                    //无错误跳出重试循环
                    exception = null;
                    break;
                }
                catch (Exception e)
                {
                    exception = e;
                    task.SetState(string.Format("重试第{0}次", i));
                    SendDownloadErrLog(task, e);

                    //继续重试
                    continue;
                }
            }

            //处理异常
            if (exception != null)
            {
                SendDownloadErrLog(task, exception);
                if (!_errorList.ContainsKey(task))
                {
                    _errorList.Add(task, exception);
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
            SendLog(new Log() { Exception = ex, LogLevel = LogLevel.ERROR, Message = string.Format("任务{0}下载失败,源地址:{1}错误:\n{2}", task.TaskName, task.From, ex.ToString()) });
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}