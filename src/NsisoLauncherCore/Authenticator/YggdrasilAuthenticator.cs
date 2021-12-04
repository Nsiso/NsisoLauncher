using Newtonsoft.Json;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
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
    public class YggdrasilAuthenticator : IYggdrasilAuthenticator, IAuthenticator
    {
        public string YggdrasilApiAddress
        {
            get { return api.ApiAddress; }
            set { api.ApiAddress = value; }
        }

        public string ClientToken { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        protected YggdrasilApi api;

        public YggdrasilAuthenticator(string client_token) : this("https://authserver.mojang.com", client_token)
        {
        }

        [JsonConstructor]
        public YggdrasilAuthenticator(string yggdrasilApiAddress, string clientToken)
        {
            api = new YggdrasilApi(yggdrasilApiAddress);
            this.ClientToken = clientToken;
        }

        public async Task<YggdrasilAuthenticateUserResult> AuthenticateAsync(string username, string password, CancellationToken cancellation = default)
        {
            AuthenticateResponse result = await api.Authenticate(new AuthenticateRequest(username, password, ClientToken), cancellation);
            if (result.IsSuccess)
            {
                return new YggdrasilAuthenticateUserResult(result, new YggdrasilUser(result.Data));
            }
            else
            {
                return new YggdrasilAuthenticateUserResult(result, null);
            }
        }

        public async Task<YggdrasilAuthenticateUserResult> RefreshAsync(YggdrasilUser user, CancellationToken cancellation = default)
        {
            RefreshRequest request = new RefreshRequest(new AccessClientTokenPair(user.GameAccessToken, ClientToken));
            TokenResponse result = await api.Refresh(request, cancellation);
            if (result.IsSuccess)
            {
                user.GameAccessToken = result.Data.AccessToken;
            }
            return new YggdrasilAuthenticateUserResult(result, user);
        }

        public async Task<YggdrasilAuthenticateResult> ValidateAsync(YggdrasilUser user, CancellationToken cancellation = default)
        {
            Response result = await api.Validate(new AccessClientTokenPair(user.GameAccessToken, ClientToken), cancellation);
            return new YggdrasilAuthenticateResult(result);
        }

        public async Task<YggdrasilAuthenticateResult> SignoutAsync(string username, string password, CancellationToken cancellation = default)
        {
            Response result = await api.Signout(new UsernamePasswordPair() { Username = username, Password = password }, cancellation);
            return new YggdrasilAuthenticateResult(result);
        }

        public async Task<YggdrasilAuthenticateResult> InvalidateAsync(YggdrasilUser user, CancellationToken cancellation = default)
        {
            Response result = await api.Invalidate(new AccessClientTokenPair(user.GameAccessToken, ClientToken), cancellation);
            return new YggdrasilAuthenticateResult(result);
        }
    }

    public class YggdrasilAuthenticateUserResult : YggdrasilAuthenticateResult
    {
        public YggdrasilUser User { get; set; }

        public YggdrasilAuthenticateUserResult(Response response, YggdrasilUser user) : base(response)
        {
            this.User = user;
        }
    }

    public class YggdrasilAuthenticateResult
    {

        public Response Response { get; set; }

        public bool IsSuccess { get => Response.IsSuccess; }

        public ResponseState State { get => Response.State; }

        public YggdrasilAuthenticateResult(Response response)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response));
        }
    }
}
