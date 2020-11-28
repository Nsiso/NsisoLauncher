using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NsisoLauncherCore.Net.Tools;

namespace NsisoLauncherCore.Modules
{
    public class Library : IDownloadable
    {
        [JsonProperty("name")]
        public Artifact Artifact { get; set; }

        /// <summary>
        /// 库文件下载信息
        /// </summary>
        public DownloadInfo Downloads { get; set; }

        /// <summary>
        /// 本机库信息
        /// </summary>
        public NativesInfo Natives { get; set; }

        /// <summary>
        /// 提取规则
        /// </summary>
        public ExtractInfo Extract { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        public List<RuleInfo> Rules { get; set; }

        /// <summary>
        /// 下载URL（旧版本部分json会有的参数，平常不应使用）
        /// </summary>
        public string Url { get; set; }

        public Library(string descriptor)
        {
            this.Artifact = new Artifact(descriptor);
        }

        public string GetDownloadSourceURL()
        {
            return GetDownloadUri.GetLibDownloadURL(this);
        }
    }

    /// <summary>
    /// 下载信息
    /// </summary>
    public class DownloadInfo
    {
        /// <summary>
        /// 游戏引用jar工件
        /// </summary>
        public PathSha1SizeUrl Artifact { get; set; }

        /// <summary>
        /// 游戏使用系统库（如windows下dll）文件
        /// </summary>
        public Dictionary<string, PathSha1SizeUrl> Classifiers { get; set; }
    }

    /// <summary>
    /// 本机库文件信息，对应Classifiers key
    /// </summary>
    public class NativesInfo
    {
        /// <summary>
        /// Linux系统
        /// </summary>
        public string Linux { get; set; }

        /// <summary>
        /// 苹果osx系统
        /// </summary>
        public string Osx { get; set; }

        /// <summary>
        /// windows系统
        /// </summary>
        public string Windows { get; set; }
    }

    /// <summary>
    /// 本机库文件提取规则
    /// </summary>
    public class ExtractInfo
    {
        /// <summary>
        /// 排除文件/文件夹
        /// </summary>
        public List<string> Exclude { get; set; }
    }

    public class RuleInfo
    {
        /// <summary>
        /// 是否执行（Allow/Disallow）
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 操作系统要求
        /// </summary>
        public OsInfo Os { get; set; }
    }

    /// <summary>
    /// 系统信息
    /// </summary>
    public class OsInfo
    {
        /// <summary>
        /// 系统名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 系统版本
        /// </summary>
        public string Version { get; set; }
    }
}
