using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IMirror
    {
        Dictionary<string, string> ReplaceDictionary { get; set; }

        string MirrorName { get; set; }

        string BaseDomain { get; set; }

        string DoDownloadUrlReplace(string source);
    }
}