using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Tools;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util
{
    public static class FileHelper
    {
        #region 文件工具
        public static string GetSHA1(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retval = sha1.ComputeHash(file);
            file.Close();

            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < retval.Length; i++)
            {
                sc.Append(retval[i].ToString("x2"));
            }
            return sc.ToString();
        }
        #endregion

        #region 检查Jar核心文件
        public static bool IsLostJarCore(LaunchHandler core, Version version)
        {
            if (version.InheritsVersion == null)
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
        public static Dictionary<string, JAssetsInfo> GetLostAssets(LaunchHandler core, JAssets assets)
        {
            Dictionary<string, JAssetsInfo> lostAssets = new Dictionary<string, JAssetsInfo>();
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

        public static async Task<bool> IsLostAssetsAsync(DownloadSource source, LaunchHandler core, Version ver)
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
        public async static Task<List<DownloadTask>> GetLostDependDownloadTaskAsync(DownloadSource source, LaunchHandler core, Version version)
        {
            var lostLibs = GetLostLibs(core, version);
            var lostNatives = GetLostNatives(core, version);
            List<DownloadTask> tasks = new List<DownloadTask>();
            if (IsLostJarCore(core, version))
            {
                if (version.Jar == null)
                {
                    tasks.Add(GetDownloadUrl.GetCoreJarDownloadTask(source, version, core));
                }
            }

            if (version.InheritsVersion != null)
            {
                string innerJsonPath = core.GetJsonPath(version.InheritsVersion);
                string innerJsonStr;
                if (!File.Exists(innerJsonPath))
                {
                    innerJsonStr = await APIRequester.HttpGetStringAsync(GetDownloadUrl.GetCoreJsonDownloadURL(source, version.InheritsVersion));
                    string jsonFolder = Path.GetDirectoryName(innerJsonPath);
                    if (!Directory.Exists(jsonFolder))
                    {
                        Directory.CreateDirectory(jsonFolder);
                    }
                    File.WriteAllText(innerJsonPath, innerJsonStr);
                }
                else
                {
                    innerJsonStr = File.ReadAllText(innerJsonPath);
                }
                Version innerVer = core.JsonToVersion(innerJsonStr);
                if (innerVer != null)
                {
                    tasks.AddRange(await GetLostDependDownloadTaskAsync(source, core, innerVer));
                }

            }
            foreach (var item in lostLibs)
            {
                tasks.Add(GetDownloadUrl.GetLibDownloadTask(source, item));
            }
            foreach (var item in lostNatives)
            {
                tasks.Add(GetDownloadUrl.GetNativeDownloadTask(source, item));
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
        public async static Task<List<DownloadTask>> GetLostAssetsDownloadTaskAsync(DownloadSource source, LaunchHandler core, Version ver)
        {
            List<DownloadTask> tasks = new List<DownloadTask>();
            JAssets assets = null;

            string assetsPath = core.GetAssetsIndexPath(ver.Assets);
            if (!File.Exists(assetsPath))
            {
                if (ver.AssetIndex != null)
                {
                    string jsonUrl = GetDownloadUrl.DoURLReplace(source, ver.AssetIndex.URL);
                    string assetsJson = await APIRequester.HttpGetStringAsync(jsonUrl);
                    assets = core.GetAssetsByJson(assetsJson);
                    tasks.Add(new DownloadTask("资源文件引导", jsonUrl, assetsPath));
                }
                else
                {
                    return tasks;
                }
            }
            else
            {
                assets = core.GetAssets(ver);
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
    }
}
