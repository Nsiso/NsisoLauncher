using NsisoLauncherCore.Util.Checker;
using System;
using System.ComponentModel;

namespace NsisoLauncherCore.Net
{
    public class DownloadTask : INotifyPropertyChanged
    {
        public DownloadTask(string name, string from, string to)
        {
            this.TaskName = name;
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// 任务下载来源URL
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 下载到路径
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 下载完成后执行方法
        /// </summary>
        public Func<Exception> Todo { get; set; }

        /// <summary>
        /// 校验器，不设置即不校验
        /// </summary>
        public IChecker Checker { get; set; }

        #region 界面绑定属性

        private long _totalSize = 1;
        /// <summary>
        /// 文件总大小
        /// </summary>
        public long TotalSize
        {
            get { return _totalSize; }
            private set
            {
                _totalSize = value;
                OnPropertyChanged("TotalSize");
            }
        }

        private long _downloadSize = 0;
        /// <summary>
        /// 已下载大小
        /// </summary>
        public long DownloadSize
        {
            get { return _downloadSize; }
            private set
            {
                _downloadSize = value;
                OnPropertyChanged("DownloadSize");
            }
        }

        private string _state;
        /// <summary>
        /// 任务状态
        /// </summary>
        public string State
        {
            get { return _state; }
            private set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        #endregion

        #region 设置属性方法
        public void SetTotalSize(long size)
        {
            TotalSize = size;
        }

        public void IncreaseDownloadSize(long size)
        {
            DownloadSize += size;
        }

        public void SetDone()
        {
            DownloadSize = TotalSize;
            State = "已完成";
        }

        public void SetState(string state)
        {
            State = state;
        }
        #endregion

        #region 属性更改通知事件(base)
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string strPropertyInfo)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyInfo));
        }
        #endregion
    }

    //public class DownloadFrom
    //{
    //    public string OriginUrl { get; set; }
    //    public DownloadFrom(string originUrl)
    //    {
    //        this.OriginUrl = originUrl;
    //    }

    //    public string GetDownloadUrl(DownloadSource source)
    //    {
    //        return Tools.GetDownloadUrl.DoURLReplace(source, OriginUrl);
    //    }
    //}
}
