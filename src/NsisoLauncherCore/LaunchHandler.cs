using NsisoLauncherCore.LaunchException;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// 是否开启版本隔离
        /// </summary>
        public bool VersionIsolation { get; set; }

        /// <summary>
        /// 是否在处理启动
        /// </summary>
        public bool IsBusyLaunching { get; private set; } = false;

        /// <summary>
        /// All javas
        /// </summary>
        public ObservableCollection<Java> Javas { get; set; }

        public event EventHandler<Log> GameLog;
        public event EventHandler<GameExitArg> GameExit;
        public event EventHandler<Log> LaunchLog;

        private ArgumentsParser argumentsParser;
        private VersionReader versionReader;
        private AssetsReader assetsReader;

        public LaunchHandler() : this(PathManager.CurrentLauncherDirectory + "\\.minecraft", false) { }

        public LaunchHandler(string gamepath, bool isversionIsolation)
        {
            this.GameRootPath = gamepath;
            this.VersionIsolation = isversionIsolation;

            versionReader = new VersionReader(this);
            versionReader.VersionReaderLog += (s, l) => LaunchLog?.Invoke(s, l);

            argumentsParser = new ArgumentsParser(this);
            argumentsParser.ArgumentsParserLog += (s, l) => LaunchLog?.Invoke(s, l);

            assetsReader = new AssetsReader(this);
        }

        public async Task<LaunchResult> LaunchAsync(VersionBase ver, LaunchSetting setting)
        {
            var result = await Task.Factory.StartNew(() =>
            {
                return Launch(ver, setting);
            });
            return result;
        }

        public JAssets GetAssets(VersionBase version)
        {
            return assetsReader.GetAssets(version);
        }

        public JAssets GetAssetsByJson(string json)
        {
            return assetsReader.GetAssetsByJson(json);
        }

        public async Task<JAssets> GetAssetsAsync(VersionBase version)
        {
            return await Task.Factory.StartNew(() =>
            {
                return assetsReader.GetAssets(version);
            });

        }

        #region 启动主方法
        private LaunchResult Launch(VersionBase ver, LaunchSetting setting)
        {
            LaunchResult result = new LaunchResult() { Setting = setting, UsingJava = setting.UsingJava, Version = ver };
            try
            {
                IsBusyLaunching = true;
                lock (launchLocker)
                {
                    #region Check
                    // check launch user is null
                    if (setting.LaunchUser == null)
                    {
                        result.SetException(new ArgumentException("启动所需必要的用户参数为空"));
                        return result;
                    }

                    // check launch version is null
                    if (ver == null)
                    {
                        result.SetException(new ArgumentException("启动所需必要的版本参数为空"));
                        return result;
                    }

                    // take care with inherit
                    if (ver.InheritsFrom != null && ver.InheritsFromInstance == null)
                    {
                        throw new LaunchException.LaunchException("父版本不存在", "启动该版本需要依赖父版本");
                    }

                    // choose suitable java for version
                    if (setting.UsingJava == null)
                    {
                        setting.UsingJava = Java.GetSuitableJava(Javas, ver);
                    }

                    //check java for minecraft
                    JavaVersion javaVersion = null;
                    if (setting.UsingJava == null)
                    {
                        result.SetException(new NullJavaException());
                        return result;
                    }
                    if (ver.JavaVersion != null)
                    {
                        javaVersion = ver.JavaVersion;
                    }
                    else if (ver.InheritsFromInstance != null && ver.InheritsFromInstance.JavaVersion != null)
                    {
                        javaVersion = ver.InheritsFromInstance.JavaVersion;
                    }
                    if (javaVersion != null && javaVersion.MajorVersion > setting.UsingJava.MajorVersion)
                    {
                        result.SetException(new JavaNotMatchedException(javaVersion, setting.UsingJava));
                        return result;
                    }

                    // auto memory size
                    if (setting.AutoMemory)
                    {
                        setting.MaxMemory = SystemTools.GetBestMemory(setting.UsingJava);
                        setting.MinMemory = 0;
                    }

                    // check folder is exsist
                    string assetsRoot = this.GetAssetsRoot();
                    if (!Directory.Exists(assetsRoot))
                    {
                        Directory.CreateDirectory(assetsRoot);
                    }
                    #endregion

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    if (setting.LaunchType == LaunchType.SAFE)
                    {
                        setting.AdvencedGameArguments = null;
                        setting.AdvencedJvmArguments = null;
                        setting.GCArgument = null;
                        setting.JavaAgent = null;

                        AppendLaunchInfoLog("正在检查游戏依赖文件完整性");
                        var validate_result = GameValidator.Validate(this, ver, ValidateType.DEPEND);
                        if (!validate_result.IsPass)
                        {
                            foreach (var item in validate_result.FailedFiles)
                            {
                                switch (item.Value)
                                {
                                    case FileFailedState.NOT_EXIST:
                                        AppendLaunchInfoLog(string.Format("文件缺失:{0}", item.Key));
                                        break;
                                    case FileFailedState.WRONG_HASH:
                                        AppendLaunchInfoLog(string.Format("文件损坏（hash不一致）:{0}", item.Key));
                                        break;
                                    default:
                                        AppendLaunchInfoLog(string.Format("未知文件异常:{0}", item.Key));
                                        break;
                                }
                            }
                            result.SetException(new GameValidateFailedException(validate_result.FailedFiles));
                            return result;
                        }
                    }

                    #region 检查依赖文件
                    string core_path = GetVersionJarPath(ver);
                    if (!File.Exists(core_path))
                    {
                        if (ver.InheritsFromInstance != null)
                        {
                            string base_core_path = GetVersionJarPath(ver.InheritsFromInstance);
                            if (!File.Exists(base_core_path))
                            {
                                throw new LaunchException.LaunchException("继承版本丢失启动核心且无法补全", "请尝试重新安装此版本");
                            }
                            File.Copy(base_core_path, core_path, true);
                        }
                        else
                        {
                            throw new LaunchException.LaunchException("丢失启动核心且无法补全", "请尝试重新安装此版本");
                        }
                    }

                    List<Library> libraries = ver.GetAllLibraries();
                    foreach (var item in libraries)
                    {
                        if (item.IsEnable())
                        {
                            string libPath = GetLibraryPath(item);
                            if (item is Native native)
                            {
                                // is a native
                                if (File.Exists(libPath))
                                {
                                    AppendLaunchInfoLog(string.Format("检查并解压不存在的库文件:{0}", libPath));
                                    Unzip.UnZipNativeFile(libPath, GetVersionWorkspaceDir(ver) + @"\$natives", native.Extract, setting.LaunchType != LaunchType.SAFE);
                                }
                                else
                                {
                                    result.SetException(new NativeNotFoundException(native, libPath));
                                    return result;
                                }
                            }
                            else
                            {
                                // not a native
                                if (!File.Exists(libPath))
                                {
                                    result.SetException(new LibraryNotFoundException(item, libPath));
                                    return result;
                                }
                            }
                        }
                    }
                    #endregion

                    // 生成启动参数
                    string arg = argumentsParser.Parse(ver, setting);
                    result.LaunchArguments = arg;


                    if (setting.LaunchType == LaunchType.CREATE_SHORT)
                    {
                        File.WriteAllText(Environment.CurrentDirectory + "\\LaunchMinecraft.bat", string.Format("\"{0}\" {1}", setting.UsingJava.Path, arg));

                        result.SetSuccess();
                        return result;
                    }

                    AppendLaunchInfoLog(string.Format("开始启动游戏进程，使用JAVA路径:{0}", setting.UsingJava.Path));

                    if (!File.Exists(setting.UsingJava.Path))
                    {
                        result.SetException(new NullJavaException());
                        return result;
                    }

                    ProcessStartInfo startInfo = new ProcessStartInfo(setting.UsingJava.Path, arg)
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        WorkingDirectory = GetVersionWorkspaceDir(ver)
                    };

                    LaunchInstance instance = new LaunchInstance(ver, setting, startInfo);
                    result.Instance = instance;
                    instance.Exit += Instance_Exit;
                    instance.Log += Instance_Log;

                    instance.Start();

                    sw.Stop();
                    long launchUsingMsTime = sw.ElapsedMilliseconds;
                    result.LaunchUsingMs = launchUsingMsTime;


                    AppendLaunchInfoLog(string.Format("成功启动游戏进程,总共用时:{0}ms", launchUsingMsTime));


                    result.SetSuccess();
                    return result;
                }
            }
            catch (LaunchException.LaunchException ex)
            {
                result.SetException(ex);
                return result;
            }
            catch (Exception ex)
            {
                result.SetException(ex);
                return result;
            }
            finally
            {
                IsBusyLaunching = false;
            }
        }

        private void Instance_Log(object sender, Log e)
        {
            this.GameLog?.Invoke(this, e);
        }


        private void Instance_Exit(object sender, GameExitArg e)
        {
            GameExit?.Invoke(this, e);
        }
        #endregion

        #region 版本获取
        public VersionBase GetVersionByID(string id)
        {
            return versionReader.GetVersion(id);
        }

        public VersionBase RefreshVersion(VersionBase ver)
        {
            return versionReader.GetVersion(ver.Id);
        }

        public async Task<List<VersionBase>> GetVersionsAsync()
        {
            try
            {
                return await versionReader.GetVersionsAsync();
            }
            catch (Exception)
            {
                return new List<VersionBase>();
            }
        }

        public List<VersionBase> GetVersions()
        {
            try
            {
                return versionReader.GetVersions();
            }
            catch (Exception)
            {
                return new List<VersionBase>();
            }
        }

        public VersionBase JsonToVersion(string json)
        {
            return versionReader.JsonToVersion(json);
        }
        #endregion

        #region 路径获取
        public string GetVersionsRoot()
        {
            return PathManager.GetVersionsRoot(GameRootPath);
        }

        public string GetVersionWorkspaceDir(VersionBase ver)
        {
            return PathManager.GetVersionWorkspaceDir(VersionIsolation, GameRootPath, ver);
        }

        public string GetLibrariesRoot()
        {
            return PathManager.GetLibrariesRoot(GameRootPath);
        }

        public string GetLibraryPath(Library lib)
        {
            return PathManager.GetLibraryPath(GameRootPath, lib);
        }

        public string GetVersionJsonPath(string ID)
        {
            return PathManager.GetVersionJsonPath(GameRootPath, ID);
        }

        public string GetVersionJarPath(VersionBase ver)
        {
            return PathManager.GetVersionJarPath(GameRootPath, ver);
        }

        public string GetAssetsRoot()
        {
            return PathManager.GetAssetsRoot(GameRootPath);
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

        public string GetVersionOptionsPath(VersionBase version)
        {
            return PathManager.GetVersionOptionsPath(VersionIsolation, GameRootPath, version);
        }

        public string GetVersionSavesDir(VersionBase version)
        {
            return PathManager.GetVersionSavesDir(VersionIsolation, GameRootPath, version);
        }

        public string GetVersionModsDir(VersionBase version)
        {
            return PathManager.GetVersionModsDir(VersionIsolation, GameRootPath, version);
        }

        public string GetVersionResourcePacksDir(VersionBase version)
        {
            return PathManager.GetVersionResourcePacksDir(VersionIsolation, GameRootPath, version);
        }
        #endregion

        #region DEBUG方法

        private void AppendLaunchDebugLog(string str)
        {
            this.LaunchLog?.Invoke(this, new Log(LogLevel.DEBUG, str));
        }

        private void AppendLaunchInfoLog(string str)
        {
            this.LaunchLog?.Invoke(this, new Log(LogLevel.INFO, str));
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
        public VersionBase Version { get; set; }

        /// <summary>
        /// From launch to exit time spawn
        /// </summary>
        public TimeSpan Duration { get; set; }

        public bool IsNormalExit()
        {
            if (ExitCode == 0 || Instance.IsBeenKilled)
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
