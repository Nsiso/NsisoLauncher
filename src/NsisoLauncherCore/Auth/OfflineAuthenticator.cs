using NsisoLauncherCore.Modules;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.Net.Yggdrasil;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Auth
{
    public class OfflineAuthenticator : IAuthenticator
    {
        public YggdrasilUser User { get; set; }

        public OfflineAuthenticator(string username, string displayname)
        {
            string uuidValue = Guid.NewGuid().ToString("N");
            string userId = Guid.NewGuid().ToString("N");
            YggdrasilUser user = new YggdrasilUser()
            {
                Username = username,
                AccessToken = Guid.NewGuid().ToString("N"),
                Profiles = new Dictionary<string, PlayerProfile>() { { uuidValue, new PlayerProfile() { PlayerName = displayname, Id = uuidValue } } },
                SelectedProfileUuid = uuidValue,
                UserData = new UserData() { ID = userId, Username = username }
            };
            this.User = user;
        }

        public OfflineAuthenticator(YggdrasilUser user)
        {
            this.User = user;
        }

        public Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");
                User.AccessToken = accessToken;
                return new AuthenticateResponse
                {
                    IsSuccess = true,
                    Data = new AuthenticateResponseData()
                    {
                        AccessToken = accessToken,
                        SelectedProfile = User.SelectedProfile,
                        AvailableProfiles = new List<PlayerProfile>() { this.User.SelectedProfile },
                        User = this.User.UserData
                    }
                };
            });

        }

        public Task<TokenResponse> Refresh(RefreshRequest request, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");
                User.AccessToken = accessToken;

                var res = new Response
                {
                    IsSuccess = true
                };
                return new TokenResponse(res)
                {
                    Data = new AccessClientTokenPair()
                    {
                        AccessToken = accessToken
                    }
                };
            });
        }

        public Task<Response> Validate(AccessClientTokenPair data, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new Response
                {
                    IsSuccess = true
                };
            });
        }

        public Task<Response> Signout(UsernamePasswordPair data, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new Response
                {
                    IsSuccess = true
                };
            });
        }

        public Task<Response> Invalidate(AccessClientTokenPair data, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new Response
                {
                    IsSuccess = true
                };
            });
        }
    }
}
