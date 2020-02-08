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

        public string VersionListURL { get; set; } = GetDownloadUrl.MojangVersionUrl;
        public string JavaListURL { get; set; } = GetDownloadUrl.BMCLUrl + "java/list";
        public string ForgeListURL { get; set; } = GetDownloadUrl.BMCLUrl + "forge/minecraft";
        public string NewListURL { get; set; } = "https://authentication.x-speed.cc/mcbbsNews/";

        public FunctionAPIHandler(DownloadSource lib)
        {
            Source = lib;
            switch (Source)
            {
                case DownloadSource.Mojang:
                    VersionListURL = GetDownloadUrl.MojangVersionUrl;
                    break;

                case DownloadSource.BMCLAPI:
                    VersionListURL = GetDownloadUrl.BMCLVersionURL;
                    JavaListURL = GetDownloadUrl.BMCLUrl + "java/list";
                    ForgeListURL = GetDownloadUrl.BMCLUrl + "forge/minecraft";
                    break;

                case DownloadSource.MCBBS:
                    VersionListURL = GetDownloadUrl.MCBBSVersionURL;
                    JavaListURL = GetDownloadUrl.MCBBSUrl + "java/list";
                    ForgeListURL = GetDownloadUrl.MCBBSUrl + "forge/minecraft";
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
            string json = await NetRequester.HttpGetStringAsync(VersionListURL);
            var e = JsonConvert.DeserializeObject<JWVersions>(json);
            return e.Versions;
        }

        /// <summary>
        /// 联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public async Task<List<JWJava>> GetJavaList()
        {
            string json = await NetRequester.HttpGetStringAsync(JavaListURL);
            var e = JsonConvert.DeserializeObject<List<JWJava>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取新闻列表
        /// </summary>
        /// <returns>新闻列表</returns>
        public async Task<List<JWNews>> GetNewList()
        {
            string json = await NetRequester.HttpGetStringAsync(NewListURL);
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
            string json = await NetRequester.HttpGetStringAsync(string.Format("{0}/{1}", ForgeListURL, version.ID));
            var e = JsonConvert.DeserializeObject<List<JWForge>>(json);
            return e;
        }
    }
}
