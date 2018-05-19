using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NsisoLauncher.Core.Util
{
    public class LaunchResult
    {
        /// <summary>
        /// 游戏进程
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        /// 启动参数
        /// </summary>
        public string LaunchArguments { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; } = false;
    }
}
