using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NsisoLauncherCore.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Modules.Yggdrasil;

namespace NsisoLauncherCore.Net.Yggdrasil
{
    public interface IAuthenticator
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest request);

        Task<TokenResponse> Refresh(RefreshRequest request);

        Task<Response> Validate(AccessClientTokenPair data);

        Task<Response> Signout(UsernamePasswordPair data);

        Task<Response> Invalidate(AccessClientTokenPair data);
    }
}
