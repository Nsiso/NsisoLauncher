using NsisoLauncherCore.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken);

        Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken);
    }
}
