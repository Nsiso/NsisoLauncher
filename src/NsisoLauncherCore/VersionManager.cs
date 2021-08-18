using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore
{
    public class VersionManager
    {
        public ObservableCollection<VersionBase> VersionObservableList { get; set; }

        private VersionReader reader;
        private FileSystemWatcher fileSystemWatcher;

        public VersionManager(LaunchHandler handler)
        {
        }
    }
}
