using NsisoLauncherCore.User;
using NsisoLauncherCore.Util;
using System.Collections.Generic;

namespace NsisoLauncherCore.Modules
{
    public class LaunchSetting
    {
        /// <summary>
        /// 启动指定的java
        /// </summary>
        public Java UsingJava { get; set; }

        /// <summary>
        /// 启动模式
        /// </summary>
        public LaunchType LaunchType { get; set; }

        /// <summary>
        /// 验证器
        /// </summary>
        public IUser LaunchUser { get; set; }

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

        /// <summary>
        /// Is enable auto memory set.
        /// </summary>
        public bool AutoMemory { get; set; }

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
        public Resolution WindowSize { get; set; }

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

        /// <summary>
        /// 游戏使用的代理服务器
        /// </summary>
        public Proxy GameProxy { get; set; }
    }
}