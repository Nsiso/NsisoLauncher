using NsisoLauncherCore.Net;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer.Fabric
{
    public class FabricInstaller : IInstaller
    {
        public string InstallerPath { get; set; }

        public FabricInstallOptions Options { get; set; }

        public FabricInstaller(string installerPath, FabricInstallOptions options)
        {
            this.InstallerPath = installerPath;
            this.Options = options;
        }
        public void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken)
        {
            string arg = $"-jar \"{InstallerPath}\" client -dir \"{Options.GameRootPath}\" -mcversion {Options.VersionToInstall.Id} -loader {Options.Fabric.Version}";
            ProcessStartInfo startInfo = new ProcessStartInfo(Options.Java.Path, arg);
            Process.Start(startInfo).WaitForExit();
        }

        public Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken)
        {
            return Task.Run(() => BeginInstall(callback, cancellationToken));
        }
    }
}
