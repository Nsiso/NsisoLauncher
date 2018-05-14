using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace NsisoLauncher.Core.Modules
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
        SerialGC,

        /// <summary>
        /// 并行垃圾回收器
        /// </summary>
        ParallelGC,

        /// <summary>
        /// 并发标记扫描垃圾回收器
        /// </summary>
        CMSGC,
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
        /// 是否启动垃圾回收，默认开启
        /// </summary>
        public bool GCEnabled { get; set; } = true;

        /// <summary>
        /// 垃圾回收器种类
        /// </summary>
        public GCType GCType { get; set; }

        /// <summary>
        /// 垃圾回收器附加参数
        /// </summary>
        public List<string> GCArgument { get; set; }

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
        /// 附加虚拟机启动参数
        /// </summary>
        public List<string> AdvencedJvmArguments { get; set; }

        /// <summary>
        /// 附加游戏启动参数
        /// </summary>
        public List<string> AdvencedGameArguments { get; set; }

        /// <summary>
        /// 设置启动时登陆的服务器IP地址
        /// </summary>
        public Server LaunchToServer { get; set; }
    }
}