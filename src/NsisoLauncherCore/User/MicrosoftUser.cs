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
        public string Username { get; set; }

        public string UserId { get; set; }

        [JsonIgnore]
        public string GameAccessToken { get => MinecraftToken.AccessToken; }

        public string XboxUserId { get; set; }

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

        public MicrosoftUser()
        {

        }

        public MicrosoftUser(IAccount microsoftAccount, MinecraftToken minecraftToken, MicrosoftPlayerProfile profile)
        {
            this.Username = microsoftAccount.Username;
            this.UserId = microsoftAccount.HomeAccountId.Identifier;
            this.MinecraftToken = minecraftToken;
            this.XboxUserId = minecraftToken.XboxUserId;
            this.Profile = profile;
            if (Profile != null)
            {
                this.Profiles = new Dictionary<string, PlayerProfile>();
                this.Profiles.Add(profile.Id, Profile);
            }
        }
    }
}
