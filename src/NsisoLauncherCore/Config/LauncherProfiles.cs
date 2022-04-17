using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Config
{
    public class LauncherProfiles
    {
        private ObservableDictionary<string, VersionProfile> profiles;
        /// <summary>
        /// 版本profile列表
        /// </summary>
        public ObservableDictionary<string, VersionProfile> Profiles
        {
            get
            {
                if (profiles == null)
                {
                    profiles = new ObservableDictionary<string, VersionProfile>();
                }
                return profiles;
            }
            set { profiles = value; }
        }

        public int Version { get; set; } = 3;
    }

    /// <summary>
    /// 版本profile
    /// </summary>
    public class VersionProfile
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; }

        ///// <summary>
        ///// 游戏目录
        ///// </summary>
        //public string GameDir { get; set; }

        ///// <summary>
        ///// Java参数
        ///// </summary>
        //public string JavaArgs { get; set; }

        ///// <summary>
        ///// Java路径
        ///// </summary>
        //public string JavaDir { get; set; }

        /// <summary>
        /// 上次启动时间
        /// </summary>
        public DateTime LastUsed { get; set; }

        /// <summary>
        /// 版本图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 上一个版本Id
        /// </summary>
        public string LastVersionId { get; set; }

        /// <summary>
        /// 版本名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        ///// <summary>
        ///// 窗口分辨率
        ///// </summary>
        //public Resolution Resolution { get; set; }
    }
}
