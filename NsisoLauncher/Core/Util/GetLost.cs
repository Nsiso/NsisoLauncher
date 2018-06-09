using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Net;
using NsisoLauncher.Core.Net.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        /// 返回全部丢失的文件路径
        /// </summary>
        /// <param name="core">所使用的核心</param>
        /// <param name="version">要检查的版本</param>
        /// <returns>丢失文件的路径列表</returns>
        public static List<string> GetAllLostDepend(LaunchHandler core, Modules.Version version)
        {
            List<string> lost = new List<string>();
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
        public static List<DownloadTask> GetLostDependDownloadTask(DownloadSource source, LaunchHandler core, Modules.Version version)
        {
            var lostLibs = GetLostLibs(core, version);
            var lostNatives = GetLostNatives(core, version);
            List<DownloadTask> tasks = new List<DownloadTask>();
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
    }
}
