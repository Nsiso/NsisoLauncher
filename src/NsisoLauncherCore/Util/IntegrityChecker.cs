using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Util
{
    public class IntegrityChecker
    {
        /// <summary>
        /// 启动核心处理器
        /// </summary>
        public LaunchHandler Handler { get; set; }

        /// <summary>
        /// 获取版本丢失的库文件
        /// </summary>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为库实例的集合</returns>
        public Dictionary<string, Library> GetLostLibs(VersionBase version)
        {
            Dictionary<string, Library> lostLibs = new Dictionary<string, Library>();

            foreach (var item in version.Libraries)
            {
                string path = this.Handler.GetLibraryPath(item);
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
        /// 获取版本丢失的资源文件
        /// </summary>
        /// <param name="version">要检查的版本</param>
        /// <returns>返回Key为路径，value为资源文件信息实例的集合</returns>
        public Dictionary<string, JAssetInfo> GetLostAssets(JAssets assets)
        {
            Dictionary<string, JAssetInfo> lostAssets = new Dictionary<string, JAssetInfo>();
            if (assets == null)
            {
                return lostAssets;
            }
            foreach (var item in assets.Objects)
            {
                string path = this.Handler.GetAssetsPath(item.Value);
                if ((!lostAssets.ContainsKey(path)) && (!File.Exists(path)))
                {
                    lostAssets.Add(path, item.Value);
                }
            }
            return lostAssets;
        }

        ///// <summary>
        ///// 获取版本丢失的库文件
        ///// </summary>
        ///// <param name="version">要检查的版本</param>
        ///// <returns>返回Key为路径，value为库实例的集合</returns>
        //public Dictionary<string, Library> GetWrongLibs(Version version)
        //{
        //    Dictionary<string, Library> wrongLibs = new Dictionary<string, Library>();

        //    foreach (var item in version.Libraries)
        //    {
        //        string path = this.Handler.GetLibraryPath(item);
        //        if (wrongLibs.ContainsKey(path))
        //        {
        //            continue;
        //        }
        //        else if (!File.Exists(path))
        //        {
        //            wrongLibs.Add(path, item);
        //        }
        //        else
        //        {
        //            if (item.LibDownloadInfo == null || string.IsNullOrEmpty(item.LibDownloadInfo.Sha1))
        //            {
        //                continue;
        //            }
        //            Checker.SHA1Checker checker = new Checker.SHA1Checker()
        //            { FilePath = path, CheckSum = item.LibDownloadInfo?.Sha1 };
        //            if (!checker.CheckFilePass())
        //            {
        //                wrongLibs.Add(path, item);
        //            }
        //        }
        //    }
        //    return wrongLibs;
        //}

        ///// <summary>
        ///// 获取版本丢失的natives文件
        ///// </summary>
        ///// <param name="version">要检查的版本</param>
        ///// <returns>返回Key为路径，value为native实例的集合</returns>
        //public Dictionary<string, Native> GetWrongNatives(Version version)
        //{
        //    Dictionary<string, Native> lostNatives = new Dictionary<string, Native>();

        //    foreach (var item in version.Natives)
        //    {
        //        string native_path = this.Handler.GetNativePath(item);
        //        string lib_path = this.Handler.GetLibraryPath(item);
        //        if (lostNatives.ContainsKey(native_path) || lostNatives.ContainsKey(lib_path))
        //        {
        //            continue;
        //        }
        //        else if (!File.Exists(path))
        //        {
        //            lostNatives.Add(path, item);
        //        }
        //        else
        //        {
        //            if (item.NativeDownloadInfo == null || string.IsNullOrEmpty(item.NativeDownloadInfo.Sha1))
        //            {
        //                continue;
        //            }
        //            Checker.SHA1Checker checker_lib = new Checker.SHA1Checker()
        //            { FilePath = path, CheckSum = item.NativeDownloadInfo?.Sha1 };
        //            Checker.SHA1Checker checker_native = new Checker.SHA1Checker()
        //            { FilePath = path, CheckSum = item.NativeDownloadInfo?.Sha1 };
        //            if (!checker.CheckFilePass())
        //            {
        //                lostNatives.Add(path, item);
        //            }
        //        }
        //    }
        //    return lostNatives;
        //}

        ///// <summary>
        ///// 获取版本丢失的资源文件
        ///// </summary>
        ///// <param name="version">要检查的版本</param>
        ///// <returns>返回Key为路径，value为资源文件信息实例的集合</returns>
        //public Dictionary<string, JAssetInfo> GetWrongAssets(JAssets assets)
        //{
        //    Dictionary<string, JAssetInfo> lostAssets = new Dictionary<string, JAssetInfo>();
        //    if (assets == null)
        //    {
        //        return lostAssets;
        //    }
        //    foreach (var item in assets.Objects)
        //    {
        //        string path = this.Handler.GetAssetsPath(item.Value);
        //        if ((!lostAssets.ContainsKey(path)) && (!File.Exists(path)))
        //        {
        //            lostAssets.Add(path, item.Value);
        //        }
        //    }
        //    return lostAssets;
        //}
    }
}
