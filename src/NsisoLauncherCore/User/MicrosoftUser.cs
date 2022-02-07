using Newtonsoft.Json;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Client;
using NsisoLauncherCore.Modules;
using System.ComponentModel;

namespace NsisoLauncherCore.User
{
    public class MicrosoftUser : IUser
    {
        public IAccount MicrosoftAccount { get; set; }

        [JsonIgnore]
        public string Username => MicrosoftAccount.Username;

        [JsonIgnore]
        public string UserId => MicrosoftAccount.HomeAccountId.Identifier;

        [JsonIgnore]
        public string GameAccessToken { get => MinecraftToken.AccessToken; }

        public MinecraftToken MinecraftToken { get; set; }

        public MicrosoftPlayerProfile Profile { get; set; }

        [JsonIgnore]
        public List<UserProperty> Properties => null;

        [JsonIgnore]
        public string UserType => "msa";

        [JsonIgnore]
        public PlayerProfile SelectedProfile => Profile;

        [JsonIgnore]
        public string SelectedProfileId { get { return Profile?.Id; } set { } }

        [JsonIgnore]
        public Dictionary<string, PlayerProfile> Profiles { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public MicrosoftUser(IAccount microsoftAccount, MinecraftToken minecraftToken, MicrosoftPlayerProfile profile)
        {
            this.MicrosoftAccount = microsoftAccount;
            this.MinecraftToken = minecraftToken;
            this.Profile = profile;
            if (Profile != null)
            {
                this.Profiles = new Dictionary<string, PlayerProfile>();
                this.Profiles.Add(profile.Id, Profile);
            }
        }
    }
}
