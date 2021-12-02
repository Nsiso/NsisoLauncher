using NsisoLauncherCore.Modules;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public class AutoAuthenticator
    {
        public string ClientToken { get; set; }

        public async Task<> Relogin(AuthenticationNode authNode, IUser user)
        {
            switch (authNode.AuthType)
            {
                case AuthenticationType.OFFLINE:
                    {
                        OfflineYggdrasilAuthenticator yggdrasilAuthenticator = new OfflineYggdrasilAuthenticator();
                    }
                    break;
                case AuthenticationType.MOJANG:
                    break;
                case AuthenticationType.NIDE8:
                    break;
                case AuthenticationType.AUTHLIB_INJECTOR:
                    break;
                case AuthenticationType.CUSTOM_SERVER:
                    break;
                case AuthenticationType.MICROSOFT:
                    break;
                default:
                    break;
            }
        }
    }
}
