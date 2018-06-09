using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace NsisoLauncher.Core.Net
{
    public class DownloadTask : INotifyPropertyChanged
    {
        public DownloadTask(string name, string from, string to)
        {
            this.TaskName = name;
            this.From = from;
            this.To = to;
        }

        public string TaskName { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        private long _totalSize = 1;
        public long TotalSize
        {
            get { return _totalSize; }
            private set {
                _totalSize = value;
                OnPropertyChanged("TotalSize");
            }
        }

        private long _downloadSize = 0;
        public long DownloadSize
        {
            get { return _downloadSize; }
            private set {
                _downloadSize = value;
                OnPropertyChanged("DownloadSize");
            }
        }


        public void SetTotalSize(long size)
        {
            TotalSize = size;
        }

        public void IncreaseDownloadSize(long size)
        {
            DownloadSize += size;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string strPropertyInfo)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyInfo));
        }
    }
}
