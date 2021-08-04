using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace NsisoLauncherCore.Util
{
    public class AssetsReader
    {
        private object locker = new object();
        private LaunchHandler handler;

        public AssetsReader(LaunchHandler handler)
        {
            this.handler = handler;
        }

        public JAssets GetAssetsByJson(string json)
        {
            return JsonConvert.DeserializeObject<JAssets>(json);
        }

        public JAssets GetAssets(VersionBase version)
        {
            try
            {
                lock (locker)
                {
                    string assetsPath = handler.GetAssetsIndexPath(version.Assets);
                    if (!File.Exists(assetsPath))
                    {
                        return null;
                    }
                    var ja = GetAssetsByJson(File.ReadAllText(assetsPath));
                    return ja;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class JAssets
    {
        [JsonProperty("objects")]
        public Dictionary<string, JAssetInfo> Objects { get; set; }
    }

    public class JAssetInfo : IDownloadable
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        public char FirstHashChar { get => Hash[0]; }

        public string GetDownloadSourceURL()
        {
            return GetDownloadUri.GetAssetsDownloadURL(this);
        }
    }
}
