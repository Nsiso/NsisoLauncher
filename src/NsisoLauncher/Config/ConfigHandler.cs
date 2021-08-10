using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NsisoLauncherCore;
using NsisoLauncherCore.Config;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace NsisoLauncher.Config
{
    public class ConfigHandler : IDisposable
    {
        /// <summary>
        /// 首要配置文件路径
        /// </summary>
        public string MainConfigPath { get => PathManager.BaseStorageDirectory + @"\Config\MainConfig.json"; }

        /// <summary>
        /// 官方用户配置文件路径
        /// </summary>
        public string LauncherProfilesConfigPath { get => Path.GetFullPath(@".minecraft\launcher_profiles.json"); }

        /// <summary>
        /// 首要配置文件内容
        /// </summary>
        public MainConfig MainConfig { get; set; }

        /// <summary>
        /// 官方用户配置文件内容
        /// </summary>
        public LauncherProfiles LauncherProfilesConfig { get; set; }

        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto
        };

        private readonly ReaderWriterLockSlim mainconfigLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim launcherProfilesLock = new ReaderWriterLockSlim();

        public ConfigHandler()
        {
            try
            {
                if (!File.Exists(MainConfigPath))
                { NewConfig(); }
                else
                { Read(); }


                if (!File.Exists(LauncherProfilesConfigPath))
                {
                    NewProfilesConfig();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (System.Security.SecurityException e)
            {
                NoAccessWarning(e);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "启动器配置文件读写错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void Save()
        {
            mainconfigLock.EnterWriteLock();
            try
            {
                string dir = Path.GetDirectoryName(MainConfigPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(MainConfigPath, JsonConvert.SerializeObject(MainConfig, Formatting.Indented, SerializerSettings));
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (System.Security.SecurityException e)
            {
                NoAccessWarning(e);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "启动器文件读写错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                mainconfigLock.ExitWriteLock();
            }

        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        public void Read()
        {
            mainconfigLock.EnterReadLock();
            try
            {
                MainConfig = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(MainConfigPath), SerializerSettings);
#if !DEBUG
                if (string.IsNullOrWhiteSpace(MainConfig.ConfigVersion) ||
                    (!string.Equals(Assembly.GetExecutingAssembly().GetName().Version.ToString(), MainConfig.ConfigVersion)))
                {
                    MessageBox.Show("启动器配置文件版本不符。\n" +
                        "这可能是因为配置文件为旧版本启动器生成而导致的，虽然已经做了兼容，但有可能仍会出现异常，如果出现异常可以尝试删除配置文件",
                        "启动器配置文件版本不符", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
#endif
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (System.Security.SecurityException e)
            {
                NoAccessWarning(e);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "启动器文件读写错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                mainconfigLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 写配置文件
        /// </summary>
        public void SaveProfilesConfig()
        {
            launcherProfilesLock.EnterWriteLock();
            try
            {
                string profilesConfigDir = Path.GetDirectoryName(LauncherProfilesConfigPath);
                if (!Directory.Exists(profilesConfigDir))
                {
                    Directory.CreateDirectory(profilesConfigDir);
                }
                File.WriteAllText(LauncherProfilesConfigPath, JsonConvert.SerializeObject(LauncherProfilesConfig, Formatting.Indented, SerializerSettings));
            }
            catch (UnauthorizedAccessException e)
            {
                NoAccessWarning(e);
            }
            catch (System.Security.SecurityException e)
            {
                NoAccessWarning(e);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "启动器文件读写错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                launcherProfilesLock.ExitWriteLock();
            }
        }

        //public void ReadProfilesConfig()
        //{
        //    lock (launcherProfilesLocker)
        //    {
        //        LauncherProfilesConfig = JsonConvert.DeserializeObject<LauncherProfilesConfig>(File.ReadAllText(MainConfigPath));
        //    }
        //}

        /// <summary>
        /// 新的官方用户配置文件
        /// </summary>
        private void NewProfilesConfig()
        {
            LauncherProfilesConfig = new LauncherProfiles()
            {
                ClientToken = Guid.NewGuid().ToString("N"),
                Profiles = new ObservableDictionary<string, VersionProfile>()
            };
            SaveProfilesConfig();
        }

        /// <summary>
        /// 新建配置文件
        /// </summary>
        private void NewConfig()
        {
            MainConfig = new MainConfig()
            {
                User = new User()
                {
                    ClientToken = Guid.NewGuid().ToString("N"),
                    UserDatabase = new ObservableDictionary<string, UserNode>(),
                    AuthenticationDic = new ObservableDictionary<string, AuthenticationNode>()
                    {
                        {"offline", new AuthenticationNode("offline"){AuthType = AuthenticationType.OFFLINE, Name="离线登录", Locked = true} },
                        {"mojang", new AuthenticationNode("mojang"){AuthType = AuthenticationType.MOJANG, Name="Mojang正版登录", Locked = true} },
                        {"microsoft", new AuthenticationNode("microsoft"){AuthType = AuthenticationType.MICROSOFT, Name="微软正版登录", Locked = true} },
                    }
                },
                History = new History(),
                Environment = new Environment()
                {
                    VersionIsolation = true,
                    AutoMemory = true,
                    GamePathType = GameDirEnum.ROOT,
                    DownloadLostAssets = true,
                    DownloadLostDepend = true,
                    GCEnabled = true,
                    GCType = GCType.G1GC,
                    GCArgument = "-XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M",
                    AutoJava = true,
                    WindowSize = new Resolution() { FullScreen = false },
                    ExitAfterLaunch = false
                },
                Net = new Net()
                {
                    DownloadSource = DownloadSource.Auto,
                    DownloadThreadsSize = 3,
                    CheckDownloadFileHash = true,
                    FunctionSource = FunctionSourceType.MCBBS,
                    VersionSource = VersionSourceType.MCBBS
                },
                Launcher = new Launcher()
                {
                    Debug = false,
                    CheckUpdate = true,
                    NoTracking = false
                },
                Server = new Server()
                {
                    ShowServerInfo = false,
                    LaunchToServer = false,
                    Port = 25565
                },
                Customize = new Customize()
                {
                    CustomBackGroundMusic = false,
                    CustomBackGroundPicture = false,
                    AccentColor = "Blue",
                    AppTheme = "Light"
                },
                ConfigVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
            Save();
        }

        private void NoAccessWarning(Exception e)
        {
            var result = MessageBox.Show("启动器无法正常读/写配置文件。\n" +
                    "这可能是由于您将启动器放置在系统敏感目录（如C盘，桌面等系统关键位置）\n" +
                    "或您没有足够的系统操作权限而导致系统自我保护机制，禁止启动器读写文件。\n" +
                    "是否以管理员模式运行启动器？若拒绝则请自行移动到有权限的路径运行。\n" +
                    "详细信息:\n" +
                    e.ToString(),
                    "启动器权限不足", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                App.Reboot(true);
            }
        }

        public void Dispose()
        {
            mainconfigLock.Dispose();
            launcherProfilesLock.Dispose();
        }
    }
}
