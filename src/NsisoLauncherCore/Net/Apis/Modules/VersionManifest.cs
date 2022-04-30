using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Apis.Modules
{
    public class VersionManifestV2
    {
        [JsonProperty("latest")]
        public VersionLatest Latest { get; set; }

        [JsonProperty("versions")]
        public List<VersionMetaV2> Versions { get; set; }
    }

    public class VersionManifest
    {
        [JsonProperty("latest")]
        public VersionLatest Latest { get; set; }

        [JsonProperty("versions")]
        public List<VersionMeta> Versions { get; set; }
    }

    public class VersionMetaV2 : VersionMeta
    {
        /// <summary>
        /// 版本ID
        /// </summary>
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        /// <summary>
        /// 版本类型
        /// </summary>
        [JsonProperty("complianceLevel")]
        public int ComplianceLevel { get; set; }
    }

    public class VersionMeta
    {
        /// <summary>
        /// 版本ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// 版本类型
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// 版本修改时间
        /// </summary>
        [JsonProperty("time")]
        public string Time { get; set; }

        /// <summary>
        /// 版本发布时间
        /// </summary>
        [JsonProperty("releaseTime")]
        public string ReleaseTime { get; set; }

        /// <summary>
        /// 版本下载URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class VersionLatest
    {
        [JsonProperty("release")]
        public string Release { get; set; }

        [JsonProperty("snapshot")]
        public string Snapshot { get; set; }
    }
}
