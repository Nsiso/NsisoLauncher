using NsisoLauncherCore.LaunchException;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using NsisoLauncherCore.Util.Mod;
using NsisoLauncherCore.Util.Save;
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

        /// <summary>
        /// 存档处理器
        /// </summary>
        public SaveHandler SaveHandler { get; set; }

        /// <summary>
        /// Mod处理器
        /// </summary>
        public ModHandler ModHandler { get; set; }

        public event EventHandler<string> GameLog;
        public event EventHandler<GameExitArg> GameExit;
        public event EventHandler<Log> LaunchLog;

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

            SaveHandler = new SaveHandler(this);
            ModHandler = new ModHandler(this);
        }

        public async Task<LaunchResult> LaunchAsync(LaunchSetting setting)
        {
            var result = await Task.Factory.StartNew(() =>
            {
                return Launch(setting);
            });
            return result;
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
                    if (setting.LaunchUser == null)
                    {
                        return new LaunchResult(new ArgumentException("启动所需必要的用户参数为空"));
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

                    #region 检查处理库文件
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

                    LaunchInstance instance = new LaunchInstance(setting, startInfo);
                    instance.Exit += Instance_Exit;
                    instance.Log += Instance_Log;

                    instance.Start();

                    sw.Stop();
                    long launchUsingMsTime = sw.ElapsedMilliseconds;
                    AppendLaunchInfoLog(string.Format("成功启动游戏进程,总共用时:{0}ms", launchUsingMsTime));


                    return new LaunchResult() { Instance = instance, IsSuccess = true, LaunchArguments = arg, LaunchUsingMs = launchUsingMsTime };
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

        private void Instance_Log(object sender, string e)
        {
            this.GameLog?.Invoke(this, e);
        }


        private void Instance_Exit(object sender, GameExitArg e)
        {
            GameExit?.Invoke(this, e);
        }
        #endregion

        #region 版本获取
        public Modules.Version GetVersionByID(string id)
        {
            return versionReader.GetVersion(id);
        }

        public Modules.Version RefreshVersion(Modules.Version ver)
        {
            return versionReader.GetVersion(ver.Id);
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

        public string GetAssetsPath(JAssetInfo assetsInfo)
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

        public string GetVersionSavesDir(Modules.Version version)
        {
            return PathManager.GetVersionSavesDir(VersionIsolation, GameRootPath, version);
        }

        public string GetVersionModsDir(Modules.Version version)
        {
            return PathManager.GetVersionModsDir(VersionIsolation, GameRootPath, version);
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
        public LaunchInstance Instance { get; set; }

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
            return ExitCode == 0;
        }
    }
}
