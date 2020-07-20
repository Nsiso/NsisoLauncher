using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IDownloadableMirror : IMirror
    {
        Dictionary<string, string> ReplaceDictionary { get; set; }

        string DoDownloadUriReplace(string source);
    }
}
