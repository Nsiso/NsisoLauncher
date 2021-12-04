using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class ApiBaseMirror : IDownloadableMirror, IFunctionalMirror, IVersionListMirror, IMirror
    {
        public Dictionary<string, string> ReplaceDictionary { get; set; }
        public string MirrorName { get; set; }

        public Uri BaseUri { get; set; }

        public Uri VersionListUri { get; set; }
        public Uri CoreVersionListUri { get; set; }

        public Uri JavaListUri { get; set; }
        public Uri ForgeListUri { get; set; }
        public Uri LiteloaderListUri { get; set; }
        public Uri OptifineListUri { get; set; }
        public Uri FabricListUri { get; set; }

        public Uri MCDownloadUri { get; set; }
        public Uri ForgeDownloadUri { get; set; }
        public Uri FabricDownloadUri { get; set; }

        public string DoDownloadUriReplace(string source)
        {
            return GetDownloadUri.ReplaceUriByDic(source, ReplaceDictionary);
        }
    }
}
