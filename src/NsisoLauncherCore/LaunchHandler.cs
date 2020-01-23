using NsisoLauncherCore.LaunchException;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncherCore
{
    public class LaunchHandler
    {
        private readonly object launchLocker = new object();

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

        /// <summary>
        /// 是否在处理启动
        /// </summary>
        public bool IsBusyLaunching { get; private set; } = false;

        public event GameLogHandler GameLog;
        public event GameExitHandler GameExit;
        public event LaunchLogHandler LaunchLog;
        public delegate void GameLogHandler(object sender, string e);
        public delegate void GameExitHandler(object sender, GameExitArg arg);
        public delegate void LaunchLogHandler(object sender, Log log);

        private ArgumentsParser argumentsParser;
        private VersionReader versionReader;
        private AssetsReader assetsReader;

        public LaunchHandler() : this(PathManager.CurrentLauncherDirectory + "\\.minecraft", Java.GetSuitableJava(), false) { }

        public LaunchHandler(string gamepath, Java java, bool isversionIsolation)
        {
            this.GameRootPath = gamepath;
            this.Java = java;
            this.VersionIsolation = isversionIsolation;

            versionReader = new VersionReader(this);
            versionReader.VersionReaderLog += (s, l) => LaunchLog?.Invoke(s, l);

            argumentsParser = new ArgumentsParser(this);
            argumentsParser.ArgumentsParserLog += (s, l) => LaunchLog?.Invoke(s, l);

            assetsReader = new AssetsReader(this);
        }

        public async Task<LaunchResult> LaunchAsync(LaunchSetting setting)
        {
            var result = await Task.Factory.StartNew(() =>
            {
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

        public JAssets GetAssets(Modules.Version version)
        {
            return assetsReader.GetAssets(version);
        }

        public JAssets GetAssetsByJson(string json)
        {
            return assetsReader.GetAssetsByJson(json);
        }

        public async Task<JAssets> GetAssetsAsync(Modules.Version version)
        {
            return await Task.Factory.StartNew(() =>
            {
                return assetsReader.GetAssets(version);
            });

        }

        #region 启动主方法
        private LaunchResult Launch(LaunchSetting setting)
        {
            try
            {
                IsBusyLaunching = true;
                lock (launchLocker)
                {
                    if (Java == null)
                    {
                        return new LaunchResult(new NullJavaException());
                    }
                    if (setting.AuthenticateResult == null)
                    {
                        return new LaunchResult(new ArgumentException("启动所需必要的验证参数为空"));
                    }
                    if (setting.Version == null)
                    {
                        return new LaunchResult(new ArgumentException("启动所需必要的版本参数为空"));
                    }

                    if (setting.MaxMemory == 0)
                    {
                        setting.MaxMemory = SystemTools.GetBestMemory(this.Java);
                    }

                    Stopwatch sw = new Stopwatch();
                    sw.Start();


                    if (setting.LaunchType == LaunchType.SAFE)
                    {
                        setting.AdvencedGameArguments = null;
                        setting.AdvencedJvmArguments = null;
                        setting.GCArgument = null;
                        setting.JavaAgent = null;
                    }

                    string arg = argumentsParser.Parse(setting);

                    if (setting.LaunchType == LaunchType.CREATE_SHORT)
                    {
                        File.WriteAllText(Environment.CurrentDirectory + "\\LaunchMinecraft.bat", string.Format("\"{0}\" {1}", Java.Path, arg));
                        return new LaunchResult() { IsSuccess = true, LaunchArguments = arg };
                    }

                    #region 处理库文件
                    foreach (var item in setting.Version.Natives)
                    {
                        string nativePath = GetNativePath(item);
                        if (File.Exists(nativePath))
                        {
                            AppendLaunchInfoLog(string.Format("检查并解压不存在的库文件:{0}", nativePath));
                            Unzip.UnZipNativeFile(nativePath, GetGameVersionRootDir(setting.Version) + @"\$natives", item.Exclude, setting.LaunchType != LaunchType.SAFE);
                        }
                        else
                        {
                            return new LaunchResult(new NativeNotFoundException(item, nativePath));
                        }

                    }
                    #endregion

                    AppendLaunchInfoLog(string.Format("开始启动游戏进程，使用JAVA路径:{0}", this.Java.Path));
                    ProcessStartInfo startInfo = new ProcessStartInfo(Java.Path, arg)
                    { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = GetGameVersionRootDir(setting.Version) };
                    var process = Process.Start(startInfo);
                    sw.Stop();
                    long launchUsingMsTime = sw.ElapsedMilliseconds;
                    AppendLaunchInfoLog(string.Format("成功启动游戏进程,总共用时:{0}ms", launchUsingMsTime));

                    Task.Factory.StartNew(() =>
                    {
                        process.WaitForExit();
                        AppendLaunchInfoLog("游戏进程退出，代码:" + process.ExitCode);
                        GameExit?.Invoke(this, new GameExitArg()
                        {
                            Process = process,
                            Version = setting.Version,
                            ExitCode = process.ExitCode,
                            Duration = (process.StartTime - process.ExitTime)
                        });

                    });

                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    return new LaunchResult() { Process = process, IsSuccess = true, LaunchArguments = arg, LaunchUsingMs = launchUsingMsTime };
                }
            }
            catch (LaunchException.LaunchException ex)
            {
                return new LaunchResult(ex);
            }
            catch (Exception ex)
            {
                return new LaunchResult(ex);
            }
            finally
            {
                IsBusyLaunching = false;
            }
        }
        #endregion

        #region 版本获取
        public Modules.Version GetVersionByID(string id)
        {
            return versionReader.GetVersion(id);
        }

        public Modules.Version RefreshVersion(Modules.Version ver)
        {
            return versionReader.GetVersion(ver.ID);
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

        public List<Modules.Version> GetVersions()
        {
            try
            {
                return versionReader.GetVersions();
            }
            catch (Exception)
            {
                return new List<Modules.Version>();
            }
        }

        public Modules.Version JsonToVersion(string json)
        {
            return versionReader.JsonToVersion(json);
        }
        #endregion

        #region 路径获取
        public string GetGameVersionRootDir(Modules.Version ver)
        {
            return PathManager.GetGameVersionRootDir(VersionIsolation, GameRootPath, ver);
        }

        public string GetLibraryPath(Modules.Library lib)
        {
            return PathManager.GetLibraryPath(GameRootPath, lib);
        }

        public string GetNativePath(Native native)
        {
            return PathManager.GetNativePath(GameRootPath, native);
        }

        public string GetJsonPath(string ID)
        {
            return PathManager.GetJsonPath(GameRootPath, ID);
        }

        public string GetJarPath(Modules.Version ver)
        {
            return PathManager.GetJarPath(GameRootPath, ver);
        }

        public string GetJarPath(string id)
        {
            return PathManager.GetJarPath(GameRootPath, id);
        }

        public string GetAssetsIndexPath(string assetsID)
        {
            return PathManager.GetAssetsIndexPath(GameRootPath, assetsID);
        }

        public string GetAssetsPath(JAssetsInfo assetsInfo)
        {
            return PathManager.GetAssetsPath(GameRootPath, assetsInfo);
        }

        public string GetNide8JarPath()
        {
            return PathManager.GetNide8JarPath(GameRootPath);
        }

        public string GetAIJarPath()
        {
            return PathManager.GetAIJarPath(GameRootPath);
        }

        public string GetVersionOptionsPath(Modules.Version version)
        {
            return PathManager.GetVersionOptionsPath(VersionIsolation, GameRootPath, version);
        }
        #endregion

        #region DEBUG方法

        private void AppendLaunchDebugLog(string str)
        {
            this.LaunchLog?.Invoke(this, new Log() { LogLevel = LogLevel.DEBUG, Message = str });
        }

        private void AppendLaunchInfoLog(string str)
        {
            this.LaunchLog?.Invoke(this, new Log() { LogLevel = LogLevel.INFO, Message = str });
        }

        #endregion
    }

    public class GameExitArg : EventArgs
    {
        /// <summary>
        /// Exit code
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Process obj
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        /// Exited Version
        /// </summary>
        public Modules.Version Version { get; set; }

        /// <summary>
        /// From launch to exit time spawn
        /// </summary>
        public TimeSpan Duration { get; set; }

        public bool IsNormalExit()
        {
            if (ExitCode == 0)
            {
                return true;
            }
            else if (ExitCode == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
