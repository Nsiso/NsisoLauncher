using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public interface IAuthenticator
    {
        string Name { get; set; }
    }

    public class AuthenticateResult
    {

    }

    public enum AuthenticateState
    {
        SUCCESS,
        CANCELED,
        FAILED,
        ERROR_INSIDE,
        TIMEOUT
    }
}
