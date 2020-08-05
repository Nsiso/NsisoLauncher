using Newtonsoft.Json;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    /// <summary>
    /// 用户
    /// </summary>
    public class User
    {
        /// <summary>
        /// 验证token
        /// </summary>
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 用户角色列表
        /// </summary>
        [JsonProperty("profiles")]
        public ObservableDictionary<string,Profile> Profiles { get; set; }

        /// <summary>
        /// 用户属性
        /// </summary>
        [JsonProperty("properties")]
        public List<UserProperty> Properties { get; set; }
    }

    /// <summary>
    /// 角色
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// 
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
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// 属性值
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; internal set; }
    }
}
