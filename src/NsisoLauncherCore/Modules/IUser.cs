using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public interface IUser
    {
        string GetLaunchAccessToken();

        string GetLaunchUuid();

        string GetLaunchPlayerName();

        UserData GetUserData();

        bool? IsLegacy();
    }
}
