using Newtonsoft.Json;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;
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
        public AssetIndex AssetIndex { get; set; }

        /// <summary>
        /// 资源ID
        /// </summary>
        public string Assets { get; set; }

        /// <summary>
        /// 下载引导
        /// </summary>
        public CoreDownloads Downloads { get; set; }

        /// <summary>
        /// 版本ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 继承版本
        /// </summary>
        public string InheritsFrom { get; set; }

        /// <summary>
        /// 使用Java版本
        /// </summary>
        public JavaVersion JavaVersion { get; set; }

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
        public string MainClass { get; set; }

        /// <summary>
        /// 指定JAR包
        /// </summary>
        public string Jar { get; set; }

        /// <summary>
        /// JVM启动参数
        /// </summary>
        [JsonIgnore]
        public string JvmArguments { get; set; }

        /// <summary>
        /// Minecraft启动参数
        /// </summary>
        public string MinecraftArguments { get; set; }

        /// <summary>
        /// 启动器最低能启动版本号
        /// </summary>
        public int MinimumLauncherVersion { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime ReleaseTime { get; set; }

        /// <summary>
        /// 版本类型
        /// </summary>
        public string Type { get; set; }
    }

    #region assetsIndex
    public class AssetIndex : Sha1SizeUrl
    {
        /// <summary>
        /// 资源ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 总大小
        /// </summary>
        public long TotalSize { get; set; }
    }
    #endregion

    #region coreDownloads
    public class CoreDownloads
    {
        /// <summary>
        /// 客户端下载信息
        /// </summary>
        public Sha1SizeUrl Client { get; set; }

        /// <summary>
        /// 服务端下载信息
        /// </summary>
        public Sha1SizeUrl Server { get; set; }
    }
    #endregion

    #region Java
    public class JavaVersion
    {
        public string Component { get; set; }

        public int MajorVersion { get; set; }
    }
    #endregion
}
