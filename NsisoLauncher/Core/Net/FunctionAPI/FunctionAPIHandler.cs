using System.Collections.Generic;
using System.Text;
using static NsisoLauncher.Core.Net.FunctionAPI.APIModules;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using NsisoLauncher.Core.Net.Tools;

namespace NsisoLauncher.Core.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        public DownloadSource Source { get; private set; }
        const string BMCLBase = "https://bmclapi2.bangbang93.com";
        public string VersionListURL { get; set; } = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public string JavaListURL { get; set; } = BMCLBase + "/java/list";
        public string NewListURL { get; set; } = "https://authentication.x-speed.cc/mcbbsNews/";
        public string MojangStatusURL { get; set; } = "https://status.mojang.com/check";

        public FunctionAPIHandler(DownloadSource lib)
        {
            Source = lib;
            switch (Source)
            {
                case DownloadSource.Mojang:
                    VersionListURL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
                    break;

                case DownloadSource.BMCLAPI:
                    VersionListURL = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";
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
        public List<JWVersion> GetVersionList()
        {
            string json = HttpGet(VersionListURL);
            var e = JsonConvert.DeserializeObject<JWVersions>(json);
            return e.Versions;
        }

        /// <summary>
        /// 联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public List<JWJava> GetJavaList()
        {
            string json = HttpGet(JavaListURL);
            var e = JsonConvert.DeserializeObject<List<JWJava>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取新闻列表
        /// </summary>
        /// <returns>新闻列表</returns>
        public List<JWNews> GetNewList()
        {
            string json = HttpGet(NewListURL);
            var e = JsonConvert.DeserializeObject<List<JWNews>>(json);
            return e;
        }

        public static string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            return WebGet(request);
        }

        public static string WebGet(HttpWebRequest request)
        {
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
    }
}
