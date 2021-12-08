using NsisoLauncherCore.Net.Mirrors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public class ActionDownloadTask : IDownloadTask
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

        public Action Todo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public ActionDownloadTask(string taskName, Action todo)
        {
            TaskName = taskName;
            Todo = todo;

            ProgressCallback = new ProgressCallback();
        }

        public ActionDownloadTask(string taskName, Action todo, string to)
        {
            TaskName = taskName;
            Todo = todo;
            DisplayTo = to;

            ProgressCallback = new ProgressCallback();
        }

        public async Task<DownloadResult> DownloadAsync(CancellationToken cancellationToken, ManualResetEventSlim manualResetEvent, IDownloadableMirror mirror, DownloadSetting downloadSetting)
        {
            this.ProgressCallback.State = "执行中";
            try
            {
                await Task.Run(this.Todo);
                this.ProgressCallback.SetDone();
                return new DownloadResult() { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new DownloadResult() { DownloadException = ex, IsSuccess = false };
            }
        }
    }
}
