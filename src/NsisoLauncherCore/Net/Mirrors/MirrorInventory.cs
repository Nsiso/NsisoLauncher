using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class MirrorInventory
    {
        private const string BMCLUrl = "https://bmclapi2.bangbang93.com/";
        private const string BMCLVersionURL = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

        private const string MCBBSUrl = "https://download.mcbbs.net/";
        private const string MCBBSVersionURL = "https://download.mcbbs.net/mc/game/version_manifest.json";

        public const string BmclAPI = "BmclAPI";
        public const string OfficalAPI = "OfficalAPI";
        public const string McbbsAPI = "MCBBS-BmclAPI";

        public List<IDownloadableMirror> DownloadableMirrorList { get; set; }

        public List<IFunctionalMirror> FunctionalMirrorList { get; set; }

        public List<IVersionListMirror> VersionListMirrorList { get; set; }

        public MirrorInventory()
        {
            DownloadableMirrorList = new List<IDownloadableMirror>();
            FunctionalMirrorList = new List<IFunctionalMirror>();
            VersionListMirrorList = new List<IVersionListMirror>();
        }

        // !WARNING!: THIS IS A HARD CODE.
        public ApiBaseMirror GetOfficalApi()
        {
            ApiBaseMirror offical = new ApiBaseMirror
            {
                MirrorName = OfficalAPI,

                MCDownloadUri = new Uri(GetDownloadUri.MojangMainUrl),
                ReplaceDictionary = new Dictionary<string, string>(),
                VersionListUri = new Uri(GetDownloadUri.MojangVersionUrl),
                ForgeListUri = new Uri("https://files.minecraftforge.net/net/minecraftforge/forge/"),
                FabricListUri = new Uri("https://meta.fabricmc.net/v2/versions/loader"),
                ForgeDownloadUri = new Uri("https://maven.minecraftforge.net/net/minecraftforge/forge/"),
                FabricDownloadUri = new Uri("https://maven.fabricmc.net/net/fabricmc/fabric-installer/")
            };
            return offical;
        }

        public ApiBaseMirror GetBmclApi()
        {
            ApiBaseMirror bmclapi = new ApiBaseMirror
            {
                MirrorName = BmclAPI,

                MCDownloadUri = new Uri(BMCLUrl),
                CoreVersionListUri = new Uri($"{BMCLUrl}version/"),
                ForgeDownloadUri = new Uri($"{BMCLUrl}forge/download/"),
                ReplaceDictionary = GetBmclApiBaseReplaceUriDic("bmclapi2.bangbang93.com"),
                VersionListUri = new Uri(BMCLVersionURL),
                ForgeListUri = new Uri($"{BMCLUrl}/forge/minecraft"),
                FabricListUri = new Uri("https://meta.fabricmc.net/v2/versions/loader"),
                FabricDownloadUri = new Uri("https://maven.fabricmc.net/net/fabricmc/fabric-installer/")
            };
            return bmclapi;
        }

        public ApiBaseMirror GetMcbbsApi()
        {
            ApiBaseMirror mcbbsapi = new ApiBaseMirror
            {
                MirrorName = McbbsAPI,

                MCDownloadUri = new Uri(MCBBSUrl),
                CoreVersionListUri = new Uri($"{MCBBSUrl}version/"),
                ForgeDownloadUri = new Uri($"{MCBBSUrl}forge/download/"),
                ReplaceDictionary = GetBmclApiBaseReplaceUriDic("download.mcbbs.net"),
                VersionListUri = new Uri(MCBBSVersionURL),
                ForgeListUri = new Uri($"{MCBBSUrl}/forge/minecraft"),
                FabricListUri = new Uri("https://meta.fabricmc.net/v2/versions/loader"),
                FabricDownloadUri = new Uri("https://maven.fabricmc.net/net/fabricmc/fabric-installer/")
            };
            return mcbbsapi;
        }

        private Dictionary<string, string> GetBmclApiBaseReplaceUriDic(string bmclBase)
        {
            string maven = bmclBase + "/maven";
            return new Dictionary<string, string>()
            {
                {"launchermeta.mojang.com", bmclBase },
                {"launcher.mojang.com", bmclBase },
                {"resources.download.minecraft.net", bmclBase + "/assets" },
                {"libraries.minecraft.net", maven },
                {"files.minecraftforge.net/maven", maven },
                {"maven.minecraftforge.net", maven },
                {"meta.fabricmc.net", bmclBase + "/fabric-meta" },
                {"maven.fabricmc.net", maven }
            };
        }
    }
}