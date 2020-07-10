using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        public DownloadSource Source { get; private set; }

        public string VersionListURL { get; set; } = GetDownloadUrl.MojangVersionUrl;
        public string JavaListURL { get; set; } = BmclMirror.BMCLUrl + "java/list";
        public string ForgeListURL { get; set; } = BmclMirror.BMCLUrl + "forge/minecraft";
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
                    VersionListURL = BmclMirror.BMCLVersionURL;
                    JavaListURL = BmclMirror.BMCLUrl + "java/list";
                    ForgeListURL = BmclMirror.BMCLUrl + "forge/minecraft";
                    break;

                case DownloadSource.MCBBS:
                    VersionListURL = McbbsMirror.MCBBSVersionURL;
                    JavaListURL = McbbsMirror.MCBBSUrl + "java/list";
                    ForgeListURL = McbbsMirror.MCBBSUrl + "forge/minecraft";
                    break;
            }
        }

        /// <summary>
        /// 联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public async Task<List<JWVersion>> GetVersionList()
        {
            HttpResponseMessage jsonRespond = await NetRequester.Client.GetAsync(VersionListURL);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (!string.IsNullOrEmpty(json))
            {
                var e = JsonConvert.DeserializeObject<JWVersions>(json);
                return e.Versions;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public async Task<List<JWJava>> GetJavaList()
        {
            HttpResponseMessage jsonRespond = await NetRequester.Client.GetAsync(JavaListURL);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var e = JsonConvert.DeserializeObject<List<JWJava>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取新闻列表
        /// </summary>
        /// <returns>新闻列表</returns>
        public async Task<List<JWNews>> GetNewList()
        {
            HttpResponseMessage jsonRespond = await NetRequester.Client.GetAsync(NewListURL);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
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
            HttpResponseMessage jsonRespond = await NetRequester.Client.GetAsync(string.Format("{0}/{1}", ForgeListURL, version.ID));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var e = JsonConvert.DeserializeObject<List<JWForge>>(json);
            return e;
        }
    }
}
