using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;

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

        private SelectedUser selectedUser;
        /// <summary>
        /// 选中的用户信息
        /// </summary>
        public SelectedUser SelectedUser
        {
            get
            {
                if (selectedUser == null)
                {
                    selectedUser = new SelectedUser();
                }
                return selectedUser;
            }
            set { selectedUser = value; }
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
        /// 游戏目录
        /// </summary>
        public string GameDir { get; set; }

        /// <summary>
        /// Java参数
        /// </summary>
        public string JavaArgs { get; set; }

        /// <summary>
        /// Java路径
        /// </summary>
        public string JavaDir { get; set; }

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

        /// <summary>
        /// 窗口分辨率
        /// </summary>
        public Resolution Resolution { get; set; }
    }

    /// <summary>
    /// 用户
    /// </summary>
    public class User
    {
        /// <summary>
        /// 验证token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 用户角色列表
        /// </summary>
        public ObservableDictionary<string, UserProfile> Profiles { get; set; }

        /// <summary>
        /// 用户属性
        /// </summary>
        public List<UserProperty> Properties { get; set; }
    }

    /// <summary>
    /// 角色
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// 显示的名字
        /// </summary>
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// 代表一个用户属性
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public string Value { get; internal set; }
    }

    public class SelectedUser
    {
        /// <summary>
        /// 选中的用户
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 选中的角色
        /// </summary>
        public string Profile { get; set; }
    }
}
