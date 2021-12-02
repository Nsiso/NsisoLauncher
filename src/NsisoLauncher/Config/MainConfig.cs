using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.User;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NsisoLauncher.Config
{
    #region Enum
    public enum GameDirEnum
    {
        /// <summary>
        /// 启动器根目录
        /// </summary>
        ROOT = 0,

        /// <summary>
        /// 系统AppData
        /// </summary>
        APPDATA = 1,

        /// <summary>
        /// 系统程序文件夹
        /// </summary>
        PROGRAMFILES = 2,

        /// <summary>
        /// 自定义
        /// </summary>
        CUSTOM = 3
    }

    #endregion

    /// <summary>
    /// 主设置
    /// </summary>
    public class MainConfig
    {
        private User user;
        private Environment environment;
        private History history;
        private Launcher launcher;
        private Net net;
        private Server server;
        private Customize customize;

        /// <summary>
        /// 用户信息
        /// </summary>
        public User User
        {
            get
            {
                if (user == null)
                {
                    user = new User();
                }
                return user;
            }
            set { user = value; }
        }

        /// <summary>
        /// 启动环境设置
        /// </summary>
        public Environment Environment
        {
            get
            {
                if (environment == null)
                {
                    environment = new Environment();
                }
                return environment;
            }
            set { environment = value; }
        }

        /// <summary>
        /// 历史数据
        /// </summary>
        public History History
        {
            get
            {
                if (history == null)
                {
                    history = new History();
                }
                return history;
            }
            set { history = value; }
        }

        /// <summary>
        /// 启动器设置
        /// </summary>
        public Launcher Launcher
        {
            get
            {
                if (launcher == null)
                {
                    launcher = new Launcher();
                }
                return launcher;
            }
            set { launcher = value; }
        }

        /// <summary>
        /// 网络设置
        /// </summary>
        public Net Net
        {
            get
            {
                if (net == null)
                {
                    net = new Net();
                }
                return net;
            }
            set { net = value; }
        }

        /// <summary>
        /// 服务器设置
        /// </summary>
        public Server Server
        {
            get
            {
                if (server == null)
                {
                    server = new Server();
                }
                return server;
            }
            set { server = value; }
        }

        /// <summary>
        /// 自定义设置
        /// </summary>
        public Customize Customize
        {
            get
            {
                if (customize == null)
                {
                    customize = new Customize();
                }
                return customize;
            }
            set { customize = value; }
        }

        /// <summary>
        /// 配置文件版本
        /// </summary>
        public string ConfigVersion { get; set; }
    }

    /// <summary>
    /// 用户基本设置
    /// </summary>
    public class User : INotifyPropertyChanged
    {
        private ObservableDictionary<string, UserNode> userDatabase;

        /// <summary>
        /// 用户数据库
        /// </summary>
        public ObservableDictionary<string, UserNode> UserDatabase
        {
            get
            {
                if (userDatabase == null)
                {
                    userDatabase = new ObservableDictionary<string, UserNode>();
                }
                return userDatabase;
            }
            set
            {
                userDatabase = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UserDatabase"));
            }
        }

        /// <summary>
        /// 选中的用户
        /// </summary>
        public string SelectedUserUuid { get; set; }

        private ObservableDictionary<string, AuthenticationNode> authenticationDic;

        [JsonIgnore]
        public UserNode SelectedUser
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedUserUuid))
                {
                    return null;
                }
                if (UserDatabase.ContainsKey(SelectedUserUuid))
                {
                    return UserDatabase[SelectedUserUuid];
                }
                else
                {
                    return null;
                }
            }
        }

        public AuthenticationNode GetUserAuthenticationNode(UserNode userNode)
        {
            if (userNode == null)
            {
                throw new Exception("The Usernode argument is null");
            }
            string authNodeName = userNode.AuthModule;
            if (string.IsNullOrEmpty(authNodeName))
            {
                return null;
            }
            if (!AuthenticationDic.ContainsKey(authNodeName))
            {
                return null;
            }
            return AuthenticationDic[authNodeName];
        }

        /// <summary> 
        /// 验证节点
        /// </summary>
        public ObservableDictionary<string, AuthenticationNode> AuthenticationDic
        {
            get
            {
                if (authenticationDic == null)
                {
                    authenticationDic = new ObservableDictionary<string, AuthenticationNode>();
                }
                return authenticationDic;
            }
            set
            {
                authenticationDic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AuthenticationDic"));
            }
        }

        /// <summary>
        /// 用户端Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// 锁定全局验证
        /// </summary>
        public string LockAuthName { get; set; }

        /// <summary>
        /// 全局是否对NIDE8服务器依赖
        /// </summary>
        public bool Nide8ServerDependence { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 获取锁定验证模型，若不存在返回NULL
        /// </summary>
        /// <returns>锁定的验证模型</returns>
        public AuthenticationNode GetLockAuthNode()
        {
            if ((!string.IsNullOrWhiteSpace(LockAuthName)) && (AuthenticationDic.ContainsKey(LockAuthName)))
            {
                return AuthenticationDic[LockAuthName];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 启动环境基本设置
    /// </summary>
    public class Environment : INotifyPropertyChanged
    {
        /// <summary>
        /// 版本隔离
        /// </summary>
        public bool VersionIsolation { get; set; }

        /// <summary>
        /// 游戏路径类型
        /// </summary>
        public GameDirEnum GamePathType { get; set; }

        /// <summary>
        /// 游戏根路径
        /// </summary>
        public string GamePath { get; set; }

        /// <summary>
        /// 是否启动垃圾回收，默认开启
        /// </summary>
        public bool GCEnabled { get; set; }

        /// <summary>
        /// 垃圾回收器种类(默认G1)
        /// </summary>
        public GCType GCType { get; set; }

        /// <summary>
        /// 垃圾回收器附加参数
        /// </summary>
        public string GCArgument { get; set; }

        /// <summary>
        /// 是否使用自动选择java
        /// </summary>
        public bool AutoJava { get; set; }

        /// <summary>
        /// 启动所使用JAVA路径
        /// </summary>
        public string JavaPath { get; set; }

        /// <summary>
        /// JavaAgent参数
        /// </summary>
        public string JavaAgent { get; set; }

        /// <summary>
        /// 是否使用自动内存分配
        /// </summary>
        public bool AutoMemory { get; set; }

        /// <summary>
        /// 游戏最大内存
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// 游戏最小内存
        /// </summary>
        public int MinMemory { get; set; }

        /// <summary>
        /// 附加虚拟机启动参数
        /// </summary>
        public string AdvencedJvmArguments { get; set; }

        /// <summary>
        /// 附加游戏启动参数
        /// </summary>
        public string AdvencedGameArguments { get; set; }


        private Resolution windowSize;

        /// <summary>
        /// 游戏窗口大小
        /// </summary>
        public Resolution WindowSize
        {
            get
            {
                if (windowSize == null)
                {
                    windowSize = new Resolution();
                }
                return windowSize;
            }
            set
            {
                windowSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WindowSize"));
            }
        }

        /// <summary>
        /// 是否下载丢失游戏依赖库
        /// </summary>
        public bool DownloadLostDepend { get; set; }

        /// <summary>
        /// 是否下载丢失游戏资源库
        /// </summary>
        public bool DownloadLostAssets { get; set; }

        /// <summary>
        /// 启动后退出启动器
        /// </summary>
        public bool ExitAfterLaunch { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 启动器设置
    /// </summary>
    public class Launcher : INotifyPropertyChanged
    {
        /// <summary>
        /// 是否开启DEBUG模式
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// 是否禁止数据追踪
        /// </summary>
        public bool NoTracking { get; set; }

        /// <summary>
        /// 是否检查更新
        /// </summary>
        public bool CheckUpdate { get; set; }

        /// <summary>
        /// 是否记录日志到本地
        /// </summary>
        public bool WriteLog { get; set; }


        private Resolution launcherWindowSize;

        /// <summary>
        /// 游戏窗口大小
        /// </summary>
        public Resolution LauncherWindowSize
        {
            get
            {
                if (launcherWindowSize == null)
                {
                    launcherWindowSize = new Resolution() { Width = 980, Height = 540 };
                }
                return launcherWindowSize;
            }
            set
            {
                launcherWindowSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LauncherWindowSize"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 网络设置
    /// </summary>
    public class Net : INotifyPropertyChanged
    {
        /// <summary>
        /// 下载源设置
        /// </summary>
        public NsisoLauncherCore.Net.DownloadSource DownloadSource { get; set; }

        /// <summary>
        /// 版本设置
        /// </summary>
        public VersionSourceType VersionSource { get; set; }

        /// <summary>
        /// 功能设置
        /// </summary>
        public FunctionSourceType FunctionSource { get; set; }

        /// <summary>
        /// 下载使用协议
        /// </summary>
        public DownloadProtocolType DownloadProtocolType { get; set; }

        /// <summary>
        /// 线程数量
        /// </summary>
        public int DownloadThreadsSize { get; set; }

        /// <summary>
        /// 代理下载服务器地址
        /// </summary>
        public string ProxyHost { get; set; }

        /// <summary>
        /// 代理下载服务器端口
        /// </summary>
        public ushort ProxyPort { get; set; }

        /// <summary>
        /// 代理服务器账号
        /// </summary>
        public string ProxyUsername { get; set; }

        /// <summary>
        /// 代理服务器密码
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// 游戏是否使用代理
        /// </summary>
        public bool IsGameUseProxy { get; set; }

        /// <summary>
        /// 启动器是否使用代理
        /// </summary>
        public bool IsLauncherUseProxy { get; set; }

        /// <summary>
        /// 下载后是否检查哈希值（前提为可用）
        /// </summary>
        public bool CheckDownloadFileHash { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 历史记录设置
    /// </summary>
    public class History : INotifyPropertyChanged
    {
        /// <summary>
        /// 上一次启动版本
        /// </summary>
        public string LastLaunchVersion { get; set; }

        /// <summary>
        /// 上次启动时间
        /// </summary>
        public DateTime LastLaunchTime { get; set; }

        /// <summary>
        /// 上次启动使用的时间(Ms)
        /// </summary>
        public long LastLaunchUsingMs { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 服务器设置
    /// </summary>
    public class Server : INotifyPropertyChanged
    {
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 是否在主界面显示服务器信息
        /// </summary>
        public bool ShowServerInfo { get; set; }

        /// <summary>
        /// 是否在启动后直接进入服务器
        /// </summary>
        public bool LaunchToServer { get; set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort Port { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 自定义设置
    /// </summary>
    public class Customize : INotifyPropertyChanged
    {
        /// <summary>
        /// 是否使用自定义壁纸
        /// </summary>
        public bool CustomBackGroundPicture { get; set; }

        /// <summary>
        /// 是否使用自定义背景音乐
        /// </summary>
        public bool CustomBackGroundMusic { get; set; }

        /// <summary>
        /// 是否使用自定义背景视频
        /// </summary>
        public bool CustomBackGroundVideo { get; set; }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public string AccentColor { get; set; }

        /// <summary>
        /// 主题Thme
        /// </summary>
        public string AppTheme { get; set; }

        /// <summary>
        /// 启动器标题
        /// </summary>
        public string LauncherTitle { get; set; }

        /// <summary>
        /// 游戏窗口标题
        /// </summary>
        public string GameWindowTitle { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        public string VersionInfo { get; set; }

        /// <summary>
        /// 欢迎界面标题
        /// </summary>
        public string WelcomeTitle { get; set; }

        /// <summary>
        /// 欢迎界面Icon源
        /// </summary>
        public string WelcomeIconSource { get; set; }

        /// <summary>
        /// 欢迎界面自定背景图片
        /// </summary>
        public string WelcomeImageSource { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 用户验证节点设置
    /// </summary>
    public class UserNode
    {
        /// <summary>
        /// 所使用的验证模型
        /// </summary>
        public string AuthModule { get; set; }

        public IUser User { get; set; }
    }


    public enum VersionSourceType
    {
        MOJANG = 0,
        BMCLAPI = 1,
        MCBBS = 2
    }

    public enum FunctionSourceType
    {
        BMCLAPI = 0,
        MCBBS = 1
    }
}
