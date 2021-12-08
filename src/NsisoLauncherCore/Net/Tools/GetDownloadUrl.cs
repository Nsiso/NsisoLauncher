using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Apis;
using NsisoLauncherCore.Net.Apis.Modules;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Util;
using NsisoLauncherCore.Util.Checker;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace NsisoLauncherCore.Net.Tools
{
    public static class GetDownloadUri
    {
        public const string MojangMainUrl = "https://launcher.mojang.com/";
        public const string MojangMetaUrl = "https://launchermeta.mojang.com/";
        public const string MojangVersionUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        public const string MojanglibrariesUrl = "https://libraries.minecraft.net/";
        public const string MojangAssetsBaseUrl = "https://resources.download.minecraft.net/";
        public const string ForgeHttpUrl = "http://files.minecraftforge.net/maven/";
        public const string ForgeHttpsUrl = "https://files.minecraftforge.net/maven/";

        public static string ReplaceUriByDic(string str, Dictionary<string, string> dic)
        {
            string ret = str;
            foreach (var item in dic)
            {
                ret = ret.Replace(item.Key, item.Value);
            }
            return ret;
        }

        private static string GetAssetsBasePath(JAssetInfo assetsInfo)
        {
            return $"{(assetsInfo.Hash.Substring(0, 2))}/{assetsInfo.Hash}";
        }

        public static async Task<string> GetCoreJsonDownloadURL(string verID, IVersionListMirror mirror)
        {
            if (mirror.MirrorName == MirrorInventory.OfficalAPI)
            {
                var res = await NetRequester.HttpGetStringAsync(MojangVersionUrl);
                if (res == null)
                    return null;
                JObject obj = JObject.Parse(res);
                if (obj.ContainsKey("versions"))
                {
                    JArray array = obj["version"] as JArray;
                    var item = array.Where(a =>
                    {
                        var b = a as JObject;
                        return b?.ContainsKey("id") == true && b["id"]?.ToString() == verID;
                    }).First();
                    return item["url"].ToString();
                }
                return null;
            }
            return $"{mirror.CoreVersionListUri}{verID}/json";
        }

        //public static DownloadTask GetCoreJsonDownloadTask(string verID, LaunchHandler core, IVersionListMirror mirror)
        //{
        //    string to = core.GetVersionJsonPath(verID);
        //    string from = GetCoreJsonDownloadURL(verID, mirror);
        //    return new DownloadTask("游戏版本核心Json文件", new StringUrl(from), to);
        //}

        public static string GetCoreJarDownloadURL(VersionBase ver, IVersionListMirror mirror)
        {
            string url;
            if (ver.Downloads?.Client != null)
            {
                url = ver.Downloads.Client.Url;
            }
            else
            {
                url = $"{MojangMainUrl}version/{ver.Id}/client";
            }

            if (mirror != null)
            {
                url.Replace(MojangMainUrl, mirror.MCDownloadUri.AbsoluteUri);
            }

            return url;
        }

        public static DownloadTask GetCoreJarDownloadTask(VersionBase version, LaunchHandler core, IVersionListMirror mirror)
        {
            string to = core.GetVersionJarPath(version);
            string from = GetCoreJarDownloadURL(version, mirror);
            DownloadTask downloadTask = new DownloadTask("游戏版本核心Jar文件", new StringUrl(from), to);
            if (!string.IsNullOrWhiteSpace(version.Downloads?.Client?.Sha1))
            {
                downloadTask.DownloadObject.Checker = new SHA1Checker() { CheckSum = version.Downloads.Client.Sha1, FilePath = to };
            }
            return downloadTask;
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
            DownloadTask task = new DownloadTask("版本依赖库文件" + lib.Value.Name.Name, lib.Value, to);
            if (lib.Value.LocalDownloadInfo != null)
            {
                task.DownloadObject.Checker = new SHA1Checker() { CheckSum = lib.Value.LocalDownloadInfo.Sha1, FilePath = to };
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

        public async static Task<DownloadTask> GetAICoreDownloadTask(DownloadSource source, string downloadTo)
        {
            AuthlibInjectorAPI.APIHandler handler = new AuthlibInjectorAPI.APIHandler();
            return await handler.GetLatestAICoreDownloadTask(source, downloadTo);
        }

        private static List<IDownloadTask> GetJavaManifestDownloadTasks(JavaManifest manifest, string download_to_dir)
        {
            List<IDownloadTask> tasks = new List<IDownloadTask>();
            foreach (var item in manifest.Files)
            {
                switch (item.Value.Type)
                {
                    case "directory":
                        {
                            string dir = Path.Combine(download_to_dir, item.Key);
                            ActionDownloadTask task = new ActionDownloadTask("创建Java组件文件夹", () =>
                            {
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                            });
                            tasks.Add(task);
                            break;
                        }

                    case "file":
                        {
                            string file = Path.Combine(download_to_dir, item.Key);
                            DownloadTask task = new DownloadTask(string.Format("下载JAVA组件{0}", item.Key), new DownloadObject(item.Value.Downloads.Raw, file)
                            {
                                Checker = new SHA1Checker(item.Value.Downloads.Raw.Sha1, file)
                            });
                            tasks.Add(task);
                            break;
                        }

                    default:
                        break;
                }
            }
            return tasks;
        }

        public static List<IDownloadTask> GetJavaDownloadTasks(NativeJavaMeta native_meta)
        {
            List<IDownloadTask> tasks = GetJavaManifestDownloadTasks(native_meta.Manifest, native_meta.GetJavaDestinationPath(PathManager.RuntimeDirectory));

            tasks.Add(new ActionDownloadTask("创建Java版本信息", () =>
            {
                string file_name = string.Format("{0}\\{1}", native_meta.GetBasePath(PathManager.RuntimeDirectory), ".version");
                File.WriteAllText(file_name, native_meta.Version);
            }));

            return tasks;
        }
    }
}
