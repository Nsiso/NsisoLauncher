using Newtonsoft.Json;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Client;

namespace NsisoLauncherCore.User
{
    public class MicrosoftUser : IUser
    {
        public IAccount MicrosoftAccount { get; set; }

        [JsonIgnore]
        public string Username => MicrosoftAccount.Username;

        [JsonIgnore]
        public string UserId => MicrosoftAccount.HomeAccountId.Identifier;

        public string PlayerUUID { get; set; }

        public string Playername { get; set; }

        public string GameAccessToken { get => MinecraftToken.AccessToken; }

        public MinecraftToken MinecraftToken { get; set; }


        [JsonIgnore]
        public List<UserProperty> Properties => null;

        [JsonIgnore]
        public string UserType => "msa";
    }
}
