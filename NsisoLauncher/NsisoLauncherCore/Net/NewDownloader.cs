using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;

namespace NsisoLauncherCore.Net
{
    //public class NewDownloader
    //{
    //    public ConcurrentQueue<DownloadTask> DownloadTaskQueue { get; private set; } = new ConcurrentQueue<DownloadTask>();
    //    public bool IsBusy { get; private set; }
    //    public int ProcessorSize { get; set; } = 5;

    //    //private Thread[] _threads;

    //    public NewDownloader()
    //    {
    //        #region 设置定时器
    //        _timer.Elapsed += _timer_Elapsed;
    //        #endregion
    //    }

    //    #region 开始下载任务

    //    #endregion

    //    #region 添加下载任务
    //    public void AppendDownloadTasks(DownloadTask task)
    //    {
    //        DownloadTaskQueue.Enqueue(task);
    //    }

    //    public void AppendDownloadTasks(List<DownloadTask> tasks)
    //    {
    //        foreach (var item in tasks)
    //        {
    //            DownloadTaskQueue.Enqueue(item);
    //        }
    //    }
    //    #endregion

    //    #region 定时器
    //    private System.Timers.Timer _timer = new System.Timers.Timer(1000);
    //    /// <summary>
    //    /// 每秒触发事件（下载速度计算）
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="e"></param>
    //    private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    //    {
            
    //    }
    //    #endregion
    //}
}
