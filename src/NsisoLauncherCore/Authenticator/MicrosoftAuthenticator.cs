using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
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
    public class MicrosoftAuthenticator : IAuthenticator
    {
        private OAuthFlow oAuthFlower;
        private XboxliveAuth xboxliveAuther;
        private MinecraftServices mcServices;

        public MicrosoftAuthenticator()
        {
            oAuthFlower = new OAuthFlow();
            xboxliveAuther = new XboxliveAuth();
            mcServices = new MinecraftServices();
        }

        //public string Name { get; set; }

        //public async Task<MinecraftToken> LoginGetMinecraftToken(CancellationToken cancellation = default)
        //{
        //    AuthenticationResult result = await oAuthFlower.Login(cancellation);
        //    XboxLiveToken xbox_result = await xboxliveAuther.Authenticate(result.AccessToken, cancellation);
        //    MinecraftToken mc_result = await mcServices.Authenticate(xbox_result, cancellation);

        //    return mc_result;
        //}

        //public async Task<MinecraftToken> RefreshGetMinecraftToken(IAccount account, CancellationToken cancellation = default)
        //{
        //    AuthenticationResult result = await oAuthFlower.Login(account, cancellation);
        //    XboxLiveToken xbox_result = await xboxliveAuther.Authenticate(result.AccessToken, cancellation);
        //    MinecraftToken mc_result = await mcServices.Authenticate(xbox_result, cancellation);

        //    return mc_result;
        //}
        public string Name { get; set; }

        [JsonIgnore]
        public bool RequireUsername => false;
        [JsonIgnore]
        public string InputUsername { get; set; }

        [JsonIgnore]
        public bool RequirePassword => false;
        [JsonIgnore]
        public string InputPassword { get; set; }

        [JsonIgnore]
        public bool RequireRemember => true;
        [JsonIgnore]
        public bool InputRemember { get; set; }

        public ObservableDictionary<string, User.IUser> Users { get; set; }

        public string SelectedUserId { get; set; }
        [JsonIgnore]
        public User.IUser SelectedUser
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedUserId) ? Users[SelectedUserId] : null;
            }
        }

        [JsonIgnore]
        public bool AllowAuthenticate => true;
        [JsonIgnore]
        public bool AllowRefresh => true;
        [JsonIgnore]
        public bool AllowValidate => false;
        [JsonIgnore]
        public bool AllowSignout => false;
        [JsonIgnore]
        public bool AllowInvalidate => false;

        public bool Locked { get; set; }

        [JsonIgnore]
        public List<Library> Libraries => null;

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<AuthenticateResult> AuthenticateAsync(CancellationToken cancellation)
        {
            try
            {
                AuthenticationResult result = await oAuthFlower.Login(cancellation);
                XboxLiveToken xbox_result = await xboxliveAuther.Authenticate(result.AccessToken, cancellation);
                MinecraftToken mc_result = await mcServices.Authenticate(xbox_result, cancellation);
                MicrosoftPlayerProfile profile = null;
                if (await mcServices.CheckHaveGameOwnership(mc_result, cancellation))
                {
                    profile = await mcServices.GetProfile(result.AccessToken, mc_result, cancellation).ConfigureAwait(false);
                }
                MicrosoftUser user = new MicrosoftUser(result.Account, mc_result, profile);
                this.SelectedUserId = user.UserId;
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };

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

        public async Task<AuthenticateResult> RefreshAsync(CancellationToken cancellation)
        {
            try
            {
                MicrosoftUser user = (MicrosoftUser)SelectedUser;
                AuthenticationResult result = await oAuthFlower.Login(user.MicrosoftAccount, cancellation);
                XboxLiveToken xbox_result = await xboxliveAuther.Authenticate(result.AccessToken, cancellation);
                MinecraftToken mc_result = await mcServices.Authenticate(xbox_result, cancellation);
                return new AuthenticateResult() { State = AuthenticateState.SUCCESS };
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

        public Task<AuthenticateResult> InvalidateAsync(CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> SignoutAsync(CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> ValidateAsync(CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public string GetExtraJvmArgument(LaunchHandler handler)
        {
            return null;
        }

        public string GetExtraGameArgument(LaunchHandler handler)
        {
            return null;
        }

        public Task UpdateAuthenticatorAsync(CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }
    }
}
