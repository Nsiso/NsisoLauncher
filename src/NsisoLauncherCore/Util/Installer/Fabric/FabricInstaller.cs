using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer.Fabric
{
    class FabricInstaller : IInstaller
    {
        public string InstallerPath { get; set; }

        public CommonInstallOptions Options { get; set; }

        public FabricInstaller(string installerPath, CommonInstallOptions options)
        {
            this.InstallerPath = installerPath;
            this.Options = options;
        }
        public void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken)
        {
            string arg = string.Format("-jar \"{0}\" client -dir \"{1}\" -mcversion {2}", InstallerPath, Options.GameRootPath, Options.VersionToInstall.ID);
            ProcessStartInfo startInfo = new ProcessStartInfo(Options.Java.Path, arg);
            Process.Start(startInfo).WaitForExit();
        }

        public Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken)
        {
            return Task.Run(() => BeginInstall(callback, cancellationToken));
        }
    }
}
