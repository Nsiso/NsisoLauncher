using System;
using System.Collections.Generic;
using NsisoLauncherCore.Net.Tools;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class MirrorInventory
    {
        public MirrorInventory()
        {
            DownloadableMirrorList = new List<IDownloadableMirror>();
            FunctionalMirrorList = new List<IFunctionalMirror>();
            VersionListMirrorList = new List<IVersionListMirror>();
        }

        public List<IDownloadableMirror> DownloadableMirrorList { get; set; }

        public List<IFunctionalMirror> FunctionalMirrorList { get; set; }

        public List<IVersionListMirror> VersionListMirrorList { get; set; }

        // !WARNING!: THIS IS A HARD CODE.
        public BmclApiBaseMirror GetBmclApi()
        {
            var BMCLUrl = "https://bmclapi2.bangbang93.com/";
            var BMCLLibrariesURL = BMCLUrl + "libraries/";
            var BMCLVersionURL = BMCLUrl + "mc/game/version_manifest.json";
            var BMCLAssetsURL = BMCLUrl + "objects/";

            var bmclapi = new BmclApiBaseMirror();
            bmclapi.MirrorName = "BmclAPI";
            bmclapi.BaseUri = new Uri(BMCLUrl);
            bmclapi.ReplaceDictionary = new Dictionary<string, string>
            {
                {GetDownloadUri.MojangVersionUrl, BMCLVersionURL},
                {GetDownloadUri.MojangMainUrl, BMCLUrl},
                {GetDownloadUri.MojangMetaUrl, BMCLUrl},
                {GetDownloadUri.MojanglibrariesUrl, BMCLLibrariesURL},
                {GetDownloadUri.MojangAssetsBaseUrl, BMCLAssetsURL},
                {GetDownloadUri.ForgeHttpUrl, BMCLLibrariesURL}
            };
            bmclapi.VersionListUri = new Uri(BMCLVersionURL);
            bmclapi.ForgeListUri = new Uri(bmclapi.BaseUri, "/forge/minecraft/");
            return bmclapi;
        }

        public BmclApiBaseMirror GetMcbbsApi()
        {
            var MCBBSUrl = "https://download.mcbbs.net/";
            var MCBBSLibrariesURL = MCBBSUrl + "libraries/";
            var MCBBSVersionURL = MCBBSUrl + "mc/game/version_manifest.json";
            var MCBBSAssetsURL = MCBBSUrl + "objects/";

            var mcbbsapi = new BmclApiBaseMirror();
            mcbbsapi.MirrorName = "MCBBS-BmclAPI";
            mcbbsapi.BaseUri = new Uri(MCBBSUrl);
            mcbbsapi.ReplaceDictionary = new Dictionary<string, string>
            {
                {GetDownloadUri.MojangVersionUrl, MCBBSVersionURL},
                {GetDownloadUri.MojangMainUrl, MCBBSUrl},
                {GetDownloadUri.MojangMetaUrl, MCBBSUrl},
                {GetDownloadUri.MojanglibrariesUrl, MCBBSLibrariesURL},
                {GetDownloadUri.MojangAssetsBaseUrl, MCBBSAssetsURL},
                {GetDownloadUri.ForgeHttpUrl, MCBBSLibrariesURL}
            };
            mcbbsapi.VersionListUri = new Uri(MCBBSVersionURL);
            mcbbsapi.ForgeListUri = new Uri(mcbbsapi.BaseUri, "/forge/minecraft/");
            return mcbbsapi;
        }
    }
}