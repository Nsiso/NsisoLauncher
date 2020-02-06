using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using NsisoLauncherCore.Net;
using System.Threading;

namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken);

        Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken);
    }
}
