using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace NsisoLauncherCore.Net.Tools
{
    public class DownloadUtils
    {
        public static Exception DownloadForgeJLibraries(ProgressCallback monitor, DownloadSource source, CancellationToken cancelToken, List<JLibrary> libs, string librariesDir)
        {
            try
            {
                foreach (var item in libs)
                {
                    monitor.SetDoneSize(0);
                    monitor.SetState(string.Format("补全库文件{0}", item.Name));
                    Exception exception = null;
                    for (int i = 1; i <= 3; i++)
                    {
                        try
                        {
                            string from = GetDownloadUrl.DoURLReplace(source, item.Downloads.Artifact.URL);
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
                                monitor.SetTotalSize(response.ContentLength);
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
                            monitor.SetState(string.Format("重试第{0}次", i));

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
}
