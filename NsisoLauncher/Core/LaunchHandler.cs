using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NsisoLauncher.Core.LaunchException;
using System.Threading.Tasks;

namespace NsisoLauncher.Core
{
    public class LaunchHandler
    {
        private object launchLocker = new object();
        private object getVersionsLocker = new object();

        /// <summary>
        /// 启动器所处理的游戏根目录
        /// </summary>
        public string GameRootPath { get; set; }

        /// <summary>
        /// 启动器所使用JAVA
        /// </summary>
        public Java Java { get; set; }

        /// <summary>
        /// 是否开启版本隔离
        /// </summary>
        public bool VersionIsolation { get; set; }

        public event GameLogHandler GameLog;
        public event GameExitHandler GameExit;
        public delegate void GameLogHandler(object sender, string e);
        public delegate void GameExitHandler(object sender, int ret);

        private ArgumentsParser argumentsParser;
        private VersionReader versionReader;
        private AssetsReader assetsReader;

        public LaunchHandler(string gamepath, Java java, bool isversionIsolation)
        {
            this.GameRootPath = gamepath;
            this.Java = java;
            this.VersionIsolation = isversionIsolation;
            versionReader = new VersionReader(this);
            assetsReader = new AssetsReader(this);
            argumentsParser = new ArgumentsParser(this);
        }

        public async Task<LaunchResult> LaunchAsync(LaunchSetting setting)
        {
            var result = await Task.Factory.StartNew(() =>
            {
                if (setting.MaxMemory == 0)
                {
                    setting.MaxMemory = SystemTools.GetBestMemory(this.Java);
                }
                return Launch(setting);
            });
            return result;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.GameLog?.Invoke(this, e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.GameLog?.Invoke(this, e.Data);
        }

        public Assets GetAssets(Modules.Version version)
        {
            return assetsReader.GetAssets(version);
        }

        public async Task<Assets> GetAssetsAsync(Modules.Version version)
        {
            return await Task.Factory.StartNew(() =>
            {
                return assetsReader.GetAssets(version);
            });
            
        }

        private LaunchResult Launch(LaunchSetting setting)
        {
            try
            {
                if (setting.AuthenticateAccessToken == null || setting.AuthenticateUUID == null)
                {
                    return new LaunchResult(new ArgumentException("启动所需必要的验证参数为空"));
                }
                if (setting.Version == null)
                {
                    return new LaunchResult(new ArgumentException("启动所需必要的版本参数为空"));
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                string arg = argumentsParser.Parse(setting);

                #region 处理库文件
                foreach (var item in setting.Version.Natives)
                {
                    string nativePath = GetNativePath(item);
                    if (File.Exists(nativePath))
                    {
                        App.logHandler.AppendDebug(string.Format("检查并解压不存在的库文件:{0}", nativePath));
                        Unzip.UnZipFile(nativePath, GetGameVersionRootDir(setting.Version) + @"\$natives", item.Exclude);
                    }
                    else
                    {
                        return new LaunchResult(new NativeNotFoundException(item, nativePath));
                    }

                }
                #endregion

                ProcessStartInfo startInfo = new ProcessStartInfo(Java.Path, arg)
                { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = GetGameVersionRootDir(setting.Version) };
                var process = Process.Start(startInfo);
                sw.Stop();
                App.logHandler.AppendInfo(string.Format("成功启动游戏进程,总共用时:{0}ms", sw.ElapsedMilliseconds));

                Task.Factory.StartNew(() =>
                {
                    process.WaitForExit();
                    GameExit?.Invoke(this, process.ExitCode);
                });

                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                #region 配置文件
                App.config.MainConfig.History.LastLaunchUsingMs = sw.ElapsedMilliseconds;
                App.config.MainConfig.History.LastLaunchTime = DateTime.Now;
                App.config.MainConfig.History.LastLaunchVersion = setting.Version.ID;
                App.config.Save();
                #endregion

                return new LaunchResult() { Process = process, IsSuccess = true, LaunchArguments = arg };
            }
            catch (LaunchException.LaunchException ex)
            {
                return new LaunchResult(ex);
            }
            catch (Exception ex)
            {
                return new LaunchResult(ex);
            }
        }

        public async Task<List<Modules.Version>> GetVersionsAsync()
        {
            try
            {
                return await versionReader.GetVersionsAsync();
            }
            catch (Exception)
            {
                return new List<Modules.Version>();
            }
        }

        public string GetGameVersionRootDir(Modules.Version ver)
        {
            if (VersionIsolation)
            {
                return string.Format(@"{0}\versions\{1}", GameRootPath, ver.ID);
            }
            else
            {
                return GameRootPath;
            }

        }

        public string GetLibraryPath(Modules.Library lib)
        {
            return string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.jar",
                this.GameRootPath, lib.Package.Replace(".", @"\"), lib.Name, lib.Version);
        }

        public string GetNativePath(Native native)
        {
            return string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}-{4}.jar",
                this.GameRootPath, native.Package.Replace(".", @"\"), native.Name, native.Version, native.NativeSuffix);
        }

        public string GetJsonPath(string ID)
        {
            return string.Format(@"{0}\versions\{1}\{1}.json", this.GameRootPath, ID);
        }

        public string GetJarPath(Modules.Version ver)
        {
            if (ver.Jar!=null)
            {
                return string.Format(@"{0}\versions\{1}\{1}.jar", this.GameRootPath, ver.Jar);
            }
            else
            {
                return string.Format(@"{0}\versions\{1}\{1}.jar", this.GameRootPath, ver.ID);
            }
        }

        public string GetAssetsIndexPath(string assetsID)
        {
            return string.Format(@"{0}\assets\indexes\{1}.json", this.GameRootPath, assetsID);
        }

        public string GetAssetsPath(AssetsInfo assetsInfo)
        {
            return string.Format(@"{0}\assets\objects\{1}\{2}", this.GameRootPath, assetsInfo.Hash.Substring(0, 2), assetsInfo.Hash);
        }

        public string GetVersionOptions(Modules.Version version)
        {
            return GetVersionOptions(version.ID);
        }

        public string GetVersionOptions(string versionId)
        {
            return string.Format(@"{0}\versions\{1}\options.txt", this.GameRootPath, versionId);
        }
    }
}
