using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util
{
    /// <summary>
    /// 游戏设置OPTION类
    /// </summary>
    public class VersionOption
    {
        /// <summary>
        /// 设置项
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 设置值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 转换为MC设置文件格式string
        /// </summary>
        /// <returns>一条设置Line</returns>
        public override string ToString()
        {
            return Key.Trim() + ':' + Value.Trim();
        }
    }

    public static class GameHelper
    {
        public async static Task<List<VersionOption>> GetOptionsAsync(LaunchHandler core, Modules.Version version)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (version != null)
                {
                    try
                    {
                        string optionsPath = core.GetVersionOptionsPath(version);
                        if (!File.Exists(optionsPath))
                        {
                            return null;
                        }
                        string[] lines = File.ReadAllLines(optionsPath);
                        List<VersionOption> options = new List<VersionOption>();
                        foreach (var item in lines)
                        {
                            string[] kv = item.Split(':');
                            if (kv.Length != 2)
                            {
                                return null;
                            }
                            options.Add(new VersionOption() { Key = kv[0], Value = kv[1] });
                        }
                        return options;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
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
                catch (Exception) { }
            });
        }

        public async static Task SaveOptionsAsync(List<VersionOption> opts, LaunchHandler core, Modules.Version version)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    if (version != null && opts != null)
                    {
                        List<string> optLines = new List<string>();
                        foreach (var item in opts)
                        {
                            optLines.Add(item.ToString());
                        }
                        File.WriteAllLines(core.GetVersionOptionsPath(version), optLines.ToArray());
                    }
                }
                catch (Exception) { }
            });
        }
    }
}
