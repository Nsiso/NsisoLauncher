using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Net.Apis.Modules
{
    public class JavaManifest
    {
        public Dictionary<string, JavaManifestObject> Files { get; set; }
    }

    public class JavaManifestObject
    {
        public string Type { get; set; }

        public bool? Executable { get; set; }

        public JavaManifestObjectDownloads Downloads { get; set; }
    }

    public class JavaManifestObjectDownloads
    {
        public Sha1SizeUrl Lzma { get; set; }

        public Sha1SizeUrl Raw { get; set; }
    }
}
