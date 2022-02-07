using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.User;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class OfflineYggdrasilAuthenticator : IAuthenticator
    {
        public string Name { get; set; }

        public bool RequireUsername => true;
        public string InputUsername { get; set; }

        public bool RequirePassword => false;
        public string InputPassword { get; set; }

        public bool RequireRemember => true;
        public bool InputRemember { get; set; }

        public ObservableDictionary<string, IUser> Users { get; set; }

        private string _selectedUserId;
        public string SelectedUserId
        {
            get
            {
                return _selectedUserId;
            }
            set
            {
                this._selectedUserId = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedUserId)));
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedUser)));
            }
        }

        [JsonIgnore]
        public IUser SelectedUser
        {
            get
            {
                if (!string.IsNullOrEmpty(SelectedUserId))
                {
                    return Users[SelectedUserId];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.SelectedUserId = value?.UserId;
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedUserId)));
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedUser)));
            }
        }

        public bool AllowAuthenticate => true;
        public bool AllowRefresh => true;
        public bool AllowValidate => true;
        public bool AllowSignout => false;
        public bool AllowInvalidate => true;

        public bool Locked { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task<AuthenticateResult> AuthenticateAsync(CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");
                string profileId = Guid.NewGuid().ToString("N");
                string userId = Guid.NewGuid().ToString("N");

                PlayerProfile profile = new PlayerProfile()
                {
                    Id = profileId,
                    PlayerName = this.InputUsername
                };

                UserData userData = new UserData()
                {
                    ID = userId,
                    Username = this.InputUsername
                };

                YggdrasilUser user = new YggdrasilUser()
                {
                    UserData = userData,
                    GameAccessToken = accessToken,
                    Profiles = new Dictionary<string, PlayerProfile>() { { profileId, profile } },
                    SelectedProfileId = profileId,
                };
                Users.Add(user.UserId, user);
                this.SelectedUser = user;
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };
            });
        }

        public Task<AuthenticateResult> InvalidateAsync(CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                YggdrasilUser user = (YggdrasilUser)this.SelectedUser;
                user.GameAccessToken = null;
                this.SelectedUser = null;
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };
            });
        }

        public Task<AuthenticateResult> RefreshAsync(CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");
                YggdrasilUser user = (YggdrasilUser)SelectedUser;
                user.GameAccessToken = accessToken;
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };
            });
        }

        public Task<AuthenticateResult> SignoutAsync(CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> ValidateAsync(CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };
            });
        }
    }
}
