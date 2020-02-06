using Newtonsoft.Json;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using static NsisoLauncherCore.Util.JsonTools;

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
    public class Library
    {
        public Artifact Artifact { get; set; }

        /// <summary>
        /// 下载URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 库文件下载信息
        /// </summary>
        public PathSha1SizeUrl LibDownloadInfo { get; set; }

        public Library(string descriptor)
        {
            this.Artifact = new Artifact(descriptor);
        }
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

        public Native(string descriptor, string nativeSuffix) : base(descriptor)
        {
            this.NativeSuffix = nativeSuffix;
        }
    }

    [JsonConverter(typeof(ArtifactJsonConverter))]
    public class Artifact
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
        /// Classifier分类
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// 扩展名
        /// </summary>
        public string Extension { get; set; } = "jar";

        public string Descriptor { get; set; }

        public Artifact(string descriptor)
        {
            this.Descriptor = descriptor;
            string[] parts = descriptor.Split(':');

            this.Package = parts[0];
            this.Name = parts[1];

            int last = parts.Length - 1;
            int idx = parts[last].IndexOf('@');
            if (idx != -1)
            {
                this.Extension = parts[last].Substring(idx + 1);
                parts[last] = parts[last].Substring(0, idx);
            }

            this.Version = parts[2];

            if (parts.Length > 3)
            {
                this.Classifier = parts[3];
            }
        }

        public static Artifact From(string descriptor)
        {
            return new Artifact(descriptor);
        }
    }
    #endregion

    /// <summary>
    /// 基本数据要素类
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
    /// 基本数据要素类（包括路径）
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
