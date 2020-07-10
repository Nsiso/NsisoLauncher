using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class McbbsMirror : BmclMirror
    {
        public const string MCBBSUrl = "https://download.mcbbs.net/";
        public const string MCBBSLibrariesURL = MCBBSUrl + "libraries/";
        public const string MCBBSVersionURL = MCBBSUrl + "mc/game/version_manifest.json";
        public const string MCBBSAssetsURL = MCBBSUrl + "objects/";

        public new Dictionary<string, string> ReplaceDictionary { get; set; } = new Dictionary<string, string>()
        {
            {GetDownloadUrl.MojangVersionUrl, MCBBSVersionURL },
            {GetDownloadUrl.MojangMainUrl, MCBBSUrl },
            {GetDownloadUrl.MojangMetaUrl, MCBBSUrl },
            {GetDownloadUrl.MojanglibrariesUrl, MCBBSLibrariesURL },
            {GetDownloadUrl.MojangAssetsBaseUrl, MCBBSAssetsURL },
            {GetDownloadUrl.ForgeHttpUrl, MCBBSLibrariesURL },
            {GetDownloadUrl.ForgeHttpsUrl, MCBBSLibrariesURL }
        };
    }
}
