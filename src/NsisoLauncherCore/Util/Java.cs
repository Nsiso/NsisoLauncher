using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util
{
    public class InstallJavaOptions
    {
        /// <summary>
        /// 是否静默安装
        /// </summary>
        public bool SilentInstall { get; set; }

        /// <summary>
        /// 安装目录
        /// </summary>
        public string InstallDir { get; set; }

        public string ToArg()
        {
            StringBuilder str = new StringBuilder();
            if (SilentInstall)
            {
                str.Append("/s INSTALL_SILENT=1 ");
                str.Append("/L setup.log ");
            }
            if (!string.IsNullOrWhiteSpace(InstallDir))
            {
                str.AppendFormat("INSTALLDIR={0} ", InstallDir);
            }
            return str.ToString().Trim();
        }
    }

    public class Java
    {
        /// <summary>
        /// JAVA路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// JAVA版本
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// java位数
        /// </summary>
        public ArchEnum Arch { get; private set; }

        public Java(string path, string version, ArchEnum arch)
        {
            this.Path = path;
            this.Version = version;
            this.Arch = arch;
        }

        /// <summary>
        /// 安装JAVA
        /// </summary>
        /// <param name="installerPath">JAVA安装包路径</param>
        /// <param name="options">JAVA安装选项</param>
        /// <returns></returns>
        public static Process InstallJava(string installerPath, InstallJavaOptions options)
        {
            Process p = Process.Start(
                new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = string.Format("{0} {1}", installerPath, options.ToArg()),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    Verb = "RunAs"
                });
            return p;
        }

        /// <summary>
        /// 根据路径获取单个JAVA详细信息
        /// </summary>
        /// <param name="javaPath"></param>
        /// <returns></returns>
        public static Java GetJavaInfo(string javaPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(javaPath) && File.Exists(javaPath))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = javaPath;
                    p.StartInfo.Arguments = "-version";
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    string result = p.StandardError.ReadToEnd();
                    string version = result.Replace("java version \"", "");
                    version = version.Remove(version.IndexOf("\""));
                    bool is64 = result.Contains("64-Bit");
                    Java info;
                    if (is64) { info = new Java(javaPath, version, ArchEnum.x64); } else { info = new Java(javaPath, version, ArchEnum.x32); }
                    p.Dispose();
                    return info;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据路径获取单个JAVA详细信息
        /// </summary>
        /// <param name="javaPath"></param>
        /// <returns></returns>
        public static Task<Java> GetJavaInfoAsync(string javaPath)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetJavaInfo(javaPath);
            });
        }

        /// <summary>
        /// 从所给JAVA列表中寻找最适合本机的JAVA
        /// </summary>
        /// <param name="javalist"></param>
        /// <returns></returns>
        public static Java GetSuitableJava(List<Java> javalist)
        {
            try
            {
                List<Java> goodjava = new List<Java>();
                if (SystemTools.GetSystemArch() == ArchEnum.x64)
                {
                    foreach (var item in javalist)
                    {
                        if (item.Arch == ArchEnum.x64)
                        { goodjava.Add(item); }
                    }
                    if (goodjava.Count == 0)
                    { goodjava.AddRange(javalist); }
                }
                else
                { goodjava = javalist; }

                var java8 = goodjava.Where((x) =>
                {
                    return x.Version.StartsWith("1.8");
                });
                if (java8.Count() != 0)
                {
                    return java8.OrderByDescending(x => x.Version).ToList().FirstOrDefault();
                }
                else
                {
                    return goodjava.OrderByDescending(a => a.Version).ToList().FirstOrDefault();
                }
            }
            catch (Exception)
            { return null; }
        }

        public static Java GetSuitableJava()
        {
            return GetSuitableJava(GetJavaList());
        }

        public static Dictionary<string, string> GetJavaRegisterPath(RegistryKey key)
        {
            Dictionary<string, string> jres = new Dictionary<string, string>();

            var oldKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("Java Runtime Environment");
            var newKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("JRE");
            var jdkKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("Java Development Kit");

            //oldJre
            if (oldKey != null)
            {
                foreach (var verStr in oldKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string path = oldKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString() + @"\bin\javaw.exe";
                        if (File.Exists(path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, path);
                        }
                    }
                }
            }

            //newJre
            if (newKey != null)
            {
                foreach (var verStr in newKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string path = newKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString() + @"\bin\javaw.exe";
                        if (File.Exists(path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, path);
                        }
                    }
                }
            }

            //jdk
            if (jdkKey != null)
            {
                foreach (var verStr in jdkKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string path = jdkKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString() + @"\jre\bin\javaw.exe";
                        if (File.Exists(path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, path);
                        }
                    }
                }
            }

            return jres;
        }

        /// <summary>
        /// 从注册表寻找本机JAVA列表
        /// </summary>
        /// <returns></returns>
        public static List<Java> GetJavaList()
        {
            List<Java> javas = new List<Java>();
            RegistryKey localMachine = Registry.LocalMachine.OpenSubKey("SOFTWARE");
            switch (SystemTools.GetSystemArch())
            {
                case ArchEnum.x32:
                    var jres = GetJavaRegisterPath(localMachine);
                    javas.AddRange(jres.Select(x => new Java(x.Value, x.Key, ArchEnum.x32)));
                    break;

                case ArchEnum.x64:
                    var jres64 = GetJavaRegisterPath(localMachine);
                    javas.AddRange(jres64.Select(x => new Java(x.Value, x.Key, ArchEnum.x64)));
                    var jres32 = GetJavaRegisterPath(localMachine.OpenSubKey("Wow6432Node"));
                    javas.AddRange(jres32.Select(x => new Java(x.Value, x.Key, ArchEnum.x32)));
                    break;
            }
            return javas;
        }

        ///// <summary>
        ///// 从注册表中查找可能的javaw.exe位置
        ///// This code is from kmccc, thx
        ///// </summary>
        ///// <returns>JAVA地址列表</returns>
        //public static IEnumerable<string> GetJavaPathList()
        //{
        //    try
        //    {
        //        var rootReg = Registry.LocalMachine.OpenSubKey("SOFTWARE");
        //        return rootReg == null
        //            ? new string[0]
        //            : FindJavaPathInternal(rootReg).Union(FindJavaPathInternal(rootReg.OpenSubKey("Wow6432Node"))).Where(x => File.Exists(x));
        //    }
        //    catch
        //    {
        //        return new string[0];
        //    }
        //}

        ///// <summary>
        ///// 内部注册表搜索方法
        ///// This code is from kmccc, thx
        ///// </summary>
        ///// <param name="registry">注册表</param>
        ///// <returns>JAVA可能路径</returns>
        //private static IEnumerable<string> FindJavaPathInternal(RegistryKey registry)
        //{
        //    try
        //    {
        //        var registryKey = registry.OpenSubKey("JavaSoft");
        //        if ((registryKey == null) || ((registry = registryKey.OpenSubKey("Java Runtime Environment")) == null)) return new string[0];
        //        return (from ver in registry.GetSubKeyNames()
        //                select registry.OpenSubKey(ver)
        //            into command
        //                where command != null
        //                select command.GetValue("JavaHome")
        //            into javaHomes
        //                where javaHomes != null
        //                select javaHomes.ToString()
        //            into str
        //                where !String.IsNullOrWhiteSpace(str)
        //                select str + @"\bin\javaw.exe");
        //    }
        //    catch
        //    {
        //        return new string[0];
        //    }
        //}
    }
}
