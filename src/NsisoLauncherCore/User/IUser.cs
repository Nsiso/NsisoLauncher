using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.User
{
    public interface IUser
    {
        /// <summary>
        /// The user account's user name.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// The user account's id.
        /// </summary>
        string UserId{ get; }

        /// <summary>
        /// The account now player uuid
        /// </summary>
        string PlayerUUID { get; }

        /// <summary>
        /// The account now player name
        /// </summary>
        string Playername { get; }

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
