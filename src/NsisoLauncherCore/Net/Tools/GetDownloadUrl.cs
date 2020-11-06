using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Util;
using NsisoLauncherCore.Util.Checker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Tools
{
    public static class GetDownloadUri
    {
        public static string MojangMainUrl { get => "https://launcher.mojang.com/"; }
        public static string MojangMetaUrl { get => "https://launchermeta.mojang.com/"; }
        public static string MojangVersionUrl { get => MojangMetaUrl + "mc/game/version_manifest.json"; }
        public static string MojanglibrariesUrl { get => "https://libraries.minecraft.net/"; }
        public static string MojangAssetsBaseUrl { get => "https://resources.download.minecraft.net/"; }
        public static string ForgeHttpUrl { get => "http://files.minecraftforge.net/maven/"; }
        public static string ForgeHttpsUrl { get => "https://files.minecraftforge.net/maven/"; }

        public static string ReplaceUriByDic(string str, Dictionary<string, string> dic)
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
                return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Artifact.Package.Replace(".", "\\"), lib.Artifact.Name, lib.Artifact.Version);
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
                return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.jar", native.Artifact.Package.Replace(".", "\\"), native.Artifact.Name, native.Artifact.Version, native.NativeSuffix);
            }
        }

        private static string GetAssetsBasePath(JAssetInfo assetsInfo)
        {
            return string.Format(@"{0}\{1}", assetsInfo.Hash.Substring(0, 2), assetsInfo.Hash);
        }

        public static string GetCoreJsonDownloadURL(string verID, IVersionListMirror mirror)
        {
            return string.Format("{0}version/{1}/json", mirror.BaseUri, verID);
        }

        public static DownloadTask GetCoreJsonDownloadTask(string verID, LaunchHandler core, IVersionListMirror mirror)
        {
            string to = core.GetJsonPath(verID);
            string from = GetCoreJsonDownloadURL(verID, mirror);
            return new DownloadTask("游戏版本核心Json文件", new StringUrl(from), to);
        }

        public static string GetCoreJarDownloadURL(Modules.Version ver, IVersionListMirror mirror)
        {
            string url;
            if (ver.Downloads?.Client != null)
            {
                url = ver.Downloads.Client.Url;

            }
            else
            {
                url = string.Format("{0}version/{1}/client", MojangMainUrl, ver.Id);
            }

            if (mirror != null)
            {
                url.Replace(MojangMainUrl, mirror.BaseUri.AbsoluteUri);
            }

            return url;
        }

        public static DownloadTask GetCoreJarDownloadTask(Modules.Version version, LaunchHandler core, IVersionListMirror mirror)
        {
            string to = core.GetJarPath(version);
            string from = GetCoreJarDownloadURL(version, mirror);
            DownloadTask downloadTask = new DownloadTask("游戏版本核心Jar文件", new StringUrl(from), to);
            if (!string.IsNullOrWhiteSpace(version.Downloads?.Client?.Sha1))
            {
                downloadTask.DownloadObject.Checker = new SHA1Checker() { CheckSum = version.Downloads.Client.Sha1, FilePath = to };
            }
            return downloadTask;
        }

        /// <summary>
        /// 获取Lib下载地址
        /// </summary>
        /// <param name="lib">lib实例</param>
        /// <returns>下载URL</returns>
        public static string GetLibDownloadURL(Library lib)
        {
            if (!string.IsNullOrWhiteSpace(lib.LibDownloadInfo?.Url))
            {
                return lib.LibDownloadInfo.Url;
            }
            else if (!string.IsNullOrWhiteSpace(lib.Url))
            {
                string libUrlPath = GetLibBasePath(lib).Replace('\\', '/');
                return lib.Url + libUrlPath;
            }
            else
            {
                string libUrlPath = GetLibBasePath(lib).Replace('\\', '/');
                return MojanglibrariesUrl + libUrlPath;
            }
        }

        /// <summary>
        /// 获取Lib下载任务
        /// </summary>
        /// <param name="lib">lib实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetLibDownloadTask(KeyValuePair<string, Library> lib)
        {
            string to = lib.Key;
            DownloadTask task = new DownloadTask("版本依赖库文件" + lib.Value.Artifact.Name, lib.Value, to);
            if (lib.Value.LibDownloadInfo != null)
            {
                task.DownloadObject.Checker = new SHA1Checker() { CheckSum = lib.Value.LibDownloadInfo.Sha1, FilePath = to };
            }
            return task;
        }

        /// <summary>
        /// 获取native下载地址
        /// </summary>
        /// <param name="native">native实例</param>
        /// <returns>下载URL</returns>
        public static string GetNativeDownloadURL(Native native)
        {
            if (!string.IsNullOrWhiteSpace(native.NativeDownloadInfo?.Url))
            {
                return native.NativeDownloadInfo.Url;
            }
            else
            {
                return (MojanglibrariesUrl + GetNativeBasePath(native)).Replace('\\', '/');
            }
        }

        /// <summary>
        /// 获取Native下载任务
        /// </summary>
        /// <param name="native">native实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetNativeDownloadTask(KeyValuePair<string, Native> native)
        {
            string to = native.Key;
            DownloadTask task = new DownloadTask("版本系统依赖库文件" + native.Value.Artifact.Name, native.Value, to);
            if (native.Value.NativeDownloadInfo != null)
            {
                task.DownloadObject.Checker = new SHA1Checker() { CheckSum = native.Value.NativeDownloadInfo.Sha1, FilePath = to };
            }
            return task;
        }

        /// <summary>
        /// 获取assets下载地址
        /// </summary>
        /// <param name="assets">assets实例</param>
        /// <returns>下载URL</returns>
        public static string GetAssetsDownloadURL(JAssetInfo assets)
        {
            return (MojangAssetsBaseUrl + GetAssetsBasePath(assets)).Replace('\\', '/');
        }

        /// <summary>
        /// 获取assets下载任务
        /// </summary>
        /// <param name="assets">assets实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetAssetsDownloadTask(JAssetInfo assets, LaunchHandler core)
        {
            string to = core.GetAssetsPath(assets);
            return new DownloadTask("游戏资源文件" + assets.Hash, assets, to);
        }

        /// <summary>
        /// 获取assets下载任务
        /// </summary>
        /// <param name="assets">assets实例</param>
        /// <param name="core">所使用的核心</param>
        /// <returns>下载任务</returns>
        public static DownloadObject GetAssetsDownloadObject(JAssetInfo assets, LaunchHandler core)
        {
            string to = core.GetAssetsPath(assets);
            return new DownloadObject(assets, to);
        }

        /// <summary>
        /// 获取NIDE8核心下载任务
        /// </summary>
        /// <param name="downloadTo">下载目的路径</param>
        /// <returns>下载任务</returns>
        public static DownloadTask GetNide8CoreDownloadTask(string downloadTo)
        {
            return new DownloadTask("统一通行证核心", new StringUrl("https://login2.nide8.com:233/index/jar"), downloadTo);
        }

        public async static Task<DownloadTask> GetAICoreDownloadTask(DownloadSource source, string downloadTo, NetRequester requester)
        {
            AuthlibInjectorAPI.APIHandler handler = new AuthlibInjectorAPI.APIHandler();
            return await handler.GetLatestAICoreDownloadTask(source, downloadTo, requester);
        }
    }
}
