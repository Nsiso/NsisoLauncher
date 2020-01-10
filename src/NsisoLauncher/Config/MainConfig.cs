using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.MojangApi.Api;
using System;
using System.Collections.Generic;
using NsisoLauncher.Utils;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;
using System.ComponentModel;
using System.Collections.ObjectModel;

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
        /// <summary>
        /// 用户数据库
        /// </summary>
        public ObservableDictionary<string, UserNode> UserDatabase { get; set; }

        /// <summary> 
        /// 验证节点
        /// </summary>
        public ObservableDictionary<string, AuthenticationNode> AuthenticationDic { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 下载设置
    /// </summary>
    public class Download : INotifyPropertyChanged
    {
        /// <summary>
        /// 下载源设置
        /// </summary>
        public NsisoLauncherCore.Net.DownloadSource DownloadSource { get; set; }

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
        /// 选中的用户的ID
        /// </summary>
        public string SelectedUserNodeID { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 用户验证节点设置
    /// </summary>
    public class UserNode : INotifyPropertyChanged
    {
        /// <summary>
        /// 所使用的验证模型
        /// </summary>
        public string AuthModule { get; set; }

        /// <summary>
        /// 用户名/账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 验证令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 选中的profile UUID
        /// </summary>
        public string SelectProfileUUID { get; set; }

        /// <summary>
        /// 玩家选择的角色UUID
        /// </summary>
        public Dictionary<string, Uuid> Profiles { get; set; } = new Dictionary<string, Uuid>();

        /// <summary>
        /// 用户资料
        /// </summary>
        public UserData UserData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Uuid GetSelectProfileUUID()
        {
            return Profiles[SelectProfileUUID];
        }

        public void ClearAuthCache()
        {
            AccessToken = null;
            SelectProfileUUID = null;
        }
    }

    /// <summary>
    /// 验证节点设置
    /// </summary>
    public class AuthenticationNode : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public AuthenticationType AuthType { get; set; }

        /// <summary>
        /// authserver:验证服务器地址
        /// nide8ID:NIDE8的验证ID
        /// </summary>
        public Dictionary<string, string> Property { get; set; } = new Dictionary<string, string>();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
