using NsisoLauncherCore.Net;
/* 项目“NsisoLauncher.Test”的未合并的更改
在此之前:
using System;
在此之后:
using NsisoLauncherCore.Net;
*/
using System.Threading;
using System.Threading.Tasks;
/* 项目“NsisoLauncher.Test”的未合并的更改
在此之前:
using NsisoLauncherCore.Net;
using System.Threading;
在此之后:
using System.Threading;
using System.Threading.Tasks;
*/


namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken);

        Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken);
    }
}
