using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncher.Test.Net
{
    //[TestClass]
    //public class DownloadTest
    //{
    //    [TestMethod]
    //    public async Task DownloadManyLargeFiles()
    //    {
    //        var download = new MultiThreadDownloader(new NetRequester());

    //        for (var i = 0; i < 50; ++i)
    //        {
    //            download.AddDownloadTask(new DownloadTask($"mcfile{i}", new StringUrl("http://n1.akkocloud.com:34000/download/3e8912b293b52bd54f879919bd6e4e44?name=server.jar"),
    //                $"test{i}.bin"));
    //        }
    //        var result = false;
    //        download.DownloadCompleted += (sender, arg) =>
    //        {
    //            result = true;
    //        };
    //        await download.StartDownload();
    //        while (true)
    //        {
    //            if (!result)
    //                await Task.Delay(10);
    //            else
    //                break;
    //        }
    //    }

    //}
}
