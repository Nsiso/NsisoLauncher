using System.Threading;
using System.Threading.Tasks;
using NsisoLauncherCore.Net;

namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken);

        Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken);
    }
}