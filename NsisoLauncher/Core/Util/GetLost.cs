using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Net;
using NsisoLauncher.Core.Net.FunctionAPI;
using NsisoLauncher.Core.Net.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NsisoLauncher.Core.Util
{
    public static class GetLost
    {
        /// <summary>
        /// 获取版本丢失的库文件
        /// </summary>
        /// <param name="core">所使用的启动核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为库实例的集合</returns>
        public static Dictionary<string, Modules.Library> GetLostLibs(LaunchHandler core, Modules.Version version)
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

        /// <summary>
        /// 获取版本丢失的natives文件
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为native实例的集合</returns>
        public static Dictionary<string, Native> GetLostNatives(LaunchHandler core, Modules.Version version)
        {
            Dictionary<string, Native> lostNatives = new Dictionary<string, Native>();

            try
            {
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
            catch
            {
                return lostNatives;
            }
        }

        /// <summary>
        /// 获取版本丢失的资源文件
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为资源文件信息实例的集合</returns>
        public static Dictionary<string, JAssetsInfo> GetLostAssets(LaunchHandler core, JAssets assets)
        {
            Dictionary<string, JAssetsInfo> lostAssets = new Dictionary<string, JAssetsInfo>();
            try
            {
                if (assets == null)
                {
                    return lostAssets;
                }
                foreach (var item in assets.Objects)
                {

                    string path = core.GetAssetsPath(item.Value);
                    if (lostAssets.ContainsKey(path))
                    {
                        continue;
                    }
                    else if (!File.Exists(path))
                    {
                        lostAssets.Add(item.Key, item.Value);
                    }
                }

                return lostAssets;
            }
            catch
            {
                return lostAssets;
            }
        }

        /// <summary>
        /// 返回全部丢失的文件路径
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>丢失文件的路径列表</returns>
        public static List<string> GetAllLostDepend(LaunchHandler core, Modules.Version version)
        {
            List<string> lost = new List<string>();
            string jarPath = core.GetJarPath(version);
            if (!File.Exists(jarPath))
            {
                lost.Add(jarPath);
            }
            lost.AddRange(GetLostLibs(core, version).Keys);
            lost.AddRange(GetLostNatives(core, version).Keys);
            return lost;
        }

        /// <summary>
        /// 获取全部丢失的文件下载任务
        /// </summary>
        /// <param name="source">下载源</param>
        /// <param name="core">使用的核心</param>
        /// <param name="version">检查的版本</param>
        /// <returns></returns>
        public static List<DownloadTask> GetLostDependDownloadTask(DownloadSource source, LaunchHandler core, Version version)
        {
            var lostLibs = GetLostLibs(core, version);
            var lostNatives = GetLostNatives(core, version);
            List<DownloadTask> tasks = new List<DownloadTask>();
            string jarPath = core.GetJarPath(version);
            if (!File.Exists(jarPath))
            {
                tasks.Add(GetDownloadUrl.GetCoreDownloadTask(source, version, core));
            }
            foreach (var item in lostLibs)
            {
                tasks.Add(GetDownloadUrl.GetLibDownloadTask(source, item.Value, core));
            }
            foreach (var item in lostNatives)
            {
                tasks.Add(GetDownloadUrl.GetNativeDownloadTask(source, item.Value, core));
            }
            return tasks;
        }

        public static Task<List<DownloadTask>> GetLostDependDownloadTaskAsync(DownloadSource source, LaunchHandler core, Version version)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetLostDependDownloadTask(source, core, version);
            });
        }

        public static async Task<bool> IsLostAssetsAsync(DownloadSource source, LaunchHandler core, Version ver)
        {
            string assetsPath = core.GetAssetsIndexPath(ver.AssetIndex.ID);
            if (!File.Exists(assetsPath))
            {
                return true;
            }
            else
            {
                var assets = await core.GetAssetsAsync(ver);
                return await Task.Factory.StartNew(() =>
                {
                    foreach (var item in assets.Infos.Objects)
                    {
                        if (File.Exists(core.GetAssetsPath(item.Value)))
                        {
                            continue;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                });
            }
        }

        /// <summary>
        /// 获取丢失的资源文件下载任务
        /// </summary>
        /// <param name="source"></param>
        /// <param name="core"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static List<DownloadTask> GetLostAssetsDownloadTask(DownloadSource source, LaunchHandler core, Version ver)
        {
            List<DownloadTask> tasks = new List<DownloadTask>();
            JAssets assets = null;
            if (ver.AssetIndex != null)
            {
                string assetsPath = core.GetAssetsIndexPath(ver.AssetIndex.ID);
                if (!File.Exists(assetsPath))
                {
                    string jsonUrl = GetDownloadUrl.DoURLReplace(source, ver.AssetIndex.URL);
                    string assetsJson = FunctionAPIHandler.HttpGet(jsonUrl);
                    assets = core.GetAssetsByJson(assetsJson);
                    tasks.Add(new DownloadTask("资源文件引导", jsonUrl, assetsPath));
                }
                else
                {
                    assets = core.GetAssets(ver).Infos;
                }
            }
            var lostAssets = GetLostAssets(core, assets);
            foreach (var item in lostAssets)
            {
                DownloadTask task = GetDownloadUrl.GetAssetsDownloadTask(source, item.Value, core);
                if (!tasks.Contains(task))
                {
                    tasks.Add(task);
                }
            }
            return tasks;
        }

        public static Task<List<DownloadTask>> GetLostAssetsDownloadTaskAsync(DownloadSource source, LaunchHandler core, Version ver)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetLostAssetsDownloadTask(source, core, ver);
            });
        }
    }
}
