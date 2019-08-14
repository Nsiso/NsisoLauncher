using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using NsisoLauncherCore.Util.Checker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private static string GetLibBasePath(Library lib)
        {
            if (!string.IsNullOrWhiteSpace(lib.LibDownloadInfo?.Path))
            {
                return lib.LibDownloadInfo.Path;
            }
            else
            {
                return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Package.Replace(".", "\\"), lib.Name, lib.Version);
            }
        }

        private static string GetNativeBasePath(Native native)
        {
            if (!string.IsNullOrWhiteSpace(native.NativeDownloadInfo?.Path))
            {
                return native.NativeDownloadInfo.Path;
            }
            else
            {
                return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.jar", native.Package.Replace(".", "\\"), native.Name, native.Version, native.NativeSuffix);
            }
        }

        private static string GetAssetsBasePath(JAssetsInfo assetsInfo)
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
            if (ver.Downloads?.Client != null)
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
            DownloadTask downloadTask = new DownloadTask("游戏版本核心Jar文件", from, to);
            if (!string.IsNullOrWhiteSpace(version.Downloads?.Client?.SHA1))
            {
                downloadTask.Checker = new SHA1Checker() { CheckSum = version.Downloads.Client.SHA1, FilePath = to };
            }
            return downloadTask;
        }

        /// <summary>
        /// 获取Lib下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <returns>下载URL</returns>
        public static string GetLibDownloadURL(DownloadSource source, Library lib)
        {
            if (!string.IsNullOrWhiteSpace(lib.LibDownloadInfo?.URL))
            {
                return DoURLReplace(source, lib.LibDownloadInfo.URL);
            }
            else
            {
                string libUrlPath = GetLibBasePath(lib).Replace('\\', '/');
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
        }

        /// <summary>
        /// 获取Lib下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="lib">lib实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetLibDownloadTask(DownloadSource source, KeyValuePair<string, Library> lib)
        {
            string from = GetLibDownloadURL(source, lib.Value);
            string to = lib.Key;
            DownloadTask task = new DownloadTask("版本依赖库文件" + lib.Value.Name, from, to);
            if (lib.Value.LibDownloadInfo != null)
            {
                task.Checker = new SHA1Checker() { CheckSum = lib.Value.LibDownloadInfo.SHA1, FilePath = to };
            }
            return task;
        }

        /// <summary>
        /// 获取native下载地址
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="native">native实例</param>
        /// <returns>下载URL</returns>
        public static string GetNativeDownloadURL(DownloadSource source, Native native)
        {
            if (!string.IsNullOrWhiteSpace(native.NativeDownloadInfo?.URL))
            {
                return DoURLReplace(source, native.NativeDownloadInfo.URL);
            }
            else
            {
                switch (source)
                {
                    case DownloadSource.Mojang:
                        return (MojanglibrariesUrl + GetNativeBasePath(native)).Replace('\\', '/');

                    case DownloadSource.BMCLAPI:
                        return (BMCLLibrariesURL + GetNativeBasePath(native)).Replace('\\', '/');

                    default:
                        throw new ArgumentNullException("source");

                }
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
            string to = native.Key;
            DownloadTask task = new DownloadTask("版本系统依赖库文件" + native.Value.Name, from, to);
            if (native.Value.NativeDownloadInfo != null)
            {
                task.Checker = new SHA1Checker() { CheckSum = native.Value.NativeDownloadInfo.SHA1, FilePath = to };
            }
            return task;
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
                    return (MojangAssetsBaseUrl + GetAssetsBasePath(assets)).Replace('\\', '/');

                case DownloadSource.BMCLAPI:
                    return (BMCLUrl + "objects\\" + GetAssetsBasePath(assets)).Replace('\\', '/');

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

        /// <summary>
        /// 获取NIDE8核心下载任务
        /// </summary>
        /// <param name="downloadTo">下载目的路径</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetNide8CoreDownloadTask(string downloadTo)
        {
            return new DownloadTask("统一通行证核心", "https://login2.nide8.com:233/index/jar", downloadTo);
        }

        public async static Task<DownloadTask> GetAICoreDownloadTask(DownloadSource source, string downloadTo)
        {
            AuthlibInjectorAPI.APIHandler handler = new AuthlibInjectorAPI.APIHandler();
            return await handler.GetLatestAICoreDownloadTask(source, downloadTo);
        }
    }
}
