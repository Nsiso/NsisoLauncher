using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.PhalAPI
{
    /**
     * 接口返回结果
     *
     * - 与接口返回的格式对应，即有：ret/data/msg
     */
    public class PhalApiClientResponse
    {
        [JsonProperty("ret")]
        public int Ret { get; set; }

        [JsonProperty("data")]
        public dynamic Data { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public class NsisoLauncherVersionListResponse
    {
        [JsonProperty("list")]
        public List<NsisoLauncherVersionResponse> List { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("err_msg")]
        public string ErrorMessage { get; set; }

        [JsonProperty("err_code")]
        public int ErrorCode { get; set; }
    }

    public class NsisoLauncherVersionResponse
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty("version")]
        public Version Version { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [JsonProperty("release_time")]
        public DateTime ReleaseTime { get; set; }

        /// <summary>
        /// 发布类型
        /// </summary>
        [JsonProperty("release_type")]
        public string ReleaseType { get; set; }

        /// <summary>
        /// 版本描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 更新信息
        /// </summary>
        [JsonProperty("update_information")]
        public string UpdateInformation { get; set; }

        /// <summary>
        /// 直链下载源1
        /// </summary>
        [JsonProperty("download_source_1")]
        public string DownloadSource_1 { get; set; }

        /// <summary>
        /// 直链下载源2
        /// </summary>
        [JsonProperty("download_source_2")]
        public string DownloadSource_2 { get; set; }

        /// <summary>
        /// 手动下载源
        /// </summary>
        [JsonProperty("download_source_manual")]
        public string DownloadSource_manual { get; set; }

        /// <summary>
        /// 检验用MD5
        /// </summary>
        [JsonProperty("md5")]
        public string MD5 { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

}
