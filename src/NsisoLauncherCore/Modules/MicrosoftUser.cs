using Newtonsoft.Json;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace NsisoLauncherCore.Modules
{
    public class MicrosoftUser : IUser
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 皮肤列表
        /// </summary>
        public List<MicrosoftSkin> Skins { get; set; }

        /// <summary>
        /// 披风列表
        /// </summary>
        public List<string> Capes { get; set; }

        /// <summary>
        /// 微软token
        /// </summary>
        public MicrosoftToken MicrosoftToken { get; set; }

        /// <summary>
        /// 游戏验证token
        /// </summary>
        public MinecraftToken MinecraftToken { get; set; }

        public string DisplayUsername => Name;

        public string LaunchAccessToken => this.MinecraftToken.AccessToken;

        public string LaunchUuid => this.Id;

        public string LaunchPlayerName => this.Name;

        public string UserType => "msa";

        public List<UserData.Property> Properties => null;

        public string UserId => this.MicrosoftToken.User_id;

        public Dictionary<string, PlayerProfile> Profiles { get => null; set { } }
        public string SelectedProfileUuid { get => null; set { } }
    }
}
