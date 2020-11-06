using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncherCore.Util
{
    public static class FileHelper
    {
        #region 文件工具
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overWrite)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, overWrite);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs, overWrite);
                }
            }
        }
        #endregion

        #region 检查Jar核心文件
        public static bool IsLostJarCore(LaunchHandler core, Version version)
        {
            if (version.InheritsFrom == null)
            {
                string jarPath = core.GetJarPath(version);
                return !File.Exists(jarPath);
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region 检查Libs库文件
        /// <summary>
        /// 获取版本是否丢失任何库文件
        /// </summary>
        /// <param name="core">启动核心</param>
        /// <param name="version">检查的版本</param>
        /// <returns>是否丢失任何库文件</returns>
        public static bool IsLostAnyLibs(LaunchHandler core, Version version)
        {
            foreach (var item in version.Libraries)
            {
                string path = core.GetLibraryPath(item);
                if (!File.Exists(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取版本丢失的库文件
        /// </summary>
        /// <param name="core">所使用的启动核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为库实例的集合</returns>
        public static Dictionary<string, Modules.Library> GetLostLibs(LaunchHandler core, Version version)
        {
            Dictionary<string, Modules.Library> lostLibs = new Dictionary<string, Modules.Library>();

            foreach (var item in version.Libraries)
            {
                string path = core.GetLibraryPath(item);
                if (lostLibs.ContainsKey(path))
                {
                    continue;
                }
                else if (!File.Exists(path))
                {
                    lostLibs.Add(path, item);
                }
            }
            return lostLibs;
        }
        #endregion

        #region 检查Natives本机文件
        /// <summary>
        /// 获取版本是否丢失任何natives文件
        /// </summary>
        /// <param name="core"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool IsLostAnyNatives(LaunchHandler core, Version version)
        {
            foreach (var item in version.Natives)
            {
                string path = core.GetNativePath(item);
                if (!File.Exists(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取版本丢失的natives文件
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为native实例的集合</returns>
        public static Dictionary<string, Native> GetLostNatives(LaunchHandler core, Version version)
        {
            Dictionary<string, Native> lostNatives = new Dictionary<string, Native>();

            foreach (var item in version.Natives)
            {
                string path = core.GetNativePath(item);
                if (lostNatives.ContainsKey(path))
                {
                    continue;
                }
                else if (!File.Exists(path))
                {
                    lostNatives.Add(path, item);
                }
            }
            return lostNatives;
        }
        #endregion

        #region 检查Assets资源文件
        private static bool IsLostAnyAssetsFromJassets(LaunchHandler core, JAssets assets)
        {
            if (assets == null)
            {
                return false;
            }
            foreach (var item in assets.Objects)
            {

                string path = core.GetAssetsPath(item.Value);
                if (!File.Exists(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取版本丢失的资源文件
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为资源文件信息实例的集合</returns>
        public static Dictionary<string, JAssetInfo> GetLostAssets(LaunchHandler core, JAssets assets)
        {
            Dictionary<string, JAssetInfo> lostAssets = new Dictionary<string, JAssetInfo>();
            if (assets == null)
            {
                return lostAssets;
            }
            foreach (var item in assets.Objects)
            {
                string path = core.GetAssetsPath(item.Value);
                if ((!lostAssets.ContainsKey(path)) && (!File.Exists(path)))
                {
                    lostAssets.Add(path, item.Value);
                }
            }
            return lostAssets;
        }

        public static async Task<bool> IsLostAssetsAsync(LaunchHandler core, Version ver)
        {
            string assetsPath = core.GetAssetsIndexPath(ver.Assets);
            if (!File.Exists(assetsPath))
            {
                return (ver.AssetIndex != null);
            }
            else
            {
                var assets = await core.GetAssetsAsync(ver);
                return await Task.Factory.StartNew(() =>
                {
                    return IsLostAnyAssetsFromJassets(core, assets);
                });
            }
        }
        #endregion

        #region 丢失依赖文件帮助
        //作者觉得使用这个方法判断是否丢失文件不如直接根据GetLostDependDownloadTask方法返回的列表的Count数来判断
        //毕竟这种方法大部分用在启动前判断是否丢失文件，但是如果丢失还是要获取一次列表，效率并没怎样优化
        //public static bool IsLostAnyDependent(LaunchHandler core, Version version)
        //{
        //    return IsLostJarCore(core, version) || IsLostAnyLibs(core, version) || IsLostAnyNatives(core, version);
        //}

        /// <summary>
        /// 获取全部丢失的文件下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="core">使用的核心</param>
        /// <param name="version">检查的版本</param>
        /// <returns></returns>
        public async static Task<List<DownloadTask>> GetLostDependDownloadTaskAsync(LaunchHandler core, Version version, IList<IVersionListMirror> mirrors, NetRequester netRequester)
        {
            var lostLibs = GetLostLibs(core, version);
            var lostNatives = GetLostNatives(core, version);
            List<DownloadTask> tasks = new List<DownloadTask>();
            IVersionListMirror mirror = null;
            if ((mirrors != null) && (mirrors.Count != 0))
            {
                mirror = (IVersionListMirror)await MirrorHelper.ChooseBestMirror(mirrors);
            }
            if (IsLostJarCore(core, version))
            {
                if (version.Jar == null)
                {
                    tasks.Add(GetDownloadUri.GetCoreJarDownloadTask(version, core, mirror));
                }
            }

            if (version.InheritsFrom != null)
            {
                string innerJsonPath = core.GetJsonPath(version.InheritsFrom);
                string innerJsonStr = null;
                if (!File.Exists(innerJsonPath))
                {
                    if (mirror == null)
                    {
                        if (mirrors.Count == 0)
                        {
                            throw new Exception("no Version List Mirror");
                        }
                        else if (mirrors.Count == 1)
                        {
                            mirror = mirrors.First();
                        }
                        else
                        {
                            mirror = ((IVersionListMirror)await MirrorHelper.ChooseBestMirror(mirrors));
                        }
                    }
                    HttpResponseMessage jsonRespond = await netRequester.Client.GetAsync(GetDownloadUri.GetCoreJsonDownloadURL(version.InheritsFrom, mirror));
                    if (jsonRespond.IsSuccessStatusCode)
                    {
                        innerJsonPath = await jsonRespond.Content.ReadAsStringAsync();
                    }
                    if (!string.IsNullOrWhiteSpace(innerJsonStr))
                    {
                        string jsonFolder = Path.GetDirectoryName(innerJsonPath);
                        if (!Directory.Exists(jsonFolder))
                        {
                            Directory.CreateDirectory(jsonFolder);
                        }
                        File.WriteAllText(innerJsonPath, innerJsonStr);
                    }
                    throw new Exception("缺少inner json");
                }
                else
                {
                    innerJsonStr = File.ReadAllText(innerJsonPath);
                }
                Version innerVer = core.JsonToVersion(innerJsonStr);
                if (innerVer != null)
                {
                    tasks.AddRange(await GetLostDependDownloadTaskAsync(core, innerVer, mirrors, netRequester));
                }

            }
            foreach (var item in lostLibs)
            {
                tasks.Add(GetDownloadUri.GetLibDownloadTask(item));
            }
            foreach (var item in lostNatives)
            {
                tasks.Add(GetDownloadUri.GetNativeDownloadTask(item));
            }
            return tasks;
        }
        #endregion


        /// <summary>
        /// 获取丢失的资源文件下载任务
        /// </summary>
        /// <param name="source"></param>
        /// <param name="core"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static IDownloadTask GetLostAssetsDownloadTaskAsync(LaunchHandler core, Version ver)
        {
            JAssets assets = null;

            string assetsPath = core.GetAssetsIndexPath(ver.Assets);
            if (!File.Exists(assetsPath))
            {
                if (ver.AssetIndex != null)
                {
                    string jsonUrl = ver.AssetIndex.Url;
                    return new DownloadTask("资源文件引导", new StringUrl(jsonUrl), assetsPath);
                    //HttpResponseMessage jsonRespond = await NetRequester.Client.GetAsync(jsonUrl);
                    //string assetsJson = null;
                    //if (jsonRespond.IsSuccessStatusCode)
                    //{
                    //    assetsJson = await jsonRespond.Content.ReadAsStringAsync();
                    //}
                    //if (!string.IsNullOrWhiteSpace(assetsJson))
                    //{
                    //    assets = core.GetAssetsByJson(assetsJson);
                    //    tasks.Add(new DownloadTask("资源文件引导", jsonUrl, assetsPath));
                    //}
                }
                else
                {
                    return null;
                }
            }
            else
            {
                assets = core.GetAssets(ver);
            }
            var lostAssets = GetLostAssets(core, assets);
            List<DownloadObject> downloadObjects = new List<DownloadObject>();
            foreach (var item in lostAssets)
            {
                downloadObjects.Add(GetDownloadUri.GetAssetsDownloadObject(item.Value, core));
            }
            return new GroupDownloadTask("资源文件", downloadObjects, ver.AssetIndex.TotalSize);
        }
    }
}
