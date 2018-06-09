using NsisoLauncher.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NsisoLauncher.Core.Net.Tools
{
    public static class GetDownloadUrl
    {
        private const string BMCLUrl = "https://bmclapi2.bangbang93.com/";
        private const string BMCLLibrariesURL = BMCLUrl + "libraries/";
        private const string BMCLVersionURL = BMCLUrl + "mc/game/version_manifest.json";

        private const string MojangMainUrl = "https://launcher.mojang.com/";
        private const string MojangJsonBaseUrl = "https://launchermeta.mojang.com/";
        private const string MojangAssetsBaseUrl = "https://resources.download.minecraft.net/";
        private const string MojangVersionUrl = MojangJsonBaseUrl + "mc/game/version_manifest.json";
        private const string MojanglibrariesUrl = "https://libraries.minecraft.net/";

        private static string GetLibPath(Library lib)
        {
            return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Package.Replace(".", "\\"), lib.Name, lib.Version);
        }

        private static string GetNativePath(Native native)
        {
            return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.jar", native.Package.Replace(".", "\\"), native.Name, native.Version, native.NativeSuffix);
        }

        /// <summary>
        /// 获取Lib下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <returns>下载URL</returns>
        public static string GetLibDownloadURL(DownloadSource source, Library lib)
        {

            switch (source)
            {
                case DownloadSource.Mojang:
                    return (MojanglibrariesUrl + GetLibPath(lib)).Replace('\\', '/');

                case DownloadSource.BMCLAPI:
                    return (BMCLLibrariesURL + GetLibPath(lib)).Replace('\\', '/');

                default:
                    throw new ArgumentNullException("source");

            }
        }

        /// <summary>
        /// 获取Lib下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetLibDownloadTask(DownloadSource source, Library lib, LaunchHandler core)
        {
            string from = GetLibDownloadURL(source, lib);
            string to = core.GetLibraryPath(lib);
            return new DownloadTask("版本依赖库文件" + lib.Name, from, to);
        }

        /// <summary>
        /// 获取native下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="native">native实例</param>
        /// <returns>下载URL</returns>
        public static string GetNativeDownloadURL(DownloadSource source, Native native)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    return (MojanglibrariesUrl + GetNativePath(native)).Replace('\\', '/');

                case DownloadSource.BMCLAPI:
                    return (BMCLLibrariesURL + GetNativePath(native)).Replace('\\', '/');

                default:
                    throw new ArgumentNullException("source");

            }
        }

        /// <summary>
        /// 获取Native下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="native">native实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetNativeDownloadTask(DownloadSource source, Native native, LaunchHandler core)
        {
            string from = GetNativeDownloadURL(source, native);
            string to = core.GetNativePath(native);
            return new DownloadTask("版本系统依赖库文件" + native.Name, from, to);
        }
    }
}
