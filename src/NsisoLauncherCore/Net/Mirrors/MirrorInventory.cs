using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class MirrorInventory
    {
        private const string BMCLUrl = "https://bmclapi2.bangbang93.com/";
        private const string BMCLVersionURL = $"{BMCLUrl}mc/game/version_manifest.json";

        private const string MCBBSUrl = "https://download.mcbbs.net/";
        private const string MCBBSVersionURL = $"{MCBBSUrl}mc/game/version_manifest.json";

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
            ApiBaseMirror offical = new ApiBaseMirror(); 
            offical.MirrorName = OfficalAPI;

            offical.MCDownloadUri = new Uri(GetDownloadUri.MojangMainUrl);
            offical.ReplaceDictionary = GetBmclApiBaseReplaceUriDic("launcher.mojang.com");
            offical.VersionListUri = new Uri(GetDownloadUri.MojangVersionUrl);
            offical.ForgeListUri = new Uri(bmclapi.BaseUri, "/forge/minecraft");
            return offical;
        }

        public ApiBaseMirror GetBmclApi()
        {
            ApiBaseMirror bmclapi = new ApiBaseMirror();
            bmclapi.MirrorName = BmclAPI;

            bmclapi.MCDownloadUri = new Uri(BMCLUrl);
            bmclapi.CoreVersionListUri = new Uri($"{BMCLUrl}version/");
            bmclapi.ForgeDownloadUri = new Uri($"{BMCLUrl}forge/download/");
            bmclapi.ReplaceDictionary = GetBmclApiBaseReplaceUriDic("bmclapi2.bangbang93.com");
            bmclapi.VersionListUri = new Uri(BMCLVersionURL);
            bmclapi.ForgeListUri = new Uri($"{BMCLUrl}/forge/minecraft");
            return bmclapi;
        }

        public ApiBaseMirror GetMcbbsApi()
        {
            ApiBaseMirror mcbbsapi = new ApiBaseMirror();
            mcbbsapi.MirrorName = McbbsAPI;

            mcbbsapi.MCDownloadUri = new Uri(MCBBSUrl);
            mcbbsapi.CoreVersionListUri = new Uri($"{MCBBSUrl}version/");
            mcbbsapi.ForgeDownloadUri = new Uri($"{MCBBSUrl}forge/download/");
            mcbbsapi.ReplaceDictionary = GetBmclApiBaseReplaceUriDic("download.mcbbs.net");
            mcbbsapi.VersionListUri = new Uri(MCBBSVersionURL);
            mcbbsapi.ForgeListUri = new Uri($"{MCBBSUrl}/forge/minecraft");
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