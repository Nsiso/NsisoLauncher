using HtmlAgilityPack;
using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        public IFunctionalMirror FunctionalMirror { get; set; }
        public IVersionListMirror VersionListMirror { get; set; }

        public FunctionAPIHandler(IVersionListMirror versionListMirror, IFunctionalMirror functionMirror)
        {
            FunctionalMirror = functionMirror ?? throw new ArgumentNullException("FunctionalMirror is null");
            VersionListMirror = versionListMirror;
        }

        /// <summary>
        /// 联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public async Task<VersionManifest> GetVersionManifest()
        {
            Uri versionListUri;
            IVersionListMirror mirror = VersionListMirror;
            versionListUri = mirror == null ? new Uri(GetDownloadUri.MojangVersionUrl) : mirror.VersionListUri;
            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(versionListUri);
            jsonRespond.EnsureSuccessStatusCode();
            string json = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VersionManifest>(json);
        }

        /// <summary>
        /// 联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public async Task<List<JWJava>> GetJavaList()
        {
            Uri javaListUri;
            IFunctionalMirror mirror = FunctionalMirror;
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                javaListUri = mirror.JavaListUri;
            }
            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(javaListUri);
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
        /// 联网获取指定版本所有的FORGE
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Forge列表</returns>
        public async Task<List<JWForge>> GetForgeList(VersionBase version)
        {
            Uri forgeListUri;
            IFunctionalMirror mirror = FunctionalMirror;
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                forgeListUri = mirror.ForgeListUri;
            }

            if (mirror.ForgeListUri.AbsolutePath == "https://files.minecraftforge.net/net/minecraftforge/forge/")
            {
                HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync($"{forgeListUri}/index_{version.Id}.html");
                string html = null;
                if (jsonRespond.IsSuccessStatusCode)
                {
                    html = await jsonRespond.Content.ReadAsStringAsync();
                }
                if (string.IsNullOrWhiteSpace(html))
                {
                    return null;
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var nodes = doc.DocumentNode.Descendants("table")
                    .Where(x => x.Attributes["class"]?.Value == "download-list").FirstOrDefault();
                if (nodes == null)
                    return null;
                var nodes1 = nodes.Descendants("tbody").FirstOrDefault();
                if (nodes1 == null)
                    return null;
                List<JWForge> list = new List<JWForge>();
                foreach (var item in nodes1.Descendants("tr"))
                {
                    var item1 = item.Descendants("td").Where(x => x.Attributes["class"]?.Value == "download-version").FirstOrDefault();
                    if (item1 != null)
                    {
                        list.Add(new JWForge
                        {
                            Version = item1.InnerText.Trim()
                        });
                    }
                }
            }

            else
            {
                HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync($"{forgeListUri}/{version.Id}");
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

        /// <summary>
        /// 联网获取指定版本所有的FABRIC
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Fabric列表</returns>
        public async Task<List<JWFabric>> GetFabricList(VersionBase version)
        {
            Uri fabricListUri;
            IFunctionalMirror mirror = FunctionalMirror;
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                fabricListUri = mirror.FabricListUri;
            }
            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(string.Format("{0}/{1}", fabricListUri, version.Id));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var e = JsonConvert.DeserializeObject<List<JWFabric>>(json);
            return e;
        }
    }
}
