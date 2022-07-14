using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Apis;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.User;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class YggdrasilAuthenticator : IAuthenticator
    {
        public string YggdrasilApiAddress
        {
            get { return api.ApiAddress; }
            set { api.ApiAddress = value; }
        }

        public string ClientToken { get; set; }

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

        public string Name { get; set; }

        [JsonIgnore]
        public bool RequireUsername => true;
        [JsonIgnore]
        public string InputUsername { get; set; }

        [JsonIgnore]
        public bool RequirePassword => true;
        [JsonIgnore]
        public string InputPassword { get; set; }

        [JsonIgnore]
        public bool RequireRemember => true;
        [JsonIgnore]
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
                return !string.IsNullOrEmpty(SelectedUserId) ? Users[SelectedUserId] : null;
            }
        }

        [JsonIgnore]
        public bool IsOnline { get; private set; } = false;

        [JsonIgnore]
        public bool AllowAuthenticate => true;
        [JsonIgnore]
        public bool AllowRefresh => true;
        [JsonIgnore]
        public bool AllowValidate => true;
        [JsonIgnore]
        public bool AllowSignout => true;
        [JsonIgnore]
        public bool AllowInvalidate => true;

        public bool Locked { get; set; }

        public virtual List<Library> Libraries => null;

        [JsonIgnore]
        public bool IsShowLoading => true;

        public event PropertyChangedEventHandler PropertyChanged;

        public async virtual Task<AuthenticateResult> AuthenticateAsync(CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(InputUsername))
            {
                return new AuthenticateResult() { State = AuthenticateState.ERROR_CLIENT, Cause = "The input username is empty", ErrorTag = "UsernameEmpty" };
            }
            if (string.IsNullOrEmpty(InputPassword))
            {
                return new AuthenticateResult() { State = AuthenticateState.ERROR_CLIENT, Cause = "The input password is empty", ErrorTag = "PasswordEmpty" };
            }
            try
            {
                AuthenticateResponse result = await api.Authenticate(new AuthenticateRequest(InputUsername, InputPassword, ClientToken), cancellation);
                if (result.IsSuccess)
                {
                    YggdrasilUser user = new YggdrasilUser(result.Data);
                    if (Users.ContainsKey(user.UserId))
                    {
                        Users[user.UserId] = user;
                    }
                    else
                    {
                        Users.Add(user.UserId, user);
                    }
                    this.SelectedUserId = user.UserId;
                    this.IsOnline = true;
                }
                return ResponseToAuthenticateResult(result);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return new AuthenticateResult() { State = AuthenticateState.CANCELED, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
                else
                {
                    return new AuthenticateResult() { State = AuthenticateState.TIMEOUT, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult() { State = AuthenticateState.EXCEPTION, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
            }
        }

        public async virtual Task<AuthenticateResult> InvalidateAsync(CancellationToken cancellation)
        {
            if (SelectedUser == null)
            {
                throw new ArgumentNullException(nameof(SelectedUser));
            }

            try
            {
                Response result = await api.Invalidate(new AccessClientTokenPair(SelectedUser.GameAccessToken, this.ClientToken), cancellation);
                if (result.IsSuccess)
                {
                    YggdrasilUser user = (YggdrasilUser)this.SelectedUser;
                    user.GameAccessToken = null;
                    this.SelectedUserId = null;
                    this.IsOnline = false;
                }
                return ResponseToAuthenticateResult(result);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return new AuthenticateResult() { State = AuthenticateState.CANCELED, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
                else
                {
                    return new AuthenticateResult() { State = AuthenticateState.TIMEOUT, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult() { State = AuthenticateState.EXCEPTION, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
            }
        }

        public async virtual Task<AuthenticateResult> RefreshAsync(CancellationToken cancellation)
        {
            if (SelectedUser == null)
            {
                throw new ArgumentNullException(nameof(SelectedUser));
            }

            try
            {
                AuthenticateResponse result = await api.Refresh(new RefreshRequest()
                {
                    AccessToken = SelectedUser.GameAccessToken,
                    ClientToken = this.ClientToken,
                    RequestUser = false
                }, cancellation);


                if (result.IsSuccess)
                {
                    AuthenticateResponseData data = result.Data;
                    YggdrasilUser user = (YggdrasilUser)SelectedUser;
                    user.SetFromAuthenticateResponseData(data);
                    IsOnline = true;
                }
                return ResponseToAuthenticateResult(result);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return new AuthenticateResult() { State = AuthenticateState.CANCELED, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
                else
                {
                    return new AuthenticateResult() { State = AuthenticateState.TIMEOUT, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult() { State = AuthenticateState.EXCEPTION, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
            }
        }

        public async virtual Task<AuthenticateResult> SignoutAsync(CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(InputUsername))
            {
                throw new ArgumentNullException(nameof(InputUsername));
            }
            if (string.IsNullOrEmpty(InputPassword))
            {
                throw new ArgumentNullException(nameof(InputPassword));
            }
            try
            {
                Response result = await api.Signout(new AuthenticateRequest(InputUsername, InputPassword, ClientToken), cancellation);
                if (result.IsSuccess)
                {
                    this.IsOnline = false;
                }
                return ResponseToAuthenticateResult(result);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return new AuthenticateResult() { State = AuthenticateState.CANCELED, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
                else
                {
                    return new AuthenticateResult() { State = AuthenticateState.TIMEOUT, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult() { State = AuthenticateState.EXCEPTION, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
            }
        }

        public async virtual Task<AuthenticateResult> ValidateAsync(CancellationToken cancellation)
        {
            if (SelectedUser == null)
            {
                throw new ArgumentNullException(nameof(SelectedUser));
            }
            try
            {
                // if token is jwt, local validate first.
                JwtSecurityToken token;
                if (Jwt.TryParse(SelectedUser.GameAccessToken, out token))
                {
                    if (!Jwt.ValidateExp(token))
                    {
                        return new AuthenticateResult() { State = AuthenticateState.FORBIDDEN, ErrorTag = "JwtExpired", ErrorMessage = "The jwt token expired." };
                    }
                }
                Response result = await api.Validate(new AccessClientTokenPair(SelectedUser.GameAccessToken, this.ClientToken), cancellation);
                if (result.IsSuccess)
                {
                    this.IsOnline = true;
                }
                return ResponseToAuthenticateResult(result);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return new AuthenticateResult() { State = AuthenticateState.CANCELED, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
                else
                {
                    return new AuthenticateResult() { State = AuthenticateState.TIMEOUT, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticateResult() { State = AuthenticateState.EXCEPTION, ErrorTag = ex.Message, ErrorMessage = ex.ToString() };
            }
        }

        private AuthenticateResult ResponseToAuthenticateResult(Response response)
        {
            if (response.IsSuccess)
            {
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };
            }
            else
            {
                AuthenticateResult result = new AuthenticateResult();
                result.ErrorTag = result.ErrorTag;
                result.ErrorMessage = result.ErrorMessage;
                result.Cause = result.Cause;

                int code_int = (int)response.Code;
                if (response.Code == System.Net.HttpStatusCode.Forbidden)
                {
                    result.State = AuthenticateState.FORBIDDEN;
                }
                else if (code_int >= 400 && code_int < 500)
                {
                    result.State = AuthenticateState.ERROR_CLIENT;
                }
                else if (code_int >= 500 && code_int < 600)
                {
                    result.State = AuthenticateState.ERROR_SERVER;
                }
                else
                {
                    result.State = AuthenticateState.UNKNOWN;
                }

                return result;
            }
        }

        public virtual string GetExtraJvmArgument(LaunchHandler handler)
        {
            return null;
        }

        public virtual string GetExtraGameArgument(LaunchHandler handler)
        {
            return null;
        }

        public virtual Task UpdateAuthenticatorAsync(CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }
    }
}
