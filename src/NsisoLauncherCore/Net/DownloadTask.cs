using NsisoLauncherCore.Net.Mirrors;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public class DownloadTask : INotifyPropertyChanged, IDownloadTask
    {
        public string TaskName { get; set; }
        public long TotalSize { get; set; } = 1;
        public long DownloadedSize { get; set; } = 0;
        public string State { get; set; }
        public string DisplayFrom { get; set; }
        public string DisplayTo { get; set; }
        public string DisplayDownloadSourceName { get; set; }
        public string DisplayOriginFrom { get; set; }
        public ProgressCallback ProgressCallback { get; set; }

        public DownloadObject DownloadObject { get; set; }

        public DownloadTask(string taskName, DownloadObject obj)
        {
            TaskName = taskName;
            DownloadObject = obj;
            DisplayOriginFrom = obj.OriginFrom;
            DisplayTo = obj.To;
            ProgressCallback = new ProgressCallback();
        }

        public DownloadTask(string taskName, IDownloadable downloadable, string to)
        {
            TaskName = taskName;
            DownloadObject obj = new DownloadObject(downloadable, to);
            DownloadObject = obj;
            DisplayOriginFrom = obj.OriginFrom;
            DisplayTo = obj.To;
            ProgressCallback = new ProgressCallback();
        }
        #region 属性更改通知事件(base)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public async Task<DownloadResult> DownloadAsync(CancellationToken cancellationToken, ManualResetEventSlim manualResetEvent, IDownloadableMirror mirror, DownloadSetting downloadSetting)
        {
            ProgressCallback.PropertyChanged += ProgressCallback_PropertyChanged;
            var result = await DownloadUtils.DownloadAsync(DownloadObject, cancellationToken, manualResetEvent, ProgressCallback, mirror, downloadSetting);
            ProgressCallback.PropertyChanged -= ProgressCallback_PropertyChanged;
            return result;
        }

        private void ProgressCallback_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TotalSize":
                    TotalSize = ProgressCallback.TotalSize;
                    break;
                case "DoneSize":
                    DownloadedSize = ProgressCallback.DoneSize;
                    break;
                case "State":
                    State = ProgressCallback.State;
                    break;
                case "DownloadFrom":
                    DisplayFrom = ProgressCallback.DownloadFrom;
                    break;
                case "DownloadSourceName":
                    DisplayDownloadSourceName = ProgressCallback.DownloadSourceName;
                    break;
                default:
                    break;
            }
        }
    }



    //public class DownloadInfo
    //{
    //    public DownloadInfo(string from, string to)
    //    {
    //        this.From = from;
    //        this.To = to;
    //    }

    //    /// <summary>
    //    /// 任务下载来源URL
    //    /// </summary>
    //    public string From { get; set; }

    //    /// <summary>
    //    /// 下载到路径
    //    /// </summary>
    //    public string To { get; set; }

    //    /// <summary>
    //    /// 下载完成后执行方法
    //    /// </summary>
    //    public Func<Exception> Todo { get; set; }

    //    /// <summary>
    //    /// 校验器，不设置即不校验
    //    /// </summary>
    //    public IChecker Checker { get; set; }
    //}

    //public interface IDownloadTask
    //{
    //    /// <summary>
    //    /// 任务名称
    //    /// </summary>
    //    string TaskName { get; set; }

    //    int DownloadCount { get; }

    //    string UIFrom { get; set; }
    //    string UITo { get; set; }

    //    long TotalSize { get; set; }
    //    void SetTotalSize(long size);

    //    long DownloadSize { get; set; }
    //    void IncreaseDownloadSize(long size);

    //    string State { get; set; }
    //    void SetState(string state);

    //    void SetDone();

    //    List<DownloadInfo> GetDownloadInfo();
    //}

    //public class DownloadTask : IDownloadTask, INotifyPropertyChanged
    //{
    //    public DownloadTask(string name, string from, string to)
    //    {
    //        this.TaskName = name;
    //        Info = new DownloadInfo(from, to);
    //        UIFrom = from;
    //        UITo = to;
    //    }

    //    public DownloadInfo Info { get; set; }

    //    /// <summary>
    //    /// 任务名称
    //    /// </summary>
    //    public string TaskName { get; set; }

    //    /// <summary>
    //    /// 下载计数
    //    /// </summary>
    //    public int DownloadCount {
    //        get => 1;
    //    }

    //    public List<DownloadInfo> GetDownloadInfo()
    //    {
    //        List<DownloadInfo> info = new List<DownloadInfo>(1);
    //        info.Add(Info);
    //        return info;
    //    }

    //    #region 界面绑定属性

    //    private long _totalSize = 1;
    //    /// <summary>
    //    /// 文件总大小
    //    /// </summary>
    //    public long TotalSize
    //    {
    //        get { return _totalSize; }
    //        set
    //        {
    //            _totalSize = value;
    //            OnPropertyChanged("TotalSize");
    //        }
    //    }

    //    private long _downloadSize = 0;
    //    /// <summary>
    //    /// 已下载大小
    //    /// </summary>
    //    public long DownloadSize
    //    {
    //        get { return _downloadSize; }
    //        set
    //        {
    //            _downloadSize = value;
    //            OnPropertyChanged("DownloadSize");
    //        }
    //    }

    //    private string _state;
    //    /// <summary>
    //    /// 任务状态
    //    /// </summary>
    //    public string State
    //    {
    //        get { return _state; }
    //        set
    //        {
    //            _state = value;
    //            OnPropertyChanged("State");
    //        }
    //    }

    //    private string uifrom;
    //    /// <summary>
    //    /// 显示在UI上的来源
    //    /// </summary>
    //    public string UIFrom
    //    {
    //        get { return uifrom; }
    //        set 
    //        {
    //            uifrom = value;
    //            OnPropertyChanged("UIFrom");
    //        }
    //    }

    //    private string uito;
    //    /// <summary>
    //    /// 显示在UI上的来源
    //    /// </summary>
    //    public string UITo
    //    {
    //        get { return uito; }
    //        set
    //        {
    //            uito = value;
    //            OnPropertyChanged("UITo");
    //        }
    //    }


    //    #endregion

    //    #region 设置属性方法
    //    public void SetTotalSize(long size)
    //    {
    //        TotalSize = size;
    //    }

    //    public void IncreaseDownloadSize(long size)
    //    {
    //        DownloadSize += size;
    //    }

    //    public void SetDone()
    //    {
    //        DownloadSize = TotalSize;
    //        State = "已完成";
    //    }

    //    public void SetState(string state)
    //    {
    //        State = state;
    //    }
    //    #endregion

    //    #region 属性更改通知事件(base)
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void OnPropertyChanged(string strPropertyInfo)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyInfo));
    //    }
    //    #endregion
    //}

    //public class DownloadTaskGroup : IDownloadTask, INotifyPropertyChanged
    //{
    //    public DownloadTaskGroup(string name, List<DownloadInfo>info, long totalSize)
    //    {
    //        this.TaskName = name;
    //        this.InfoGroup = info;
    //        this.TotalSize = totalSize;
    //    }

    //    public List<DownloadInfo> InfoGroup { get; set; }

    //    /// <summary>
    //    /// 任务名称
    //    /// </summary>
    //    public string TaskName { get; set; }

    //    /// <summary>
    //    /// 下载计数
    //    /// </summary>
    //    public int DownloadCount
    //    {
    //        get => InfoGroup.Count;
    //    }

    //    public List<DownloadInfo> GetDownloadInfo()
    //    {
    //        return InfoGroup;
    //    }

    //    #region 界面绑定属性

    //    private long _totalSize = 1;
    //    /// <summary>
    //    /// 文件总大小
    //    /// </summary>
    //    public long TotalSize
    //    {
    //        get { return _totalSize; }
    //        set
    //        {
    //            _totalSize = value;
    //            OnPropertyChanged("TotalSize");
    //        }
    //    }

    //    private long _downloadSize = 0;
    //    /// <summary>
    //    /// 已下载大小
    //    /// </summary>
    //    public long DownloadSize
    //    {
    //        get { return _downloadSize; }
    //        set
    //        {
    //            _downloadSize = value;
    //            OnPropertyChanged("DownloadSize");
    //        }
    //    }

    //    private string _state;
    //    /// <summary>
    //    /// 任务状态
    //    /// </summary>
    //    public string State
    //    {
    //        get { return _state; }
    //        set
    //        {
    //            _state = value;
    //            OnPropertyChanged("State");
    //        }
    //    }

    //    private string uifrom;
    //    /// <summary>
    //    /// 显示在UI上的来源
    //    /// </summary>
    //    public string UIFrom
    //    {
    //        get { return uifrom; }
    //        set
    //        {
    //            uifrom = value;
    //            OnPropertyChanged("UIFrom");
    //        }
    //    }

    //    private string uito;
    //    /// <summary>
    //    /// 显示在UI上的来源
    //    /// </summary>
    //    public string UITo
    //    {
    //        get { return uito; }
    //        set
    //        {
    //            uito = value;
    //            OnPropertyChanged("UITo");
    //        }
    //    }

    //    #endregion

    //    #region 设置属性方法
    //    public void SetTotalSize(long size)
    //    {
    //        TotalSize = size;
    //    }

    //    public void IncreaseDownloadSize(long size)
    //    {
    //        DownloadSize += size;
    //    }

    //    public void SetDone()
    //    {
    //        DownloadSize = TotalSize;
    //        State = "已完成";
    //    }

    //    public void SetState(string state)
    //    {
    //        State = state;
    //    }
    //    #endregion

    //    #region 属性更改通知事件(base)
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void OnPropertyChanged(string strPropertyInfo)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyInfo));
    //    }
    //    #endregion
    //}
}
