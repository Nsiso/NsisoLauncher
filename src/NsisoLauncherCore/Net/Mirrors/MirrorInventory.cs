using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Mirrors
{
    public class MirrorInventory
    {
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
        public BmclApiBaseMirror GetBmclApi()
        {
            string BMCLUrl = "https://bmclapi2.bangbang93.com/";
            string BMCLVersionURL = BMCLUrl + "mc/game/version_manifest.json";

            BmclApiBaseMirror bmclapi = new BmclApiBaseMirror();
            bmclapi.MirrorName = "BmclAPI";
            bmclapi.BaseUri = new Uri(BMCLUrl);
            bmclapi.ReplaceDictionary = GetBmclApiBaseReplaceUriDic("bmclapi2.bangbang93.com");
            bmclapi.VersionListUri = new Uri(BMCLVersionURL);
            bmclapi.ForgeListUri = new Uri(bmclapi.BaseUri, "/forge/minecraft/");
            return bmclapi;
        }

        public BmclApiBaseMirror GetMcbbsApi()
        {
            string MCBBSUrl = "https://download.mcbbs.net/";
            string MCBBSVersionURL = MCBBSUrl + "mc/game/version_manifest.json";

            BmclApiBaseMirror mcbbsapi = new BmclApiBaseMirror();
            mcbbsapi.MirrorName = "MCBBS-BmclAPI";
            mcbbsapi.BaseUri = new Uri(MCBBSUrl);
            mcbbsapi.ReplaceDictionary = GetBmclApiBaseReplaceUriDic("download.mcbbs.net");
            mcbbsapi.VersionListUri = new Uri(MCBBSVersionURL);
            mcbbsapi.ForgeListUri = new Uri(mcbbsapi.BaseUri, "/forge/minecraft/");
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
                {"meta.fabricmc.net", bmclBase + "/fabric-meta" },
                {"maven.fabricmc.net", maven }
            };
        }
    }
}