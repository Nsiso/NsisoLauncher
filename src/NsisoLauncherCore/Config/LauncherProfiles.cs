using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Config
{
    public class LauncherProfiles
    {
        private ObservableDictionary<string,User> authenticationDatabase;
        /// <summary>
        /// 验证数据库
        /// </summary>
        public ObservableDictionary<string, User> AuthenticationDatabase
        {
            get 
            {
                if (authenticationDatabase == null)
                {
                    authenticationDatabase = new ObservableDictionary<string, User>();
                }
                return authenticationDatabase;
            }
            set { authenticationDatabase = value; }
        }

        /// <summary>
        /// 用户端Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// 选中的版本profile
        /// </summary>
        public string SelectedProfile { get; set; }

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
    }
}
