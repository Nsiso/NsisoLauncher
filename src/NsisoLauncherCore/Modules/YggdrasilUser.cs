using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public class YggdrasilUser : IUser
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

        public string LaunchAccessToken => AccessToken;

        public string LaunchUuid => SelectedProfileUuid;

        public string LaunchPlayerName => SelectedProfile?.PlayerName;

        public string UserType => "mojang";

        public List<UserData.Property> Properties => this.UserData.Properties;

        public string UserId => this.UserData.ID;

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

        public YggdrasilUser()
        {
            Profiles = new Dictionary<string, PlayerProfile>();
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
        public string ID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// 此用户的属性
        /// </summary>
        [JsonProperty("properties")]
        public List<Property> Properties { get; set; }

        /// <summary>
        /// 代表一个用户属性
        /// </summary>
        public class Property
        {
            /// <summary>
            /// Property name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Property value
            /// </summary>
            [JsonProperty("value")]
            public string Value { get; set; }
        }

    }
}
