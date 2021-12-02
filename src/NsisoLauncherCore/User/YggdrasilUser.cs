using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.User
{
    public class YggdrasilUser : IUser
    {
        public YggdrasilUser()
        {

        }

        public YggdrasilUser(AuthenticateResponseData data)
        {
            this.SetFromAuthenticateResponseData(data);
        }

        public void SetFromAuthenticateResponseData(AuthenticateResponseData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // handle the available profiles
            if (data.AvailableProfiles != null)
            {
                this.Profiles = new Dictionary<string, PlayerProfile>();
                foreach (var item in data.AvailableProfiles)
                {
                    this.Profiles.Add(item.Id, item);
                }
            }

            // handle the selected profile
            if (data.SelectedProfile != null)
            {
                this.SelectedProfileUuid = data.SelectedProfile?.Id;
            }

            this.GameAccessToken = data.AccessToken;

            this.UserData = data.User;
        }

        [JsonIgnore]
        public string Username => this.UserData.Username;

        [JsonIgnore]
        public string UserId => this.UserData.ID;

        /// <summary>
        /// 用户的角色列表
        /// </summary>
        public Dictionary<string, PlayerProfile> Profiles { get; set; }

        /// <summary>
        /// 选中的角色UUID
        /// </summary>
        public string SelectedProfileUuid { get; set; }

        /// <summary>
        /// 获得选中的用户角色
        /// <summary>
        [JsonIgnore]
        public PlayerProfile SelectedProfile
        {
            get
            {
                return Profiles[SelectedProfileUuid];
            }
            set
            {
                SelectedProfileUuid = value.Id;
            }
        }

        [JsonIgnore]
        public string PlayerUUID => SelectedProfileUuid;

        [JsonIgnore]
        public string Playername => SelectedProfile.PlayerName;

        public string GameAccessToken { get; set; }

        [JsonIgnore]
        public List<UserProperty> Properties => this.UserData.Properties;

        [JsonIgnore]
        public string UserType => "mojang";

        /// <summary>
        /// 用户数据
        /// </summary>
        public virtual UserData UserData { get; set; }
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
        public List<UserProperty> Properties { get; set; }
    }
}
