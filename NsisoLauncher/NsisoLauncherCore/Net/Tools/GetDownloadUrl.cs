using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.Tools
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

        public static string DoURLReplace(DownloadSource source, string url)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    return url;

                case DownloadSource.BMCLAPI:
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add(@"https://launcher.mojang.com/", BMCLUrl);
                    dic.Add(@"https://launchermeta.mojang.com/", BMCLUrl);
                    dic.Add(@"http://files.minecraftforge.net/maven/", BMCLLibrariesURL);
                    return ReplaceURLByDic(url, dic);

                default:
                    return null;
            }
        }

        private static string ReplaceURLByDic(string str, Dictionary<string, string> dic)
        {
            string ret = str;
            foreach (var item in dic)
            {
                ret = ret.Replace(item.Key, item.Value);
            }
            return ret;
        }

        private static string GetLibPath(Modules.Library lib)
        {
            return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Package.Replace(".", "\\"), lib.Name, lib.Version);
        }

        private static string GetNativePath(Native native)
        {
            return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.jar", native.Package.Replace(".", "\\"), native.Name, native.Version, native.NativeSuffix);
        }

        private static string GetAssetsPath(JAssetsInfo assetsInfo)
        {
            return String.Format(@"{0}\{1}", assetsInfo.Hash.Substring(0, 2), assetsInfo.Hash);
        }

        public static string GetCoreJsonDownloadURL(DownloadSource source, string verID)
        {
            return string.Format("{0}version/{1}/json", BMCLUrl, verID);
        }

        public static DownloadTask GetCoreJsonDownloadTask(DownloadSource downloadSource, string verID, LaunchHandler core)
        {
            string to = core.GetJsonPath(verID);
            string from = GetCoreJsonDownloadURL(downloadSource, verID);
            return new DownloadTask("游戏版本核心Json文件", from, to);
        }

        public static string GetCoreJarDownloadURL(DownloadSource source, Modules.Version ver)
        {
            if (ver.Downloads != null)
            {
                switch (source)
                {
                    case DownloadSource.Mojang:
                        return ver.Downloads.Client.URL;
                    case DownloadSource.BMCLAPI:
                        return ver.Downloads.Client.URL.Replace(MojangMainUrl, BMCLUrl);
                    default:
                        return ver.Downloads.Client.URL;
                }

            }
            else
            {
                return string.Format("{0}version/{1}/client", BMCLUrl, ver.ID);
            }
        }

        public static DownloadTask GetCoreJarDownloadTask(DownloadSource downloadSource, Modules.Version version, LaunchHandler core)
        {
            string to = core.GetJarPath(version);
            string from = GetCoreJarDownloadURL(downloadSource, version);
            return new DownloadTask("游戏版本核心Jar文件", from, to);
        }

        /// <summary>
        /// 获取Lib下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <returns>下载URL</returns>
        public static string GetLibDownloadURL(DownloadSource source, Modules.Library lib)
        {
            string libUrlPath = GetLibPath(lib).Replace('\\', '/');
            if (lib.Url != null)
            {
                return DoURLReplace(source, lib.Url) + libUrlPath;
            }
            else
            {
                switch (source)
                {
                    case DownloadSource.Mojang:
                        return MojanglibrariesUrl + libUrlPath;

                    case DownloadSource.BMCLAPI:
                        return BMCLLibrariesURL + libUrlPath;

                    default:
                        throw new ArgumentNullException("source");

                }
            }
        }

        /// <summary>
        /// 获取Lib下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetLibDownloadTask(DownloadSource source, KeyValuePair<string, Modules.Library> lib)
        {
            string from = GetLibDownloadURL(source, lib.Value);
            return new DownloadTask("版本依赖库文件" + lib.Value.Name, from, lib.Key);
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
        public static DownloadTask GetNativeDownloadTask(DownloadSource source, KeyValuePair<string, Native> native)
        {
            string from = GetNativeDownloadURL(source, native.Value);
            return new DownloadTask("版本系统依赖库文件" + native.Value.Name, from, native.Key);
        }

        /// <summary>
        /// 获取assets下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="assets">assets实例</param>
        /// <returns>下载URL</returns>
        public static string GetAssetsDownloadURL(DownloadSource source, JAssetsInfo assets)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    return (MojangAssetsBaseUrl + GetAssetsPath(assets)).Replace('\\', '/');

                case DownloadSource.BMCLAPI:
                    return (BMCLUrl + "objects\\" + GetAssetsPath(assets)).Replace('\\', '/');

                default:
                    throw new ArgumentNullException("source");

            }
        }

        /// <summary>
        /// 获取assets下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="assets">assets实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetAssetsDownloadTask(DownloadSource source, JAssetsInfo assets, LaunchHandler core)
        {
            string from = GetAssetsDownloadURL(source, assets);
            string to = core.GetAssetsPath(assets);
            return new DownloadTask("游戏资源文件" + assets.Hash, from, to);
        }
    }
}
