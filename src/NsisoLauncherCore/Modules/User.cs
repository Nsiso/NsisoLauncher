using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public class User
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 验证Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 用户的角色列表
        /// </summary>
        public virtual Dictionary<string, PlayerProfile> Profiles { get; set; }

        /// <summary>
        /// 选中的角色UUID
        /// </summary>
        public virtual string SelectedProfileUuid { get; set; }

        /// <summary>
        /// 获得选中的用户角色
        /// <summary>
        [JsonIgnore]
        public PlayerProfile SelectedProfile { get => GetSelectProfile(); }

        /// <summary>
        /// 用户数据
        /// </summary>
        public virtual UserData UserData { get; set; }

        private PlayerProfile GetSelectProfile()
        {
            if (string.IsNullOrWhiteSpace(SelectedProfileUuid))
            {
                return null;
            }
            if (Profiles.ContainsKey(SelectedProfileUuid))
            {
                return Profiles[SelectedProfileUuid];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 表示由requestUser选项发送的数据
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// User ID
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; internal set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// 此用户的属性
        /// </summary>
        [JsonProperty("properties")]
        public List<Property> Properties { get; internal set; }

        //[JsonProperty("registerIp")]
        //public string RegisterIp { get; set; }

        //[JsonProperty("registeredAt")]
        //[JsonConverter(typeof(UnixDateTimeConverter))]
        //public DateTime? RegisteredAt { get; set; }

        //[JsonProperty("passwordChangedAt")]
        //[JsonConverter(typeof(UnixDateTimeConverter))]
        //public DateTime? PasswordChangedAt { get; set; }

        //[JsonProperty("dateOfBirth")]
        //[JsonConverter(typeof(UnixDateTimeConverter))]
        //public DateTime? DateOfBirth { get; set; }

        //[JsonProperty("suspended")]
        //public bool? Suspended { get; set; }

        //[JsonProperty("blocked")]
        //public bool? Blocked { get; set; }

        //[JsonProperty("secured")]
        //public bool? Secured { get; set; }

        //[JsonProperty("migrated")]
        //public bool? Migrated { get; set; }

        //[JsonProperty("emailVerified")]
        //public bool? EmailVerified { get; set; }

        //[JsonProperty("legacyUser")]
        //public bool? LegacyUser { get; set; }

        //[JsonProperty("verifiedByParent")]
        //public bool? VerifiedByParent { get; set; }

        /// <summary>
        /// 代表一个用户属性
        /// </summary>
        public class Property
        {
            /// <summary>
            /// Property name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; internal set; }

            /// <summary>
            /// Property value
            /// </summary>
            [JsonProperty("value")]
            public string Value { get; internal set; }
        }

    }
}
