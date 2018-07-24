using NsisoLauncher.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncher.Core.Util
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
        public static List<VersionOption> GetOptions(LaunchHandler core, Modules.Version version)
        {
            if (version != null)
            {
                string optionsPath = core.GetVersionOptions(version);
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
                        throw new Exception("版本options.txt配置文件转换错误。请确认冒号分界线是否正确");
                    }
                    options.Add(new VersionOption() { Key = kv[0], Value = kv[1] });
                }
                return options;
            }
            else
            {
                return null;
            }
        }

        [DllImport("User32.dll")]
        public static extern int SetWindowText(IntPtr winHandle, string title);

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
                catch (Exception){}
            });
        }

        public static void SaveOptions(List<VersionOption> opts, LaunchHandler core, Modules.Version version)
        {
            if (version != null && opts != null)
            {
                List<string> optLines = new List<string>();
                foreach (var item in opts)
                {
                    optLines.Add(item.ToString());
                }
                File.WriteAllLines(core.GetVersionOptions(version), optLines.ToArray());
            }
        }
    }
}
