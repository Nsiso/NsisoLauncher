using NsisoLauncherCore.Util;
using System;

namespace NsisoLauncherCore.Modules
{
    public class LaunchResult
    {
        /// <summary>
        /// 游戏进程
        /// </summary>
        public LaunchInstance Instance { get; set; }

        /// <summary>
        /// The minecraft version to launch
        /// </summary>
        public VersionBase Version { get; set; }

        /// <summary>
        /// 启动使用的设置
        /// </summary>
        public LaunchSetting Setting { get; set; }

        /// <summary>
        /// 启动所使用的java
        /// </summary>
        public Java UsingJava { get; set; }

        /// <summary>
        /// 启动参数
        /// </summary>
        public string LaunchArguments { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; } = false;

        /// <summary>
        /// 启动时发生的意外
        /// </summary>
        public LaunchException.LaunchException LaunchException { get; set; }

        /// <summary>
        /// 启动使用的时间
        /// </summary>
        public long LaunchUsingMs { get; set; }

        public void SetException(Exception ex)
        {
            this.IsSuccess = false;
            this.LaunchException = new LaunchException.LaunchException(ex);
        }

        public void SetException(LaunchException.LaunchException ex)
        {
            this.IsSuccess = false;
            this.LaunchException = ex;
        }

        public void SetSuccess()
        {
            this.IsSuccess = true;
            this.LaunchException = null;
        }

        public LaunchResult() { }

        public LaunchResult(LaunchException.LaunchException ex)
        {
            SetException(ex);
        }

        public LaunchResult(Exception ex)
        {
            SetException(ex);
        }
    }
}
