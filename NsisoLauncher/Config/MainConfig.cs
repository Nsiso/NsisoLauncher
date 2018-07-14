using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Net.MojangApi.Api;
using System;
using static NsisoLauncher.Core.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncher.Config
{
    public class MainConfig
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// 启动环境设置
        /// </summary>
        public Environment Environment { get; set; }

        /// <summary>
        /// 历史数据
        /// </summary>
        public History History { get; set; }

        /// <summary>
        /// 启动器设置
        /// </summary>
        public Launcher Launcher { get; set; }

        /// <summary>
        /// 下载设置
        /// </summary>
        public Download Download { get; set; }

        /// <summary>
        /// 服务器设置
        /// </summary>
        public Server Server { get; set; }

        /// <summary>
        /// 自定义设置
        /// </summary>
        public Customize Customize { get; set; }
    }

    public class User
    {
        /// <summary>
        /// 用户名/账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 验证令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 用户端Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// 验证服务器
        /// </summary>
        public string AuthServer { get; set; }

        /// <summary>
        /// 验证类型
        /// </summary>
        public AuthenticationType AuthenticationType { get; set; }

        /// <summary>
        /// 玩家选择的角色UUID
        /// </summary>
        public Uuid AuthenticationUUID { get; set; }

        /// <summary>
        /// 验证的用户信息
        /// </summary>
        public UserData AuthenticationUserData { get; set; }
    }


    /// <summary>
    /// 启动环境基本设置
    /// </summary>
    public class Environment
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

        /// <summary>
        /// 游戏窗口大小
        /// </summary>
        public WindowSize WindowSize { get; set; }

        /// <summary>
        /// 是否下载丢失游戏依赖库
        /// </summary>
        public bool DownloadLostDepend { get; set; }

        /// <summary>
        /// 是否下载丢失游戏资源库
        /// </summary>
        public bool DownloadLostAssets { get; set; }
    }

    public class Launcher
    {
        /// <summary>
        /// 是否开启DEBUG模式
        /// </summary>
        public bool Debug { get; set; }
    }

    public class Download
    {
        /// <summary>
        /// 下载源设置
        /// </summary>
        public Core.Net.DownloadSource DownloadSource { get; set; }

        /// <summary>
        /// 线程数量
        /// </summary>
        public int DownloadThreadsSize { get; set; }

        /// <summary>
        /// 代理下载服务器地址
        /// </summary>
        public string DownloadProxyAddress { get; set; }

        /// <summary>
        /// 代理下载服务器端口
        /// </summary>
        public ushort DownloadProxyPort { get; set; }

        /// <summary>
        /// 代理服务器账号
        /// </summary>
        public string ProxyUserName { get; set; }

        /// <summary>
        /// 代理服务器密码
        /// </summary>
        public string ProxyUserPassword { get; set; }
    }

    public enum AuthenticationType
    {
        /// <summary>
        /// 离线验证
        /// </summary>
        OFFLINE,

        /// <summary>
        /// 在线验证
        /// </summary>
        ONLINE
    }

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

    /// <summary>
    /// 历史记录类
    /// </summary>
    public class History
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
    }

    /// <summary>
    /// 服务器设置类
    /// </summary>
    public class Server
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
    }

    public class Customize
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
        /// 主题颜色
        /// </summary>
        public string AccentColor { get; set; }

        /// <summary>
        /// 主题Thme
        /// </summary>
        public string AppThme { get; set; }

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
    }
}
