using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IDownloadableMirror : IMirror
    {
        Dictionary<string, string> ReplaceDictionary { get; set; }

        string DoDownloadUriReplace(string source);
    }
}
