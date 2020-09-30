using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncherCore.Auth
{
    public class YggdrasilAuthenticator : IAuthenticator
    {
        public Credentials Credentials { get; set; }

        public string ProxyAuthServerAddress { get; set; }

        public List<string> AuthArgs { get; set; }

        public virtual async Task<AuthenticateResult> DoAuthenticateAsync()
        {
            try
            {
                Authenticate authenticate = new Authenticate(Credentials);
                if (ProxyAuthServerAddress != null)
                {
                    authenticate.Address = new Uri(ProxyAuthServerAddress + "/authenticate");
                }
                if (AuthArgs != null && AuthArgs.Count != 0)
                {
                    authenticate.Arguments = AuthArgs;
                }

                var result = await authenticate.PerformRequestAsync();

                if (result.IsSuccess)
                {
                    return new AuthenticateResult(AuthState.SUCCESS)
                    {
                        AccessToken = result.AccessToken,
                        SelectedProfile = result.SelectedProfile,
                        UserData = result.User,
                        Profiles = result.AvailableProfiles
                    };
                }
                else
                {
                    AuthState errState = AuthState.ERR_OTHER;
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

                    return new AuthenticateResult(errState) { Error = result.Error };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult(AuthState.ERR_INSIDE) { Error = new Net.MojangApi.Error() { ErrorMessage = ex.Message, Exception = ex } };
            }
        }

        public YggdrasilAuthenticator(Credentials credentials)
        {
            this.Credentials = credentials;
        }
    }

    public class YggdrasilTokenAuthenticator : IAuthenticator
    {
        public string AccessToken { get; set; }

        public string ProxyAuthServerAddress { get; set; }

        public List<string> AuthArgs { get; set; }

        public async Task<AuthenticateResult> DoAuthenticateAsync()
        {
            try
            {
                Validate validate = new Validate(AccessToken);
                if (ProxyAuthServerAddress != null)
                {
                    validate.Address = new Uri(ProxyAuthServerAddress + "/validate");
                }
                if (AuthArgs != null && AuthArgs.Count != 0)
                {
                    validate.Arguments = AuthArgs;
                }
                var result = await validate.PerformRequestAsync();
                if (result.IsSuccess)
                {
                    return new AuthenticateResult(AuthState.SUCCESS) { 
                        AccessToken = this.AccessToken
                    };
                }
                else
                {
                    AuthState state;
                    Refresh refresh = new Refresh(AccessToken);
                    if (ProxyAuthServerAddress != null)
                    {
                        validate.Address = new Uri(ProxyAuthServerAddress + "/refresh");
                    }
                    if (AuthArgs != null && AuthArgs.Count != 0)
                    {
                        refresh.Arguments = AuthArgs;
                    }
                    var refreshResult = await refresh.PerformRequestAsync();
                    if (refreshResult.IsSuccess)
                    {
                        this.AccessToken = refreshResult.AccessToken;
                        state = AuthState.SUCCESS;
                    }
                    else
                    {
                        state = AuthState.REQ_LOGIN;
                    }
                    return new AuthenticateResult(state)
                    {
                        AccessToken = this.AccessToken,
                        Error = refreshResult.Error
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult(AuthState.ERR_INSIDE)
                {
                    Error = new Net.MojangApi.Error() { ErrorMessage = ex.Message, Exception = ex },
                    AccessToken = this.AccessToken
                };
            }
        }

        public YggdrasilTokenAuthenticator(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("The YggTokenAuthcator's ctor accesstoken argument is null");
            }
            this.AccessToken = token;
        }
    }
}
