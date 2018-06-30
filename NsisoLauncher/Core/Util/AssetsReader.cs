using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public Assets GetAssets(Modules.Version version)
        {
            try
            {
                lock (locker)
                {
                    string assetsID = version.Assets;
                    var ja = JsonConvert.DeserializeObject<JAssets>(File.ReadAllText(handler.GetAssetsIndexPath(assetsID)));
                    if (ja == null)
                    {
                        return new Assets() { ID = assetsID, IndexURL = version.AssetIndex.URL, TotalSize = version.AssetIndex.TotalSize, Infos = null };
                    }
                    Assets ass = new Assets();
                    ass.Infos = new List<AssetsInfo>();
                    ass.ID = assetsID;
                    foreach (var item in ja.Objects)
                    {
                        ass.Infos.Add(new AssetsInfo() { Hash = item.Value.Hash, Size = item.Value.Size });
                    }
                    return ass;
                }
            }
            catch (Exception)
            {
                return new Assets() { ID = version.Assets, IndexURL = version.AssetIndex.URL, TotalSize = version.AssetIndex.TotalSize, Infos = null };
            }
        }
    }

    public class JAssets
    {
        [JsonProperty("objects")]
        public Dictionary<string, JInfo> Objects { get; set; }
    }

    public class JInfo
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }
    }
}
