using NsisoLauncher.Core.Net;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace NsisoLauncher.Utils
{
    public class MultiThreadDownloader
    {
        //public MultiThreadDownloader()
        //{
        //    //debug
        //    ProcessorSize = 3;
        //    List<DownloadTask> tasks = new List<DownloadTask>();
        //    for (int i = 0; i < 20; i++)
        //    {
        //        DownloadTask task = new DownloadTask() { From = i.ToString(), To = i.ToString() };
        //        tasks.Add(task);
        //    }
        //    Download(tasks);
        //}

        public AsyncObservableCollection<DownloadTask> TasksObservableCollection { get; private set; } = new AsyncObservableCollection<DownloadTask>();
        public int ProcessorSize { get; set; } = 3;
        public bool IsBusy { get; set; }
        private Thread[] _threads;

        public bool Download(List<DownloadTask> tasks)
        {
            if (!IsBusy)
            {
                if (ProcessorSize == 0)
                {
                    throw new ArgumentException("下载器的线程数不能为0");
                }
                foreach (var item in tasks)
                {
                    TasksObservableCollection.Add(item);
                }
                _threads = new Thread[ProcessorSize];
                var threadTask = Split(tasks, ProcessorSize);
                for (int i = 0; i < ProcessorSize; i++)
                {
                    List<DownloadTask> arg = threadTask[i];
                    _threads[i] = new Thread(() =>
                    {
                        foreach (var item in arg)
                        {
                            HTTPDownload(item);
                            TasksObservableCollection.Remove(item);
                        }
                    });
                    _threads[i].Start();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void HTTPDownload(DownloadTask task)
        {
            if (Path.IsPathRooted(task.To))
            {
                if (!Directory.Exists(Path.GetDirectoryName(task.To)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(task.To));
                }
            }
            FileStream fs = new FileStream(task.To, FileMode.Create);
            HttpWebRequest request = WebRequest.Create(task.From) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            task.SetTotalSize(response.ContentLength);
            Stream responseStream = response.GetResponseStream();
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);

            while (size > 0)
            {
                fs.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
                task.IncreaseDownloadSize(size);
            }
            fs.Close();
            responseStream.Close();
        }

        private List<List<DownloadTask>> Split(List<DownloadTask> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.ToList();
            return splits.ToList();
        }
    }
}
