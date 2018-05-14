using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NsisoLauncher.Core.Util
{
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
        /// 根据路径获取单个JAVA详细信息
        /// </summary>
        /// <param name="javaPath"></param>
        /// <returns></returns>
        public static Java GetJavaInfo(string javaPath)
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

        /// <summary> n
        /// 获取本机所有JAVA列表
        /// </summary>
        /// <returns></returns>
        public static List<Java> GetJavaList()
        {
            var javaPaths = GetJavaPathList();
            List<Java> javas = new List<Java>();
            foreach (string item in javaPaths)
            {
                javas.Add(GetJavaInfo(item));
            }
            return javas;
        }

        /// <summary>
        /// 从本机JAVA中获取最符合此电脑的JAVA版本
        /// </summary>
        /// <param name="javalist">JAVA详细信息集合</param>
        /// <returns>最佳JAVA详细信息</returns>
        public static Java GetSuitableJava()
        {
            return GetSuitableJava(GetJavaList());
        }

        public static Java GetSuitableJava(List<Java> javalist)
        {
            try
            {
                List<Java> goodjava = new List<Java>();
                if (SystemTools.GetSystemArch() == Util.ArchEnum.x64)
                {
                    foreach (var item in javalist)
                    {
                        if (item.Arch == ArchEnum.x64)
                        {
                            goodjava.Add(item);
                        }
                    }

                    if (goodjava.Count == 0)
                    {
                        goodjava.AddRange(javalist);
                    }
                }
                else
                {
                    goodjava = javalist;
                }
                goodjava = goodjava.OrderByDescending(a => a.Version).ToList();
                return goodjava.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 从注册表中查找可能的javaw.exe位置
        /// This code is from kmccc, thx
        /// </summary>
        /// <returns>JAVA地址列表</returns>
        public static IEnumerable<string> GetJavaPathList()
        {
            try
            {
                var rootReg = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                return rootReg == null
                    ? new string[0]
                    : FindJavaPathInternal(rootReg).Union(FindJavaPathInternal(rootReg.OpenSubKey("Wow6432Node"))).Where(x =>  File.Exists(x));
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// 内部注册表搜索方法
        /// This code is from kmccc, thx
        /// </summary>
        /// <param name="registry">注册表</param>
        /// <returns>JAVA可能路径</returns>
        private static IEnumerable<string> FindJavaPathInternal(RegistryKey registry)
        {
            try
            {
                var registryKey = registry.OpenSubKey("JavaSoft");
                if ((registryKey == null) || ((registry = registryKey.OpenSubKey("Java Runtime Environment")) == null)) return new string[0];
                return (from ver in registry.GetSubKeyNames()
                        select registry.OpenSubKey(ver)
                    into command
                        where command != null
                        select command.GetValue("JavaHome")
                    into javaHomes
                        where javaHomes != null
                        select javaHomes.ToString()
                    into str
                        where !String.IsNullOrWhiteSpace(str)
                        select str + @"\bin\javaw.exe");
            }
            catch
            {
                return new string[0];
            }
        }
    }
}
