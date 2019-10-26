using Newtonsoft.Json;
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

        public JAssets GetAssets(Modules.Version version)
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
        public Dictionary<string, JAssetsInfo> Objects { get; set; }
    }

    public class JAssetsInfo
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }
    }
}
