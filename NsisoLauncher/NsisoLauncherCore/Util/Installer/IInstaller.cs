using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Util.Installer
{
    public interface IInstaller
    {
        string InstallerPath { get; set; }

        IInstallOptions Options { get; set; }

        void BeginInstall();
    }
}
