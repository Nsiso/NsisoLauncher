using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NsisoLauncher.Core.Modules;

namespace NsisoLauncher.Core.Util
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

        public Assets GetAssets(Modules.Version version)
        {
            try
            {
                lock (locker)
                {
                    string assetsID = null;
                    if (string.IsNullOrWhiteSpace(version.AssetIndex.ID))
                    {
                        assetsID = version.AssetIndex.ID;
                    }
                    else
                    {
                        assetsID = version.Assets;
                    }
                    
                    string assetsPath = handler.GetAssetsIndexPath(assetsID);
                    if (!File.Exists(assetsPath))
                    {
                        return new Assets() { ID = version.Assets, IndexURL = version.AssetIndex.URL, TotalSize = version.AssetIndex.TotalSize, Infos = null };
                    }
                    var ja = GetAssetsByJson(File.ReadAllText(assetsPath));
                    if (ja == null)
                    {
                        return new Assets() { ID = assetsID, IndexURL = version.AssetIndex.URL, TotalSize = version.AssetIndex.TotalSize, Infos = null };
                    }
                    return new Assets()
                    {
                        Infos = ja,
                        ID = assetsID,
                        IndexURL = version.AssetIndex.URL,
                        TotalSize = version.AssetIndex.TotalSize
                    };
                }
            }
            catch (Exception)
            {
                return new Assets() { ID = version.Assets, IndexURL = null, TotalSize = -1, Infos = null };
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
