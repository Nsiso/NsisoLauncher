using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class BmclMirror : IMirror
    {
        public const string BMCLUrl = "https://bmclapi2.bangbang93.com/";
        public const string BMCLLibrariesURL = BMCLUrl + "libraries/";
        public const string BMCLVersionURL = BMCLUrl + "mc/game/version_manifest.json";
        public const string BMCLAssetsURL = BMCLUrl + "objects/";

        public string MirrorName { get; set; } = "BmclAPI";
        public string BaseDomain { get; set; } = "bmclapi2.bangbang93.com";

        public Dictionary<string, string> ReplaceDictionary { get; set; } = new Dictionary<string, string>()
        {
            {GetDownloadUrl.MojangVersionUrl, BMCLVersionURL },
            {GetDownloadUrl.MojangMainUrl, BMCLUrl },
            {GetDownloadUrl.MojangMetaUrl, BMCLUrl },
            {GetDownloadUrl.MojanglibrariesUrl, BMCLLibrariesURL },
            {GetDownloadUrl.MojangAssetsBaseUrl, BMCLAssetsURL },
            {GetDownloadUrl.ForgeHttpUrl, BMCLLibrariesURL },
            {GetDownloadUrl.ForgeHttpsUrl, BMCLLibrariesURL }
        };

        public string DoDownloadUrlReplace(string source)
        {
            return GetDownloadUrl.ReplaceURLByDic(source, ReplaceDictionary);
        }
    }
}
