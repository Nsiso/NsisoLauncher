using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class OfflineYggdrasilAuthenticator : IYggdrasilAuthenticator
    {
        public Task<YggdrasilAuthenticateUserResult> AuthenticateAsync(string username, string password, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");
                string profileId = Guid.NewGuid().ToString("N");
                string userId = Guid.NewGuid().ToString("N");

                PlayerProfile profile = new PlayerProfile()
                {
                    Id = profileId,
                    PlayerName = username
                };

                UserData userData = new UserData()
                {
                    ID = userId,
                    Username = username
                };

                YggdrasilUser user = new YggdrasilUser()
                {
                    UserData = userData,
                    GameAccessToken = accessToken,
                    SelectedProfileUuid = profileId,
                    Profiles = new Dictionary<string, PlayerProfile>() { { profileId, profile } }
                };

                return new YggdrasilAuthenticateUserResult(new Response(ResponseState.SUCCESS), user);
            });
        }

        public Task<YggdrasilAuthenticateResult> InvalidateAsync(YggdrasilUser user, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new YggdrasilAuthenticateResult(new Response(ResponseState.SUCCESS));
            });
        }

        public Task<YggdrasilAuthenticateUserResult> RefreshAsync(YggdrasilUser user, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string accessToken = Guid.NewGuid().ToString("N");
                user.GameAccessToken = accessToken;
                return new YggdrasilAuthenticateUserResult(new Response(ResponseState.SUCCESS), user);
            });
        }

        public Task<YggdrasilAuthenticateResult> SignoutAsync(string username, string password, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new YggdrasilAuthenticateResult(new Response(ResponseState.SUCCESS));
            });
        }

        public Task<YggdrasilAuthenticateResult> ValidateAsync(YggdrasilUser user, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(() =>
            {
                return new YggdrasilAuthenticateResult(new Response(ResponseState.SUCCESS));
            });
        }
    }
}
