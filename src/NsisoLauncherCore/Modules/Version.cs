using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Modules
{
    /// <summary>
    /// the minecraft version info class
    /// </summary>
    public class Version
    {
        /// <summary>
        /// 资源引导
        /// </summary>
        [JsonProperty("assetIndex")]
        public AssetIndex AssetIndex { get; set; }

        /// <summary>
        /// 资源ID
        /// </summary>
        [JsonProperty("assets")]
        public string Assets { get; set; }

        /// <summary>
        /// 下载引导
        /// </summary>
        [JsonProperty("downloads")]
        public CoreDownloads Downloads { get; set; }

        /// <summary>
        /// 版本ID
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// 继承版本
        /// </summary>
        [JsonProperty("inheritsFrom")]
        public string InheritsVersion { get; set; }

        ///// <summary>
        ///// 库列表
        ///// </summary>
        //[JsonProperty("libraries")]
        //public List<Library> Library { get; set; }

        /// <summary>
        /// 库列表
        /// </summary>
        [JsonIgnore]
        public List<Library> Libraries { get; set; }

        /// <summary>
        /// native列表
        /// </summary>
        [JsonIgnore]
        public List<Native> Natives { get; set; }

        /// <summary>
        /// 启动主类
        /// </summary>
        [JsonProperty("mainClass")]
        public string MainClass { get; set; }

        /// <summary>
        /// 指定JAR包
        /// </summary>
        [JsonProperty("jar")]
        public string Jar { get; set; }

        /// <summary>
        /// JVM启动参数
        /// </summary>
        [JsonIgnore]
        public string JvmArguments { get; set; }

        /// <summary>
        /// Minecraft启动参数
        /// </summary>
        [JsonProperty("minecraftArguments")]
        public string MinecraftArguments { get; set; }

        /// <summary>
        /// 启动器最低能启动版本号
        /// </summary>
        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [JsonProperty("releaseTime")]
        public DateTime ReleaseTime { get; set; }

        /// <summary>
        /// 版本类型
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    #region assetsIndex
    public class AssetIndex : Sha1SizeUrl
    {
        /// <summary>
        /// 资源ID
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// 总大小
        /// </summary>
        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }
    }
    #endregion

    #region coreDownloads
    public class CoreDownloads
    {
        /// <summary>
        /// 客户端下载信息
        /// </summary>
        [JsonProperty("client")]
        public Sha1SizeUrl Client { get; set; }

        /// <summary>
        /// 服务端下载信息
        /// </summary>
        [JsonProperty("server")]
        public Sha1SizeUrl Server { get; set; }
    }
    #endregion

    #region Library
    //public class Library
    //{
    //    /// <summary>
    //    /// 库名称
    //    /// </summary>
    //    [JsonProperty("name")]
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// Native列表
    //    /// </summary>
    //    [JsonProperty("natives")]
    //    public Dictionary<string, string> Natives { get; set; }

    //    /// <summary>
    //    /// 规则
    //    /// </summary>
    //    [JsonProperty("rules")]
    //    public List<Rule> Rules { get; set; }

    //    /// <summary>
    //    /// 解压声明
    //    /// </summary>
    //    [JsonProperty("extract")]
    //    public Extract Extract { get; set; }
    //}

    public class Library
    {
        /// <summary>
        /// 包名
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 下载URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 库文件下载信息
        /// </summary>
        public PathSha1SizeUrl LibDownloadInfo { get; set; }
    }

    public class Native : Library
    {
        /// <summary>
        /// windows系统修改后缀
        /// </summary>
        public string NativeSuffix { get; set; }

        /// <summary>
        /// 不解压的文件夹
        /// </summary>
        public List<string> Exclude { get; set; }

        /// <summary>
        /// native文件下载信息
        /// </summary>
        public PathSha1SizeUrl NativeDownloadInfo { get; set; }
    }


    //public class Rule
    //{
    //    /// <summary>
    //    /// action
    //    /// </summary>
    //    [JsonProperty("action")]
    //    public string Action { get; set; }

    //    /// <summary>
    //    /// 操作系统
    //    /// </summary>
    //    [JsonProperty("os")]
    //    public OperatingSystem OS { get; set; }
    //}

    //public class OperatingSystem
    //{
    //    /// <summary>
    //    /// 系统名称
    //    /// </summary>
    //    [JsonProperty("name")]
    //    public string Name { get; set; }
    //}

    //public class Extract
    //{
    //    /// <summary>
    //    /// 排除列表
    //    /// </summary>
    //    [JsonProperty("exclude")]
    //    public List<string> Exculde { get; set; }
    //}
    #endregion

    /// <summary>
    /// 基本要素类
    /// </summary>
    public class Sha1SizeUrl
    {
        /// <summary>
        /// SHA1
        /// </summary>
        [JsonProperty("sha1")]
        public string SHA1 { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// 下载URL
        /// </summary>
        [JsonProperty("url")]
        public string URL { get; set; }
    }

    /// <summary>
    /// 基本要素类
    /// </summary>
    public class PathSha1SizeUrl : Sha1SizeUrl
    {
        /// <summary>
        /// 路径
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
