using Microsoft.Win32;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Apis.Modules;
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

        public int MajorVersion
        {
            get
            {
                string[] vers = Version.Trim().Split('.', '_', '-', '+', 'u', 'U');
                if (vers[0] == "1")
                {
                    return int.Parse(vers[1]);
                }
                else
                {
                    return int.Parse(vers[0]);
                }
            }
        }

        /// <summary>
        /// Java类型（OracleJDK or OpenJDK...）
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// java位数
        /// </summary>
        public ArchEnum Arch { get; private set; }

        public string GameCoreTag { get; private set; }

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
                    string[] lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                    //first line contain version and type.
                    string[] firstL = lines[0].Split(' ');
                    string type = firstL[0];
                    string version = firstL[2].Trim('\"');
                    bool is64 = result.Contains("64-Bit");
                    ArchEnum arch = is64 ? ArchEnum.x64 : ArchEnum.x32;
                    Java info = new Java(javaPath, version, arch)
                    {
                        Type = type
                    };
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
        /// 从所给JAVA列表中寻找最适合该版本的JAVA
        /// </summary>
        /// <param name="javalist"></param>
        /// <returns></returns>
        public static Java GetSuitableJava(IEnumerable<Java> javalist, VersionBase version)
        {
            try
            {
                if (javalist == null || javalist.Count() == 0)
                {
                    return null;
                }

                ArchEnum arch = SystemTools.GetSystemArch();
                IOrderedEnumerable<Java> ordered_javas = null;
                switch (arch)
                {
                    case ArchEnum.x32:
                        ordered_javas = javalist.Where(x => x.Arch == ArchEnum.x32).OrderByDescending(x => x.Version);
                        break;
                    case ArchEnum.x64:
                        //ordered_javas = javalist.OrderByDescending(a => a.Version).ThenByDescending(x => x.Arch);
                        ordered_javas = javalist.OrderByDescending(x => x.Version).ThenByDescending(x => x.Arch);
                        break;
                    default:
                        ordered_javas = javalist.Where(x => x.Arch == ArchEnum.x32).OrderByDescending(x => x.Version);
                        break;
                }

                Modules.JavaVersion javaVersion = null;
                if (version.JavaVersion != null)
                {
                    javaVersion = version.JavaVersion;
                }
                else if (version.InheritsFromInstance != null && version.InheritsFromInstance.JavaVersion != null)
                {
                    javaVersion = version.InheritsFromInstance.JavaVersion;
                }

                if (javaVersion != null)
                {
                    //如果指定了java
                    Java com_java = ordered_javas.Where(x => x.GameCoreTag == javaVersion.Component).FirstOrDefault();
                    if (com_java != null)
                    {
                        return com_java;
                    }
                    Java second_com_java = ordered_javas.Where(x => x.MajorVersion == javaVersion.MajorVersion).FirstOrDefault();
                    if (second_com_java != null)
                    {
                        return second_com_java;
                    }
                }

                Java a_java = ordered_javas.Where(x => x.MajorVersion <= 8).FirstOrDefault();
                if (a_java != null)
                {
                    return a_java;
                }

                return ordered_javas.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }


            //try
            //{
            //    List<Java> goodjava = new List<Java>();
            //    if (SystemTools.GetSystemArch() == ArchEnum.x64)
            //    {
            //        foreach (var item in javalist)
            //        {
            //            if (item.Arch == ArchEnum.x64)
            //            { goodjava.Add(item); }
            //        }
            //        if (goodjava.Count == 0)
            //        { goodjava.AddRange(javalist); }
            //    }
            //    else
            //    { goodjava = javalist.ToList(); }

            //    return goodjava.OrderByDescending(a => a.Version).ToList().FirstOrDefault();
            //}
            //catch (Exception)
            //{ return null; }
        }

        private static Dictionary<string, string> GetJavaRegisterPath(RegistryKey key)
        {
            Dictionary<string, string> jres = new Dictionary<string, string>();

            var oldJreKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("Java Runtime Environment");
            var newJreKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("JRE");
            var oldJdkKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("Java Development Kit");
            var newJdkKey = key?.OpenSubKey("JavaSoft")?.OpenSubKey("JDK");

            //old Jre
            if (oldJreKey != null)
            {
                foreach (var verStr in oldJreKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string path = oldJreKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString().TrimEnd('\\') + @"\bin\javaw.exe";
                        if (File.Exists(path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, path);
                        }
                    }
                }
            }

            //new Jre
            if (newJreKey != null)
            {
                foreach (var verStr in newJreKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string path = newJreKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString().TrimEnd('\\') + @"\bin\javaw.exe";
                        if (File.Exists(path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, path);
                        }
                    }
                }
            }

            //old jdk
            if (oldJdkKey != null)
            {
                foreach (var verStr in oldJdkKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string jre_javaw_path = oldJdkKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString().TrimEnd('\\') + @"\jre\bin\javaw.exe";
                        string jdk_javaw_path = oldJdkKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString().TrimEnd('\\') + @"\bin\javaw.exe";
                        if (File.Exists(jre_javaw_path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, jre_javaw_path);

                        }
                        else if (File.Exists(jdk_javaw_path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, jdk_javaw_path);

                        }
                    }
                }
            }

            //new jdk
            if (newJdkKey != null)
            {
                foreach (var verStr in newJdkKey.GetSubKeyNames())
                {
                    if (verStr.Length > 3)
                    {
                        string jre_javaw_path = newJdkKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString().TrimEnd('\\') + @"\jre\bin\javaw.exe";
                        string jdk_javaw_path = newJdkKey.OpenSubKey(verStr).GetValue("JavaHome")?.ToString().TrimEnd('\\') + @"\bin\javaw.exe";
                        if (File.Exists(jre_javaw_path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, jre_javaw_path);

                        }
                        else if (File.Exists(jdk_javaw_path) && !jres.ContainsKey(verStr))
                        {
                            jres.Add(verStr, jdk_javaw_path);

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
            ArchEnum arch = SystemTools.GetSystemArch();
            switch (arch)
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

                default:
                    var jresDefault = GetJavaRegisterPath(localMachine);
                    javas.AddRange(jresDefault.Select(x => new Java(x.Value, x.Key, ArchEnum.x32)));
                    break;
            }
            return javas;
        }

        public static List<Java> GetRuntimeRootJavaList(string runtime_dir)
        {
            List<Java> javas = new List<Java>();
            if (!Directory.Exists(runtime_dir))
            {
                return javas;
            }
            List<Tuple<string, ArchEnum>> names = new List<Tuple<string, ArchEnum>>(2);
            OsType os = SystemTools.GetOsType();
            switch (os)
            {
                case OsType.Windows:
                    ArchEnum arch = SystemTools.GetSystemArch();
                    Tuple<string, ArchEnum> win86 = new Tuple<string, ArchEnum>("windows-x86", ArchEnum.x32);
                    Tuple<string, ArchEnum> win64 = new Tuple<string, ArchEnum>("windows-x64", ArchEnum.x64);
                    switch (arch)
                    {
                        case ArchEnum.x32:
                            names.Add(win86);
                            break;
                        case ArchEnum.x64:
                            names.Add(win86);
                            names.Add(win64);
                            break;
                        default:
                            names.Add(win86);
                            break;
                    }
                    break;
                case OsType.Linux:
                    names.Add(new Tuple<string, ArchEnum>("linux", ArchEnum.x64));
                    break;
                case OsType.MacOS:
                    names.Add(new Tuple<string, ArchEnum>("mac-os", ArchEnum.x64));
                    break;
                default:
                    break;
            }
            string[] java_dirs = Directory.GetDirectories(runtime_dir);
            foreach (var item_dir in java_dirs)
            {
                string java_tag_name = System.IO.Path.GetFileName(item_dir);
                foreach (var item_os in names)
                {
                    string base_dir = string.Format("{0}\\{1}", item_dir, item_os.Item1);
                    if (!Directory.Exists(base_dir))
                    {
                        continue;
                    }
                    string base_runtime_dir = string.Format("{0}\\{1}", base_dir, java_tag_name);
                    string javaw_path = base_runtime_dir + "\\bin\\javaw.exe";
                    if (!File.Exists(javaw_path))
                    {
                        continue;
                    }
                    string version_info_path = base_dir + "\\.version";
                    Java java;
                    if (File.Exists(version_info_path))
                    {
                        string version = File.ReadAllLines(version_info_path)[0];
                        java = new Java(javaw_path, version, item_os.Item2);
                    }
                    else
                    {
                        java = GetJavaInfo(javaw_path);
                    }
                    java.GameCoreTag = java_tag_name;
                    java.Type = "offical";
                    javas.Add(java);
                }
            }
            return javas;
        }
    }

    public enum JavaImageType
    {
        /// <summary>
        /// Java runtime environment
        /// </summary>
        JRE,

        /// <summary>
        /// java develop kit
        /// </summary>
        JDK
    }
}
