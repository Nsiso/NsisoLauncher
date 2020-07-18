using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NsisoLauncherCore.Modules;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncherCore.Util
{
    /// <summary>
    ///     游戏设置OPTION类
    /// </summary>
    public class VersionOption
    {
        /// <summary>
        ///     设置项
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     设置值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     转换为MC设置文件格式string
        /// </summary>
        /// <returns>一条设置Line</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", Key.Trim(), Value.Trim());
        }
    }

    public static class GameHelper
    {
        public static async Task<List<VersionOption>> GetOptionsAsync(LaunchHandler core, Version version)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (version != null)
                    try
                    {
                        var optionsPath = core.GetVersionOptionsPath(version);
                        if (!File.Exists(optionsPath)) return null;
                        var lines = File.ReadAllLines(optionsPath);
                        var options = new List<VersionOption>();
                        foreach (var item in lines)
                        {
                            var kv = item.Split(':');
                            options.Add(new VersionOption {Key = kv[0], Value = kv[1]});
                        }

                        return options;
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                return null;
            });
        }

        [DllImport("User32.dll")]
        private static extern int SetWindowText(IntPtr winHandle, string title);

        public static void SetGameTitle(LaunchResult result, string title)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var handle = result.Process.MainWindowHandle;
                    while (!result.Process.HasExited)
                    {
                        SetWindowText(handle, title);
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        public static async Task SaveOptionsAsync(List<VersionOption> opts, LaunchHandler core, Version version)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    if (version != null && opts != null)
                    {
                        var optLines = new List<string>();
                        foreach (var item in opts) optLines.Add(item.ToString());
                        File.WriteAllLines(core.GetVersionOptionsPath(version), optLines.ToArray());
                    }
                }
                catch (Exception)
                {
                }
            });
        }
    }
}