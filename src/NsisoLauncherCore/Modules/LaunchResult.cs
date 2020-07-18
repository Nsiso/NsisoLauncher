using System;
using System.Diagnostics;

namespace NsisoLauncherCore.Modules
{
    public class LaunchResult
    {
        public LaunchResult()
        {
        }

        public LaunchResult(LaunchException.LaunchException ex)
        {
            Process = null;
            LaunchArguments = null;
            IsSuccess = false;
            LaunchException = ex;
        }

        public LaunchResult(Exception ex)
        {
            Process = null;
            LaunchArguments = null;
            IsSuccess = false;
            LaunchException = new LaunchException.LaunchException(ex);
        }

        /// <summary>
        ///     游戏进程
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        ///     启动参数
        /// </summary>
        public string LaunchArguments { get; set; }

        /// <summary>
        ///     是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        ///     启动时发生的意外
        /// </summary>
        public LaunchException.LaunchException LaunchException { get; set; }

        /// <summary>
        ///     启动使用的时间
        /// </summary>
        public long LaunchUsingMs { get; set; }
    }
}