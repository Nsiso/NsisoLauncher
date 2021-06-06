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

        public string GetDownloadSourceURL()
        {
            return GetDownloadUri.GetLibDownloadURL(this);
        }
    }


    public class Native : Library, IDownloadable
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
        public new string GetDownloadSourceURL()
        {
            return GetDownloadUri.GetNativeDownloadURL(this);
        }
    }
}
