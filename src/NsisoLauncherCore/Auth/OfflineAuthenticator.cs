using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Modules.Yggdrasil;
using NsisoLauncherCore.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.Net.Yggdrasil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Auth
{
    public class OfflineAuthenticator : IAuthenticator
    {
        public string Displayname { get; private set; }
        public UserData UserData { get; private set; }
        public PlayerProfile ProfileUUID { get; private set; }

        public OfflineAuthenticator(string displayname)
        {
            this.Displayname = displayname;
            this.ProfileUUID = new PlayerProfile()
            {
                PlayerName = Displayname,
                Value = Guid.NewGuid().ToString("N")
            };
            this.UserData = new UserData()
            {
                ID = Guid.NewGuid().ToString("N")
            };
        }

        public OfflineAuthenticator(string displayname, UserData userData, string profileUUID)
        {
            this.Displayname = displayname;
            this.UserData = userData;
            this.ProfileUUID = new PlayerProfile()
            {
                PlayerName = Displayname,
                Value = profileUUID
            };
        }

        public Task<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");

                return new AuthenticateResponse
                {
                    IsSuccess = true,
                    Data = new AuthenticateResponseData()
                    {
                        AccessToken = accessToken,
                        SelectedProfile = this.ProfileUUID,
                        AvailableProfiles = new List<PlayerProfile>() { this.ProfileUUID },
                        User = this.UserData
                    }
                };
            });

        }

        public Task<TokenResponse> Refresh(RefreshRequest request)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");

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

        public Task<Response> Validate(AccessClientTokenPair data)
        {
            return Task.Factory.StartNew(() =>
            {
                return new Response
                {
                    IsSuccess = true
                };
            });
        }

        public Task<Response> Signout(UsernamePasswordPair data)
        {
            return Task.Factory.StartNew(() =>
            {
                return new Response
                {
                    IsSuccess = true
                };
            });
        }

        public Task<Response> Invalidate(AccessClientTokenPair data)
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
