using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NsisoLauncherCore.Net.MojangApi;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncherCore.Auth
{
    public class YggdrasilAuthenticator : IAuthenticator
    {
        public YggdrasilAuthenticator(Credentials credentials)
        {
            Credentials = credentials;
        }

        public Credentials Credentials { get; set; }

        public string ProxyAuthServerAddress { get; set; }

        public List<string> AuthArgs { get; set; }

        public virtual async Task<AuthenticateResult> DoAuthenticateAsync()
        {
            try
            {
                var authenticate = new Authenticate(Credentials);
                if (ProxyAuthServerAddress != null)
                    authenticate.Address = new Uri(ProxyAuthServerAddress + "/authenticate");
                if (AuthArgs != null && AuthArgs.Count != 0) authenticate.Arguments = AuthArgs;

                var result = await authenticate.PerformRequestAsync();

                if (result.IsSuccess)
                    return new AuthenticateResult(AuthState.SUCCESS)
                    {
                        AccessToken = result.AccessToken,
                        SelectedProfileUUID = result.SelectedProfile,
                        UserData = result.User,
                        Profiles = result.AvailableProfiles
                    };

                var errState = AuthState.ERR_OTHER;
                switch (result.Code)
                {
                    case HttpStatusCode.MethodNotAllowed:
                        errState = AuthState.ERR_METHOD_NOT_ALLOW;
                        break;
                    case HttpStatusCode.NotFound:
                        errState = AuthState.ERR_NOTFOUND;
                        break;
                    case HttpStatusCode.Forbidden:
                        errState = AuthState.ERR_INVALID_CRDL;
                        break;
                    default:
                        errState = AuthState.ERR_OTHER;
                        break;
                }

                return new AuthenticateResult(errState) {Error = result.Error};
            }
            catch (Exception ex)
            {
                return new AuthenticateResult(AuthState.ERR_INSIDE)
                    {Error = new Error {ErrorMessage = ex.Message, Exception = ex}};
            }
        }
    }

    public class YggdrasilTokenAuthenticator : IAuthenticator
    {
        public YggdrasilTokenAuthenticator(string token, Uuid selectedProfileUUID, UserData userData)
        {
            AccessToken = token;
            SelectedProfileUUID = selectedProfileUUID;
            UserData = userData;
        }

        public string AccessToken { get; set; }

        public string ProxyAuthServerAddress { get; set; }

        public List<string> AuthArgs { get; set; }

        public Uuid SelectedProfileUUID { get; set; }

        public UserData UserData { get; set; }

        public async Task<AuthenticateResult> DoAuthenticateAsync()
        {
            try
            {
                var validate = new Validate(AccessToken);
                if (ProxyAuthServerAddress != null) validate.Address = new Uri(ProxyAuthServerAddress + "/validate");
                if (AuthArgs != null && AuthArgs.Count != 0) validate.Arguments = AuthArgs;
                var result = await validate.PerformRequestAsync();
                if (result.IsSuccess)
                    return new AuthenticateResult(AuthState.SUCCESS)
                        {AccessToken = AccessToken, UserData = UserData, SelectedProfileUUID = SelectedProfileUUID};

                AuthState state;
                var refresh = new Refresh(AccessToken);
                if (ProxyAuthServerAddress != null) validate.Address = new Uri(ProxyAuthServerAddress + "/refresh");
                if (AuthArgs != null && AuthArgs.Count != 0) refresh.Arguments = AuthArgs;
                var refreshResult = await refresh.PerformRequestAsync();
                if (refreshResult.IsSuccess)
                {
                    AccessToken = refreshResult.AccessToken;
                    state = AuthState.SUCCESS;
                }
                else
                {
                    state = AuthState.REQ_LOGIN;
                }

                return new AuthenticateResult(state)
                {
                    AccessToken = AccessToken = AccessToken,
                    UserData = UserData,
                    SelectedProfileUUID = SelectedProfileUUID,
                    Error = refreshResult.Error
                };
            }
            catch (Exception ex)
            {
                return new AuthenticateResult(AuthState.ERR_INSIDE)
                {
                    Error = new Error {ErrorMessage = ex.Message, Exception = ex},
                    AccessToken = AccessToken = AccessToken,
                    UserData = UserData,
                    SelectedProfileUUID = SelectedProfileUUID
                };
            }
        }
    }
}