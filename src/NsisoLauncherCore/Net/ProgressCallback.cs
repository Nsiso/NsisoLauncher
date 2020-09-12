using NsisoLauncherCore.Net.Mirrors;
using System;
using System.ComponentModel;

namespace NsisoLauncherCore.Net
{
    public class ProgressCallback : INotifyPropertyChanged
    {
        #region 属性

        private long _totalSize = 1;
        /// <summary>
        /// 总大小
        /// </summary>
        public long TotalSize
        {
            get { return _totalSize; }
            set
            {
                _totalSize = value;
                this.RaisePropertyChangedEvent("TotalSize");
            }
        }

        private long _doneSize = 0;
        /// <summary>
        /// 完成的大小
        /// </summary>
        public long DoneSize
        {
            get { return _doneSize; }
            set
            {
                _doneSize = value;
                this.RaisePropertyChangedEvent("DoneSize");
            }
        }

        private string _state;
        /// <summary>
        /// 状态
        /// </summary>
        public string State
        {
            get { return _state; }
            set
            {
                _state = value;
                this.RaisePropertyChangedEvent("State");
            }
        }

        private string _downloadFrom;
        /// <summary>
        /// 当前下载来源
        /// </summary>
        public string DownloadFrom
        {
            get { return _downloadFrom; }
            set
            {
                _downloadFrom = value;
                this.RaisePropertyChangedEvent("DownloadFrom");
            }
        }

        private string _downloadSourceName;
        /// <summary>
        /// 使用下载源名称
        /// </summary>
        public string DownloadSourceName
        {
            get { return _downloadSourceName; }
            set 
            { 
                _downloadSourceName = value;
                this.RaisePropertyChangedEvent("DownloadSourceName");
            }
        }
        #endregion

        #region 设置属性方法

        public void IncreaseDoneSize(long size)
        {
            DoneSize += size;
            this.IncreasedDoneSize?.Invoke(this, size);
        }

        public void SetDone()
        {
            DoneSize = TotalSize;
            State = "已完成";
        }

        public void ChangeMirror(IMirror mirror)
        {
            this.DownloadSourceName = mirror.MirrorName;
        }

        public void ChangeDownloadFrom(Uri uri)
        {
            this.DownloadFrom = uri.ToString();
        }
        #endregion

        public void RaisePropertyChangedEvent(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<long> IncreasedDoneSize;
    }
}
