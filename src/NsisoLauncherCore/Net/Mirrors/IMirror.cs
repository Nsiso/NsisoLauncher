using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IMirror
    {
        Dictionary<string, string> ReplaceDictionary { get; set; }

        string DoDownloadUrlReplace(string source);
    }
}
