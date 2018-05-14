using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NsisoLauncher.Core.Net.MojangApi.Endpoints;
using NsisoLauncher.Core.Net.MojangApi.Responses;

namespace NsisoLauncher.Core.Auth
{
    public enum AuthType
    {
        OFFLINE = 0,
        ONLINE,
        THIRD_PART
    }

    public interface IAuth
    {
        AuthType AuthType { get; }

        Credentials Credentials { get; set; }

        Task<AuthenticateResponse> DoAuthenticationAsync();
    }
}