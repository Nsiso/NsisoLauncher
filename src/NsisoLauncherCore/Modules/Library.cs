using NsisoLauncherCore.Net;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Util;
using Newtonsoft.Json.Linq;

namespace NsisoLauncherCore.Modules
{
    [JsonConverter(typeof(LibraryJsonConverter))]
    public class Library : IDownloadable
    {
        /// <summary>
        /// 库名称
        /// </summary>
        [JsonProperty("name")]
        public Artifact Name { get; set; }

        /// <summary>
        /// 下载引导
        /// </summary>
        [JsonProperty("downloads")]
        public Downloads Downloads { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        [JsonProperty("rules")]
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// 下载URL第一种表达
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        public virtual string GetDownloadSourceURL()
        {
            if (!string.IsNullOrWhiteSpace(LocalDownloadInfo?.Url))
            {
                return LocalDownloadInfo.Url;
            }
            else if (!string.IsNullOrWhiteSpace(Url))
            {
                string libUrlPath = Path.Replace('\\', '/');
                return Url + libUrlPath;
            }
            else
            {
                string libUrlPath = Path.Replace('\\', '/');
                return GetDownloadUri.MojanglibrariesUrl + libUrlPath;
            }
        }

        public virtual bool IsEnable()
        {
            return RuleChecker.CheckRules(Rules);
        }

        public virtual PathSha1SizeUrl LocalDownloadInfo
        {
            get
            {
                return Downloads?.Artifact;
            }
        }

        public virtual string Path
        {
            get
            {
                if (LocalDownloadInfo != null && !string.IsNullOrWhiteSpace(LocalDownloadInfo.Path))
                {
                    return LocalDownloadInfo.Path.Replace('/', '\\');
                }
                else
                {
                    return Name.Path;
                }
            }
        }

        public virtual bool IsNative()
        {
            return false;
        }

        public Library()
        {
        }

        public Library(string descriptor)
        {
            this.Name = new Artifact(descriptor);
        }
    }

    public class LibraryNameEqualityComparer : IEqualityComparer<Library>
    {
        public bool Equals(Library x, Library y)
        {
            return x.Name.Package == y.Name.Package &&
                   x.Name.Name == y.Name.Name &&
                   x.Name.Extension == y.Name.Extension &&
                   x.Name.Classifier == y.Name.Classifier &&
                   x.IsNative() == y.IsNative();
        }

        public int GetHashCode(Library obj)
        {
            return string.Format("{0}-{1}-{2}.{3} {4}", obj.Name.Package, obj.Name.Name, obj.Name.Classifier, obj.Name.Extension, obj.IsNative() ? "native" : "artifact").GetHashCode();
        }
    }

    public class Native : Library
    {
        /// <summary>
        /// Native列表
        /// </summary>
        [JsonProperty("natives")]
        public Dictionary<string, string> Natives { get; set; }

        /// <summary>
        /// 解压声明
        /// </summary>
        [JsonProperty("extract")]
        public Extract Extract { get; set; }

        /// <summary>
        /// Get native suffix
        /// </summary>
        public string NativeSuffix
        {
            get
            {
                if (Natives.ContainsKey("windows"))
                {
                    string arch;
                    switch (SystemTools.GetSystemArch())
                    {
                        case ArchEnum.x32:
                            arch = "32";
                            break;
                        case ArchEnum.x64:
                            arch = "64";
                            break;
                        default:
                            arch = "32";
                            break;
                    }
                    return Natives["windows"].Replace("${arch}", arch);
                }
                else
                {
                    return null;
                }
            }
        }

        override public string GetDownloadSourceURL()
        {
            if (!string.IsNullOrWhiteSpace(LocalDownloadInfo?.Url))
            {
                return LocalDownloadInfo.Url;
            }
            else
            {
                return (GetDownloadUri.MojanglibrariesUrl + Path).Replace('\\', '/');
            }
        }

        public override bool IsEnable()
        {
            return RuleChecker.CheckRules(Rules) && Natives.ContainsKey("windows");
        }

        override public PathSha1SizeUrl LocalDownloadInfo
        {
            get
            {
                if (NativeSuffix == null)
                {
                    return null;
                }
                else
                {
                    return Downloads.Classifiers[NativeSuffix];
                }
            }
        }

        override public string Path
        {
            get
            {
                if (LocalDownloadInfo != null && !string.IsNullOrWhiteSpace(LocalDownloadInfo.Path))
                {
                    return LocalDownloadInfo.Path;
                }
                else
                {
                    return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.jar", Name.Package.Replace(".", "\\"), Name.Name, Name.Version, NativeSuffix);
                }
            }
        }

        public override bool IsNative()
        {
            return true;
        }
    }

    public class Rule
    {
        /// <summary>
        /// action
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        [JsonProperty("os")]
        public OperatingSystem OS { get; set; }

        /// <summary>
        /// Allowed features
        /// </summary>
        [JsonProperty("features")]
        public JObject Features { get; set; }
    }

    public class OperatingSystem
    {
        /// <summary>
        /// 系统名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Support os version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Support os arch
        /// </summary>
        [JsonProperty("arch")]
        public string Arch { get; set; }
    }

    public class Extract
    {
        /// <summary>
        /// 排除列表
        /// </summary>
        [JsonProperty("exclude")]
        public List<string> Exculde { get; set; }
    }

    public class Downloads
    {
        /// <summary>
        /// 库文件
        /// </summary>
        [JsonProperty("artifact")]
        public PathSha1SizeUrl Artifact { get; set; }

        /// <summary>
        /// 本地组件文件
        /// </summary>
        [JsonProperty("classifiers")]
        public Dictionary<string, PathSha1SizeUrl> Classifiers { get; set; }
    }

    //public class Library : IDownloadable
    //{
    //    public Artifact Artifact { get; set; }

    //    /// <summary>
    //    /// 下载URL
    //    /// </summary>
    //    public string Url { get; set; }

    //    /// <summary>
    //    /// 库文件下载信息
    //    /// </summary>
    //    public PathSha1SizeUrl LibDownloadInfo { get; set; }

    //    public Library(string descriptor)
    //    {
    //        this.Artifact = new Artifact(descriptor);
    //    }

    //    public string GetDownloadSourceURL()
    //    {
    //        return GetDownloadUri.GetLibDownloadURL(this);
    //    }
    //}


    //public class Native : Library, IDownloadable
    //{
    //    /// <summary>
    //    /// windows系统修改后缀
    //    /// </summary>
    //    public string NativeSuffix { get; set; }

    //    /// <summary>
    //    /// 不解压的文件夹
    //    /// </summary>
    //    public List<string> Exclude { get; set; }

    //    /// <summary>
    //    /// native文件下载信息
    //    /// </summary>
    //    public PathSha1SizeUrl NativeDownloadInfo { get; set; }

    //    public Native(string descriptor, string nativeSuffix) : base(descriptor)
    //    {
    //        this.NativeSuffix = nativeSuffix;
    //    }
    //    public new string GetDownloadSourceURL()
    //    {
    //        return GetDownloadUri.GetNativeDownloadURL(this);
    //    }
    //}
}
