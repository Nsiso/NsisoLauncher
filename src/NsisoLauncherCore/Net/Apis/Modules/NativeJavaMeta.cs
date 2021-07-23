using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Apis.Modules
{
    public class NativeJavaMeta
    {
        public JavaManifest Manifest { get; set; }

        public string OsInfo { get; set; }

        public string Tag { get; set; }

        public string Version { get; set; }

        public string GetJavaDestinationPath(string runtime_path)
        {
            return Path.Combine(GetBasePath(runtime_path), Tag);
        }

        public string GetBasePath(string runtime_path)
        {
            return Path.Combine(runtime_path, Tag, OsInfo);
        }
    }
}
