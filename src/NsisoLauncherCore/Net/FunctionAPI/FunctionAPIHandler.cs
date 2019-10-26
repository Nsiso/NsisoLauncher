using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        public DownloadSource Source { get; private set; }

        const string BMCLBase = "https://bmclapi2.bangbang93.com";

        public string VersionListURL { get; set; }
        public string JavaListURL { get; set; } = BMCLBase + "/java/list";
        public string NewListURL { get; set; } = "https://authentication.x-speed.cc/mcbbsNews/";
        public string ForgeListURL { get; set; } = BMCLBase + "/forge/minecraft";
        public string LiteloaderListURL { get; set; } = BMCLBase + "/liteloader/list";

        public FunctionAPIHandler(DownloadSource lib)
        {
            Source = lib;
            switch (Source)
            {
                case DownloadSource.Mojang:
                    VersionListURL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
                    break;

                case DownloadSource.BMCLAPI:
                    VersionListURL = BMCLBase + "/mc/game/version_manifest.json";
                    break;
            }
        }

        public string DoURLReplace(string url)
        {
            return GetDownloadUrl.DoURLReplace(Source, url);
        }

        /// <summary>
        /// 联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public async Task<List<JWVersion>> GetVersionList()
        {
            string json = await APIRequester.HttpGetStringAsync(VersionListURL);
            var e = JsonConvert.DeserializeObject<JWVersions>(json);
            return e.Versions;
        }

        /// <summary>
        /// 联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public async Task<List<JWJava>> GetJavaList()
        {
            string json = await APIRequester.HttpGetStringAsync(JavaListURL);
            var e = JsonConvert.DeserializeObject<List<JWJava>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取新闻列表
        /// </summary>
        /// <returns>新闻列表</returns>
        public async Task<List<JWNews>> GetNewList()
        {
            string json = await APIRequester.HttpGetStringAsync(NewListURL);
            var e = JsonConvert.DeserializeObject<List<JWNews>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取指定版本所有的FORGE
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Forge列表</returns>
        public async Task<List<JWForge>> GetForgeList(Version version)
        {
            string json = await APIRequester.HttpGetStringAsync(string.Format("{0}/{1}", ForgeListURL, version.ID));
            var e = JsonConvert.DeserializeObject<List<JWForge>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取指定版本所有的Liteloader
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Liteloader列表</returns>
        public async Task<JWLiteloader> GetLiteloaderList(Version version)
        {
            string json = await APIRequester.HttpGetStringAsync(string.Format("{0}/?mcversion={1}", LiteloaderListURL, version.ID));
            var e = JsonConvert.DeserializeObject<JWLiteloader>(json);
            return e;
        }
    }
}
