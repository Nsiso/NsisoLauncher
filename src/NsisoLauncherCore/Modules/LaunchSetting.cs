using NsisoLauncherCore.Auth;

namespace NsisoLauncherCore.Modules
{
    public enum GCType
    {
        /// <summary>
        /// 默认G1垃圾回收器 兼容JAVA9
        /// </summary>
        G1GC = 0,

        /// <summary>
        /// 串行垃圾回收器
        /// </summary>
        SerialGC = 1,

        /// <summary>
        /// 并行垃圾回收器
        /// </summary>
        ParallelGC = 2,

        /// <summary>
        /// 并发标记扫描垃圾回收器
        /// </summary>
        CMSGC = 3,
    }

    public enum LaunchType
    {
        /// <summary>
        /// 正常启动模式
        /// </summary>
        NORMAL,

        /// <summary>
        /// 安全启动模式
        /// </summary>
        SAFE,

        /// <summary>
        /// 开发者启动模式
        /// </summary>
        DEBUG,

        /// <summary>
        /// 创建快捷方式启动模式
        /// </summary>
        CREATE_SHORT
    }

    public class WindowSize
    {
        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool FullScreen { get; set; }

        /// <summary>
        /// 高px
        /// </summary>
        public ushort Height { get; set; }

        /// <summary>
        /// 宽px
        /// </summary>
        public ushort Width { get; set; }

        public WindowSize()
        {
            Height = 0;
            Width = 0;
            FullScreen = false;
        }
    }

    public class Server
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort Port { get; set; }
    }

    public class LaunchSetting
    {
        /// <summary>
        /// 启动模式
        /// </summary>
        public LaunchType LaunchType { get; set; }

        /// <summary>
        /// 验证器
        /// </summary>
        public AuthenticateResult AuthenticateResult { get; set; }

        /// <summary>
        /// 是否启动垃圾回收，默认开启
        /// </summary>
        public bool GCEnabled { get; set; } = true;

        /// <summary>
        /// 垃圾回收器种类(默认G1)
        /// </summary>
        public GCType GCType { get; set; } = GCType.G1GC;

        /// <summary>
        /// 垃圾回收器附加参数
        /// </summary>
        public string GCArgument { get; set; }

        ///// <summary>
        ///// 验证Token
        ///// </summary>
        //public string AuthenticateAccessToken { get; set; }

        ///// <summary>
        ///// 选择的角色
        ///// </summary>
        //public Net.MojangApi.Api.Uuid AuthenticateUUID { get; set; }

        ///// <summary>
        ///// 验证的用户信息
        ///// </summary>
        //public UserData AuthenticationUserData { get; set; }

        /// <summary>
        /// 启动版本
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// 最大运行内存
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// 最小启动内存
        /// </summary>
        public int MinMemory { get; set; }

        /// <summary>
        /// 启动窗口设置
        /// </summary>
        public WindowSize WindowSize { get; set; }

        /// <summary>
        /// JavaAgent
        /// </summary>
        public string JavaAgent { get; set; }

        /// <summary>
        /// 附加虚拟机启动参数
        /// </summary>
        public string AdvencedJvmArguments { get; set; }

        /// <summary>
        /// 附加游戏启动参数
        /// </summary>
        public string AdvencedGameArguments { get; set; }

        /// <summary>
        /// 设置启动时登录的服务器IP地址
        /// </summary>
        public Server LaunchToServer { get; set; }

        /// <summary>
        /// 左下角信息
        /// </summary>
        public string VersionType { get; set; }
    }
}