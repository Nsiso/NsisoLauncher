using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using System.Threading;

namespace NsisoLauncherCore.Net.Yggdrasil
{
    public interface IAuthenticator
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, CancellationToken cancellation = default);

        Task<TokenResponse> Refresh(RefreshRequest request, CancellationToken cancellation = default);

        Task<Response> Validate(AccessClientTokenPair data, CancellationToken cancellation = default);

        Task<Response> Signout(UsernamePasswordPair data, CancellationToken cancellation = default);

        Task<Response> Invalidate(AccessClientTokenPair data, CancellationToken cancellation = default);
    }
}
