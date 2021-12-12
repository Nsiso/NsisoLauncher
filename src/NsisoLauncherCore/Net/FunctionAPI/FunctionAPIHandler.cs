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
using System.Xml;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public static class FunctionAPIHandler
    {
        /// <summary>
        /// 联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public static async Task<VersionManifest> GetVersionManifest(IVersionListMirror mirror)
        {
            Uri versionListUri;
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
        public static async Task<List<JWJava>> GetJavaList(IFunctionalMirror mirror)
        {
            Uri javaListUri;
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
        public static async Task<List<JWForge>> GetForgeList(IFunctionalMirror mirror, VersionBase version)
        {
            Uri forgeListUri;
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
                return list;
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
        /// 联网获取指定版本所有的FORGE
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Forge列表</returns>
        public static string GetForgeDownload(IFunctionalMirror mirror, VersionBase version, JWForge forge)
        {
            Uri forgeListUri;
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                forgeListUri = mirror.ForgeListUri;
            }

            if (forgeListUri.AbsolutePath == "https://maven.minecraftforge.net/net/minecraftforge/forge/")
            {
                return $"{forgeListUri.AbsolutePath}{version.Id}-{forge.Version}/forge-{version.Id}-{forge.Version}-installer.jar";
            }

            else
            {
                return $"{mirror.ForgeDownloadUri}{forge.Build}";
            }
        }

        /// <summary>
        /// 联网获取Fabric安装器
        /// </summary>
        /// <returns>Fabric链接</returns>
        public static async Task<string> GetFabricDownload(IFunctionalMirror mirror)
        {
            Uri fabricListUri;
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                fabricListUri = new Uri(mirror.FabricDownloadUri, "maven-metadata.xml");
            }

            var res = await NetRequester.HttpGetStringAsync(fabricListUri);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(res);

            foreach (var item in xmlDoc.GetElementsByTagName("latest"))
            {
                string version = (item as XmlElement)?.InnerText;
                if (string.IsNullOrWhiteSpace(version))
                {
                    throw new Exception("Get Fabric Version Error");
                }
                return $"{mirror.FabricDownloadUri}/{version}/fabric-installer-{version}.jar";
            }

            throw new Exception("Get Fabric Version Error");

        }

        /// <summary>
        /// 联网获取指定版本所有的FABRIC
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Fabric列表</returns>
        public static async Task<List<JWFabric>> GetFabricList(IFunctionalMirror mirror, VersionBase version)
        {
            Uri fabricListUri;
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                fabricListUri = mirror.FabricListUri;
            }

            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(fabricListUri);
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
