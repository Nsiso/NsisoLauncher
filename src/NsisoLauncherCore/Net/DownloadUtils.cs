using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Mirrors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public static class DownloadUtils
    {
        public static DownloadResult SimpleDownload(DownloadObject obj, IDownloadableMirror mirror)
        {
            try
            {
                if (obj == null || obj.Downloadable == null)
                {
                    return new DownloadResult() { IsSuccess = true, ObjectToDownload = obj };
                }
                string from = obj.Downloadable.GetDownloadSourceURL();
                if (mirror != null)
                {
                    from = mirror.DoDownloadUriReplace(from);
                }
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(obj.Downloadable.GetDownloadSourceURL(), obj.To);
                }
                #region 下载后校验
                if (obj.Checker != null)
                {
                    if (!obj.Checker.CheckFilePass())
                    {
                        return new DownloadResult()
                        {
                            IsSuccess = false,
                            ObjectToDownload = obj,
                            DownloadException = new Exception(string.Format("{0}校验哈希值失败，目标哈希值:{1}", obj.To, obj.Checker.CheckSum))
                        };
                    }
                }
                #endregion
                return new DownloadResult() { IsSuccess = true, ObjectToDownload = obj };
            }
            catch (Exception ex)
            {
                return new DownloadResult()
                {
                    IsSuccess = false,
                    ObjectToDownload = obj,
                    DownloadException = ex
                };
            }
        }

        public static async Task<DownloadResult> DownloadAsync(DownloadObject obj, CancellationToken cancellationToken, ManualResetEventSlim manualResetEvent,
            ProgressCallback progressCallback, IDownloadableMirror mirror, DownloadSetting downloadSetting)
        {
            #region 检查
            if (obj == null)
            {
                throw new ArgumentException("The download object is null");
            }
            if (obj.Downloadable == null)
            {
                throw new ArgumentException("The download object's property: Downloadable is null, where to download?");
            }
            if (obj.To == null)
            {
                throw new ArgumentException("The download object's property: To is null, where to store the download file?");
            }
            #endregion

            DownloadResult downloadResult = new DownloadResult() { IsSuccess = false, ObjectToDownload = obj };

            string from = obj.Downloadable.GetDownloadSourceURL();

            #region 检查
            if (obj == null)
            {
                throw new ArgumentNullException("Download object is null");
            }
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(obj.To))
            {
                throw new ArgumentException("Download necessary argument is empty (From or to), can't download");
            }
            if (downloadSetting == null)
            {
                throw new ArgumentNullException("Download setting is null");
            }
            #endregion

            progressCallback.State = "准备下载";

            string realFilename = obj.To;
            string buffFilename = realFilename + ".downloadtask";
            Exception exception = null;

            #region 镜像站替换URL处理
            if (mirror != null)
            {
                from = mirror.DoDownloadUriReplace(from);
                progressCallback.ChangeMirror(mirror);
            }
            else
            {
                progressCallback.DownloadSourceName = "Origin";
            }

            UriBuilder fromUriBuilder = new UriBuilder(from);
            switch (downloadSetting.ProtocolType)
            {
                case DownloadProtocolType.ORIGIN:
                    break;
                case DownloadProtocolType.HTTP:
                    fromUriBuilder.Scheme = "http";
                    break;
                case DownloadProtocolType.HTTPS:
                    fromUriBuilder.Scheme = "https";
                    break;
                default:
                    break;
            }
            #endregion

            progressCallback.ChangeDownloadFrom(fromUriBuilder.Uri);

            for (int i = 1; i <= downloadSetting.RetryTimes; i++)
            {
                try
                {
                    #region 下载前文件准备
                    if (Path.IsPathRooted(realFilename))
                    {
                        string dirName = Path.GetDirectoryName(realFilename);
                        if (!Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                        }
                    }
                    if (File.Exists(realFilename))
                    {
                        if (downloadSetting.CheckFileHash && obj.Checker != null)
                        {
                            progressCallback.State = "校验中";
                            if (await obj.Checker.CheckFilePassAsync())
                            {
                                downloadResult.IsSuccess = true;
                                return downloadResult;
                            }
                        }
                        else
                        {
                            File.Delete(realFilename);
                        }
                    }
                    if (File.Exists(buffFilename))
                    {
                        File.Delete(buffFilename);
                    }
                    #endregion

                    #region 下载流程
                    progressCallback.State = "下载中（等待Get回应）";
                    using (var getResult = await NetRequester.HttpGetAsync(fromUriBuilder.Uri,
                        HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                    {
                        getResult.EnsureSuccessStatusCode();
                        progressCallback.State = "下载中";
                        progressCallback.TotalSize = getResult.Content.Headers.ContentLength.GetValueOrDefault();
                        using (Stream responseStream = await getResult.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            using (FileStream fs = new FileStream(buffFilename, FileMode.Create))
                            {
                                byte[] bArr = new byte[4096]; //4k
                                int size = await responseStream.ReadAsync(bArr, 0, bArr.Length, cancellationToken).ConfigureAwait(false);

                                while (size > 0)
                                {
                                    manualResetEvent.Wait();
                                    await fs.WriteAsync(bArr, 0, size, cancellationToken).ConfigureAwait(false);
                                    size = await responseStream.ReadAsync(bArr, 0, bArr.Length, cancellationToken).ConfigureAwait(false);
                                    progressCallback.IncreaseDoneSize(size);
                                }
                            }
                        }
                    }
                    #endregion

                    //下载完成后转正
                    File.Move(buffFilename, realFilename);

                    #region 下载后校验
                    if (downloadSetting.CheckFileHash && obj.Checker != null)
                    {
                        progressCallback.State = "校验中";
                        if (!await obj.Checker.CheckFilePassAsync().ConfigureAwait(false))
                        {
                            progressCallback.State = "校验失败";
                            downloadResult.DownloadException = new Exception(string.Format("{0}校验哈希值失败，目标哈希值:{1}", obj.To, obj.Checker.CheckSum));
                        }
                        else
                        {
                            progressCallback.State = "校验成功";
                        }
                    }
                    #endregion

                    //无错误跳出重试循环
                    exception = null;
                    break;
                }
                catch (Exception e)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        downloadResult.IsSuccess = true;
                        break;
                    }
                    exception = e;
                    progressCallback.State = string.Format("重试第{0}次", i);

                    //继续重试
                    continue;
                }
            }

            //处理异常
            if (exception != null)
            {
                downloadResult.DownloadException = exception;
                return downloadResult;
            }

            #region 执行下载后函数
            if (obj.Todo != null)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    progressCallback.State = "安装中";

                    try
                    {
                        Exception exc = await Task.Run(() => obj.Todo(progressCallback, cancellationToken)).ConfigureAwait(false);
                        if (exc != null)
                        {
                            downloadResult.DownloadException = exc;
                            return downloadResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        downloadResult.DownloadException = ex;
                        return downloadResult;
                    }
                }
            }
            #endregion

            progressCallback.SetDone();

            downloadResult.IsSuccess = true;
            return downloadResult;
        }

        public static Exception DownloadForgeJLibraries(ProgressCallback monitor, IDownloadableMirror mirror, CancellationToken cancelToken, List<Library> libs, string librariesDir)
        {
            try
            {
                foreach (var item in libs)
                {
                    monitor.DoneSize = 0;
                    monitor.State = string.Format("补全库文件{0}", item.Name);
                    Exception exception = null;
                    for (int i = 1; i <= 3; i++)
                    {
                        try
                        {

                            string from = item.Downloads.Artifact.Url;
                            if (mirror != null)
                            {
                                from = mirror.DoDownloadUriReplace(from);
                            }
                            string to = Path.Combine(librariesDir, item.Downloads.Artifact.Path);
                            string buffFilename = to + ".downloadtask";

                            if (File.Exists(to))
                            {
                                continue;
                            }
                            if (string.IsNullOrWhiteSpace(from))
                            {
                                continue;
                            }
                            if (Path.IsPathRooted(to))
                            {
                                string dirName = Path.GetDirectoryName(to);
                                if (!Directory.Exists(dirName))
                                {
                                    Directory.CreateDirectory(dirName);
                                }
                            }
                            if (File.Exists(buffFilename))
                            {
                                File.Delete(buffFilename);
                            }

                            HttpWebRequest request = WebRequest.Create(from) as HttpWebRequest;
                            cancelToken.Register(() => { request.Abort(); });
                            request.Timeout = 5000;
                            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                            {
                                monitor.TotalSize = response.ContentLength;
                                using (Stream responseStream = response.GetResponseStream())
                                {
                                    responseStream.ReadTimeout = 5000;
                                    using (FileStream fs = new FileStream(buffFilename, FileMode.Create))
                                    {
                                        byte[] bArr = new byte[1024];
                                        int size = responseStream.Read(bArr, 0, (int)bArr.Length);

                                        while (size > 0)
                                        {
                                            if (cancelToken.IsCancellationRequested)
                                            {
                                                return null;
                                            }
                                            fs.Write(bArr, 0, size);
                                            size = responseStream.Read(bArr, 0, (int)bArr.Length);
                                            monitor.IncreaseDoneSize(size);
                                        }
                                    }
                                }
                            }

                            //下载完成后转正
                            File.Move(buffFilename, to);
                            monitor.SetDone();

                            break;
                        }
                        catch (Exception e)
                        {
                            exception = e;
                            monitor.State = string.Format("重试第{0}次", i);

                            //继续重试
                            continue;
                        }

                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }


    public class DownloadResult
    {
        public bool IsSuccess { get; set; }

        public Exception DownloadException { get; set; }

        public DownloadObject ObjectToDownload { get; set; }
    }
}
