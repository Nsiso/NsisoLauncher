using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NsisoLauncherCore.Util;
using Newtonsoft.Json;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Authenticator
{
    public interface IAuthenticator : INotifyPropertyChanged
    {
        #region Own property
        /// <summary>
        /// The authenticator's name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The authenticator if is locked on (to avoid any change)
        /// </summary>
        bool Locked { get; set; }
        #endregion

        #region Username, password, remember property
        /// <summary>
        /// The authenticator is require username input
        /// </summary>
        bool RequireUsername { get; }

        /// <summary>
        /// The input username
        /// </summary>
        string InputUsername { get; set; }


        /// <summary>
        /// The authenticator is require password input
        /// </summary>
        bool RequirePassword { get; }

        /// <summary>
        /// The input password
        /// </summary>
        string InputPassword { get; set; }

        /// <summary>
        /// The authenticator is require remember input
        /// </summary>
        bool RequireRemember { get; }

        /// <summary>
        /// The input if is remember
        /// </summary>
        bool InputRemember { get; set; }
        #endregion

        #region Users property
        /// <summary>
        /// The users dictionary
        /// </summary>
        ObservableDictionary<string, IUser> Users { get; }

        /// <summary>
        /// Currently selected user's id (key for dictionary)
        /// </summary>
        string SelectedUserId { get; set; }

        /// <summary>
        /// Currently selected user (value for dictionary)
        /// </summary>
        IUser SelectedUser { get; }

        /// <summary>
        /// The user'current state of is online.
        /// </summary>
        bool IsOnline { get; }
        #endregion

        #region Allow
        /// <summary>
        /// The authenticator allow Authenticate method
        /// </summary>
        bool AllowAuthenticate { get; }

        /// <summary>
        /// The authenticator allow Refresh method
        /// </summary>
        bool AllowRefresh { get; }

        /// <summary>
        /// The authenticator allow Validate method
        /// </summary>
        bool AllowValidate { get; }

        /// <summary>
        /// The authenticator allow Signout method
        /// </summary>
        bool AllowSignout { get; }

        /// <summary>
        /// The authenticator allow Invalidate method
        /// </summary>
        bool AllowInvalidate { get; }
        #endregion

        #region Launch depend property and method
        /// <summary>
        /// The authenticator requires extra java virtual machine argument
        /// </summary>
        string GetExtraJvmArgument(LaunchHandler handler);

        /// <summary>
        /// The authenticator requires extra game argument
        /// </summary>
        string GetExtraGameArgument(LaunchHandler handler);

        /// <summary>
        /// The authenticator requires libraries
        /// </summary>
        List<Library> Libraries { get; }
        #endregion

        Task UpdateAuthenticatorAsync(CancellationToken cancellation);

        #region All authenticate methods
        /// <summary>
        /// Authenticates a user (login).
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>result</returns>
        Task<AuthenticateResult> AuthenticateAsync(CancellationToken cancellation);

        /// <summary>
        /// Refreshes a valid selected user.
        /// It can be used to keep a selected user logged in between gaming sessions and is preferred over storing the user's password in a file.
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>result</returns>
        Task<AuthenticateResult> RefreshAsync(CancellationToken cancellation);

        /// <summary>
        /// Checks if selected user is usable for authentication.
        /// The Minecraft Launcher calls this endpoint on startup to verify that its saved token is still usable, and calls refresh if this returns an error.
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>result</returns>
        Task<AuthenticateResult> ValidateAsync(CancellationToken cancellation);

        /// <summary>
        /// Invalidates selected user (logout).
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>result</returns>
        Task<AuthenticateResult> InvalidateAsync(CancellationToken cancellation);

        /// <summary>
        /// Invalidates selected user's all sessions).
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<AuthenticateResult> SignoutAsync(CancellationToken cancellation);
    }
    #endregion


    public class AuthenticateResult
    {
        public AuthenticateState State { get; set; }

        public bool IsSuccess { get => State == AuthenticateState.SUCCESS; }

        public string ErrorTag { get; set; }

        public string ErrorMessage { get; set; }

        public string Cause { get; set; }
    }

    public enum AuthenticateState
    {
        /// <summary>
        /// Success to authenticate
        /// </summary>
        SUCCESS,

        /// <summary>
        /// Forbidden to authenticate (wrong username or password)
        /// </summary>
        FORBIDDEN,

        /// <summary>
        /// Server responsed that is a server error
        /// </summary>
        ERROR_SERVER,

        /// <summary>
        /// Server responsed that is a client error
        /// </summary>
        ERROR_CLIENT,

        /// <summary>
        /// The authenticate is canceled by user
        /// </summary>
        CANCELED,

        /// <summary>
        /// Can not get response from server because net time out
        /// </summary>
        TIMEOUT,

        /// <summary>
        /// Inner exception
        /// </summary>
        EXCEPTION,

        /// <summary>
        /// Unknown error
        /// </summary>
        UNKNOWN
    }
}
