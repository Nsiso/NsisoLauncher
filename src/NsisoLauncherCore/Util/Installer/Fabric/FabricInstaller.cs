using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NsisoLauncherCore.Net;

namespace NsisoLauncherCore.Util.Installer.Fabric
{
    internal class FabricInstaller : IInstaller
    {
        public FabricInstaller(string installerPath, CommonInstallOptions options)
        {
            InstallerPath = installerPath;
            Options = options;
        }

        public string InstallerPath { get; set; }

        public CommonInstallOptions Options { get; set; }

        public void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken)
        {
            var arg = string.Format("-jar \"{0}\" client -dir \"{1}\" -mcversion {2}", InstallerPath,
                Options.GameRootPath, Options.VersionToInstall.ID);
            var startInfo = new ProcessStartInfo(Options.Java.Path, arg);
            Process.Start(startInfo).WaitForExit();
        }

        public Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken)
        {
            return Task.Run(() => BeginInstall(callback, cancellationToken));
        }
    }
}