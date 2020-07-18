using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncherCore.Util
{
    public class AssetsReader
    {
        private readonly LaunchHandler handler;
        private readonly object locker = new object();

        public AssetsReader(LaunchHandler handler)
        {
            this.handler = handler;
        }

        public JAssets GetAssetsByJson(string json)
        {
            return JsonConvert.DeserializeObject<JAssets>(json);
        }

        public JAssets GetAssets(Version version)
        {
            try
            {
                lock (locker)
                {
                    var assetsPath = handler.GetAssetsIndexPath(version.Assets);
                    if (!File.Exists(assetsPath)) return null;
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
        [JsonProperty("objects")] public Dictionary<string, JAssetsInfo> Objects { get; set; }
    }

    public class JAssetsInfo : IDownloadable
    {
        [JsonProperty("hash")] public string Hash { get; set; }

        [JsonProperty("size")] public int Size { get; set; }

        public string GetDownloadSourceURL()
        {
            return GetDownloadUrl.GetAssetsDownloadURL(this);
        }
    }
}