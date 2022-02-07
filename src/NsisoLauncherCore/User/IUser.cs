using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.User
{
    public interface IUser : INotifyPropertyChanged
    {
        /// <summary>
        /// The user account's user name.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// The user account's id.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// The selected profile
        /// </summary>
        PlayerProfile SelectedProfile { get; }

        string SelectedProfileId { get; set; }

        /// <summary>
        /// All the profiles
        /// </summary>
        Dictionary<string, PlayerProfile> Profiles { get; }

        /// <summary>
        /// Minecraft's access token
        /// </summary>
        string GameAccessToken { get; }

        /// <summary>
        /// The user's property
        /// </summary>
        List<UserProperty> Properties { get; }

        /// <summary>
        /// The user's type
        /// </summary>
        string UserType { get; }
    }
}
