using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NsisoLauncherCore.Net
{
    #region 下载事件参数
    public class DownloadProgressChangedArg : EventArgs
    {
        public int DoneTaskCount { get; set; }
        public int LeftTasksCount { get; set; }
        public IDownloadTask DoneTask { get; set; }
    }

    public class DownloadSpeedChangedArg : EventArgs
    {
        public double SizePerSec { get; set; }
        public decimal SpeedValue { get; set; }
        public string SpeedUnit { get; set; }
    }

    public class DownloadCompletedArg : EventArgs
    {
        public Dictionary<IDownloadTask, Exception> ErrorList { get; set; }
    }
    #endregion

    /// <summary>
    /// 一个多线程下载器
    /// 可与WPF前台绑定，事件通知
    /// 高性能&易用
    /// </summary>
    public class MultiThreadDownloader : INotifyPropertyChanged, IDisposable
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
        /// 是否检查文件HASH（如果可用）
        /// </summary>
        public bool CheckFileHash { get; set; }

        /// <summary>
        /// 重新下载尝试次数
        /// </summary>
        public int RetryTimes { get; set; } = 3;

        /// <summary>
        /// 下载源列表
        /// </summary>
        public IList<IDownloadableMirror> MirrorList { get; set; }

        /// <summary>
        /// 下载任务（只读）
        /// </summary>
        public IEnumerable<IDownloadTask> DownloadTaskList { get => ViewDownloadTasks.AsEnumerable(); }

        /// <summary>
        /// 下载使用的协议
        /// </summary>
        public DownloadProtocolType ProtocolType { get; set; } = DownloadProtocolType.ORIGIN;

        /// <summary>
        /// Is downloader now paused.
        /// </summary>
        public bool Paused { get; set; } = false;
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
        public ObservableCollection<IDownloadTask> ViewDownloadTasks { get; private set; } = new ObservableCollection<IDownloadTask>();


        #endregion

        #region 事件
        public event EventHandler<DownloadProgressChangedArg> DownloadProgressChanged;
        public event EventHandler<DownloadSpeedChangedArg> DownloadSpeedChanged;
        public event EventHandler<DownloadCompletedArg> DownloadCompleted;
        public event EventHandler<Log> DownloadLog;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private ConcurrentQueue<IDownloadTask> _downloadTasks = new ConcurrentQueue<IDownloadTask>();
        private Task[] _workers;
        private long _downloadSizePerSec;
        private Dictionary<IDownloadTask, Exception> _errorList = new Dictionary<IDownloadTask, Exception>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ManualResetEventSlim _pauseResetEvent = new ManualResetEventSlim(true);
        private SynchronizationContext sc;
        private IDownloadableMirror mirror = null;

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

            Paused = false;
            DoneTaskCount = 0;
            LeftTaskCount = 0;
        }

        #region 添加任务
        /// <summary>
        /// 添加一个下载任务
        /// </summary>
        /// <param name="task"></param>
        public void AddDownloadTask(IDownloadTask task)
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
        public void AddDownloadTask(IEnumerable<IDownloadTask> tasks)
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
            _cancellationTokenSource.Cancel();
            ApendDebugLog("已申请取消下载");
        }

        public void RequestPause()
        {
            _pauseResetEvent.Reset();
            Paused = true;
            ApendDebugLog("已申请暂停下载");
        }

        public void RequestContinue()
        {
            _pauseResetEvent.Set();
            Paused = false;
            ApendDebugLog("已申请继续下载");
        }

        private void CompletedOneTask(IDownloadTask task)
        {
            sc.Post(new SendOrPostCallback((a) =>
            {
                ViewDownloadTasks.Remove(task);
            }), null);
            LeftTaskCount -= 1;
            DoneTaskCount += 1;
            DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArg()
            { LeftTasksCount = this.LeftTaskCount, DoneTaskCount = this.DoneTaskCount, DoneTask = task });
            if (LeftTaskCount == 0)
            {
                CompleteDownload();
            }
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public async Task StartDownload()
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
                    _cancellationTokenSource = new CancellationTokenSource();
                    _errorList.Clear();
                    DoneTaskCount = 0;

                    _workers = new Task[ProcessorSize];
                    _timer.Start();

                    if (MirrorList?.Count != 0)
                    {
                        mirror = (IDownloadableMirror)await MirrorHelper.ChooseBestMirror(MirrorList).ConfigureAwait(false);
                    }

                    #region 新建工作线程
                    for (int i = 0; i < ProcessorSize; i++)
                    {
                        _workers[i] = Task.Run(() => ThreadDownloadWork(_cancellationTokenSource.Token));
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程分配时发生异常");
            }
        }

        public async Task<DownloadCompletedArg> StartDownloadAndWaitDone()
        {
            EventWaitHandle _waitHandle = new AutoResetEvent(false);
            DownloadCompletedArg completedArg = null;
            await Task.Run(() =>
            {
                this.DownloadCompleted += (a, b) =>
                {
                    _waitHandle.Set();
                    completedArg = b;
                };
            });

            //start download
            await StartDownload();

            await Task.Run(() => _waitHandle.WaitOne());

            if (completedArg != null)
            {
                return completedArg;
            }
            else
            {
                throw new Exception("Can't await the result of download completed arg!");
            }
        }

        private async Task ThreadDownloadWork(CancellationToken cancelToken)
        {
            try
            {
                DownloadSetting downloadSetting = new DownloadSetting()
                { CheckFileHash = this.CheckFileHash, ProtocolType = this.ProtocolType, RetryTimes = this.RetryTimes };
                while ((!cancelToken.IsCancellationRequested) && (!_downloadTasks.IsEmpty))
                {
                    if (_downloadTasks.TryDequeue(out IDownloadTask item))
                    {
                        item.ProgressCallback.IncreasedDoneSize += ProgressCallback_IncreasedDoneSize;

                        var result = await item.DownloadAsync(cancelToken, _pauseResetEvent, mirror, downloadSetting).ConfigureAwait(false);

                        if (!result.IsSuccess)
                        {
                            _errorList.Add(item, result.DownloadException);
                            SendDownloadErrLog(item, result.DownloadException);
                        }

                        item.ProgressCallback.IncreasedDoneSize -= ProgressCallback_IncreasedDoneSize;

                        item.ProgressCallback.SetDone();
                        CompletedOneTask(item);
                        if (cancelToken.IsCancellationRequested)
                        {
                            CompleteDownload();
                            break;
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                SendFatalLog(ex, "下载线程工作时发生异常");
            }

        }

        private void ProgressCallback_IncreasedDoneSize(object sender, long e)
        {
            this._downloadSizePerSec += e;
        }

        private void CompleteDownload()
        {
            ClearAllTasks();
            _timer.Stop();
            _downloadSizePerSec = 0;
            IsBusy = false;
            DownloadCompleted?.Invoke(this, new DownloadCompletedArg() { ErrorList = _errorList });
            Paused = false;
            ApendDebugLog("全部下载任务已完成");
        }

        private void ApendDebugLog(string msg)
        {
            this.DownloadLog?.Invoke(this, new Log(LogLevel.DEBUG, msg));
        }

        private void SendLog(Log e)
        {
            DownloadLog?.Invoke(this, e);
        }

        private void SendFatalLog(Exception ex, string msg)
        {
            SendLog(new Log(LogLevel.FATAL, msg, ex));
        }

        private void SendDownloadErrLog(IDownloadTask task, Exception ex)
        {
            SendLog(new Log(LogLevel.ERROR, string.Format("任务{0}下载失败,源地址:{1}错误:\n{2}",
                task.TaskName, task.DisplayFrom, ex?.ToString()), ex));
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _timer.Dispose();
            _cancellationTokenSource.Dispose();
            _pauseResetEvent.Dispose();
        }
    }

    public enum DownloadProtocolType
    {
        ORIGIN = 0,
        HTTP = 1,
        HTTPS = 2
    }
}