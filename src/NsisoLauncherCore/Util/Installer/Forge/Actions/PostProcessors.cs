using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util.Installer.Forge.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

/* 项目“NsisoLauncher.Test”的未合并的更改
在此之前:
using static NsisoLauncherCore.PathManager;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
在此之后:
using static NsisoLauncherCore.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static NsisoLauncherCore.PathManager;
using static NsisoLauncherCore.Util.Installer.Forge.Text.RegularExpressions;
*/
using System.Text;
using System.Text.RegularExpressions;
using static NsisoLauncherCore.PathManager;
using static NsisoLauncherCore.Util.Installer.Forge.Json.Install;

namespace NsisoLauncherCore.Util.Installer.Forge.Actions
{
    public class PostProcessors
    {
        public Install Profile { get; set; }
        public bool IsClient { get; set; }
        public ProgressCallback Monitor { get; set; }
        public CommonInstallOptions Option { get; set; }
        public bool HasTasks { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<Processor> Processors { get; set; }

        public PostProcessors(Install profile, bool isClient, ProgressCallback monitor, CommonInstallOptions option)
        {
            this.Profile = profile;
            this.IsClient = isClient;
            this.Monitor = monitor;
            this.Option = option;

            this.Processors = profile.Processors;
            this.HasTasks = this.Processors.Count != 0;
            this.Data = new Dictionary<string, string>();
        }
        public Exception Process(string installer_path, string temp, string gamerootPath, string minecraft, Java java)
        {
            try
            {
                int progress = 1;
                Dictionary<string, string> originalData = Profile.GetData(IsClient);
                if (originalData.Count != 0)
                {
                    Monitor.TotalSize = originalData.Count;

                    foreach (var item in originalData)
                    {
                        Monitor.IncreaseDoneSize(progress++);

                        if (item.Value[0] == '[' && item.Value[item.Value.Length - 1] == ']')
                        { //Artifact
                            Data.Add(item.Key, GetLibraryPath(gamerootPath, item.Value.Substring(1, item.Value.Length - 2)));
                        }
                        else if (item.Value[0] == '\'' && item.Value[item.Value.Length - 1] == '\'')
                        { //Literal
                            Data.Add(item.Key, item.Value.Substring(1, item.Value.Length - 2));
                        }
                        else
                        {
                            string target = temp + item.Value;
                            Monitor.State = string.Format("Extracting:{0}", item.Value);
                            Data.Add(item.Key, target);
                        }
                    }
                }
                Data.Add("SIDE", IsClient ? "client" : "server");
                Data.Add("MINECRAFT_JAR", minecraft);
                Data.Add("ROOT", gamerootPath);
                Data.Add("INSTALLER", installer_path);

                Monitor.TotalSize = Processors.Count;
                Monitor.DoneSize = 0;
                Monitor.State = "Building Processors";

                foreach (var proc in Processors)
                {
                    //// WARNING! HARD INJECT!
                    //#region HARD INJECT
                    //try
                    //{
                    //    if (proc.Args.Contains("DOWNLOAD_MOJMAPS"))
                    //    {
                    //        Monitor.State = string.Format("接管{0}安装器mappings下载", proc.Jar.Name);
                    //        var download_result = DownloadUtils.SimpleDownload(
                    //            new DownloadObject(Option.VersionToInstall.Downloads.ClientMappings, Data["MOJMAPS"]), Option.Mirror);
                    //        if (download_result.IsSuccess)
                    //        {
                    //            continue;
                    //        }
                    //    }
                    //}
                    //catch (Exception)
                    //{
                    //    // if any error, let this processor run.
                    //}
                    //#endregion


                    if (proc.Sides != null && !proc.Sides.Contains("client"))
                    {
                        continue;
                    }

                    string jarPath = GetLibraryPath(gamerootPath, proc.Jar);
                    if (!File.Exists(jarPath))
                    {
                        return new FileNotFoundException("Jar file can not found", jarPath);
                    }

                    string jarTemp = Path.GetDirectoryName(jarPath);
                    Unzip.UnZipFile(jarPath, jarTemp);

                    string MetaPath = jarTemp + @"\META-INF\MANIFEST.MF";
                    if (!File.Exists(MetaPath))
                    {
                        return new FileNotFoundException("META-INF file can not found", MetaPath);
                    }

                    string metaContent = File.ReadAllText(MetaPath);
                    string mainClass = Regex.Match(metaContent, @"(?M)^Main-Class: (.*)$").Groups[1].Value.Trim();

                    StringBuilder argBuilder = new StringBuilder();

                    argBuilder.Append("-cp \"");
                    foreach (var item in proc.ClassPath)
                    {
                        argBuilder.Append(GetLibraryPath(gamerootPath, item)).Append(';');
                    }
                    argBuilder.Append(jarPath).Append("\" ");

                    argBuilder.Append(mainClass).Append(' ');

                    foreach (var item in proc.Args)
                    {
                        char start = item[0];
                        char end = item[item.Length - 1];

                        if (start == '[' && end == ']') //Library
                        {
                            argBuilder.Append("\"").Append(GetLibraryPath(gamerootPath, item.Substring(1, item.Length - 2))).Append("\" ");
                        }
                        else
                        {
                            string value = ReplaceByDic(item, Data);
                            if (value != null)
                            {
                                argBuilder.Append("\"").Append(value).Append("\" ");
                            }
                        }
                    }

                    ProcessStartInfo processStartInfo = new ProcessStartInfo(java.Path, argBuilder.ToString())
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    Monitor.State = string.Format("执行安装器{0}", proc.Jar.Name);
                    var result = System.Diagnostics.Process.Start(processStartInfo);
                    result.BeginOutputReadLine();
                    result.OutputDataReceived += Result_OutputDataReceived;
                    result.WaitForExit();

                    if (result.ExitCode != 0)
                    {
                        throw new Exception("Processor exited withou code zero. Please try to install other version");
                    }

                    Monitor.IncreaseDoneSize(1);
                }

                Monitor.SetDone();
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return e;
            }
        }

        private void Result_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static string ReplaceByDic(string str, Dictionary<string, string> dic)
        {
            if (str == null)
            {
                return null;
            }
            return dic.Aggregate(str, (a, b) =>
            {
                string item = string.Format("{{{0}}}", b.Key);
                return a.Replace(item, b.Value);
            });
        }
    }
}
