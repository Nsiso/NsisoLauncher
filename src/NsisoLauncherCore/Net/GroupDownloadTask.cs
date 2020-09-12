using NsisoLauncherCore.Net.Mirrors;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public class GroupDownloadTask : IDownloadTask, INotifyPropertyChanged
    {
        public string TaskName { get; set; }
        public long TotalSize { get; set; }
        public long DownloadedSize { get; set; }
        public string State { get; set; }
        public string DisplayFrom { get; set; }
        public string DisplayTo { get; set; }
        public string DisplayDownloadSourceName { get; set; }
        public string DisplayOriginFrom { get; set; }
        public ProgressCallback ProgressCallback { get; set; }

        public IEnumerable<DownloadObject> DownloadObjectsGroup { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public GroupDownloadTask(string taskName, IEnumerable<DownloadObject> objects, long totalSize)
        {
            this.TaskName = taskName;
            this.DownloadObjectsGroup = objects;
            this.TotalSize = totalSize;
            ProgressCallback = new ProgressCallback();
        }

        public async Task<DownloadResult> DownloadAsync(NetRequester requester, CancellationToken cancellationToken, IDownloadableMirror mirror, DownloadSetting downloadSetting)
        {
            ProgressCallback.PropertyChanged += ProgressCallback_PropertyChanged;
            ProgressCallback.IncreasedDoneSize += ProgressCallback_IncreasedDoneSize;
            DownloadResult downloadResult = new DownloadResult();
            foreach (var item in DownloadObjectsGroup)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var itemResult = await DownloadUtils.DownloadAsync(item, requester, cancellationToken, ProgressCallback, mirror, downloadSetting);
                if (!itemResult.IsSuccess)
                {
                    downloadResult.IsSuccess = false;
                    downloadResult.DownloadException = itemResult.DownloadException;
                    downloadResult.ObjectToDownload = item;
                }
            }
            ProgressCallback.SetDone();
            ProgressCallback.PropertyChanged -= ProgressCallback_PropertyChanged;
            ProgressCallback.IncreasedDoneSize -= ProgressCallback_IncreasedDoneSize;
            return downloadResult;
        }

        private void ProgressCallback_IncreasedDoneSize(object sender, long e)
        {
            this.DownloadedSize += e;
        }

        private void ProgressCallback_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
}
