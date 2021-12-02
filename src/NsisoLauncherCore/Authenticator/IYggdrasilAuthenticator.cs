using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Authenticator
{
    public interface IYggdrasilAuthenticator
    {
        Task<YggdrasilAuthenticateUserResult> AuthenticateAsync(string username, string password, CancellationToken cancellation = default);

        Task<YggdrasilAuthenticateUserResult> RefreshAsync(YggdrasilUser user, CancellationToken cancellation = default);

        Task<YggdrasilAuthenticateResult> ValidateAsync(YggdrasilUser user, CancellationToken cancellation = default);

        Task<YggdrasilAuthenticateResult> SignoutAsync(string username, string password, CancellationToken cancellation = default);

        Task<YggdrasilAuthenticateResult> InvalidateAsync(YggdrasilUser user, CancellationToken cancellation = default);
    }
}
