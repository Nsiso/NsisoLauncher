using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NsisoLauncher.Core
{
    public class LaunchHandler
    {
        /// <summary>
        /// 启动器所处理的游戏根目录
        /// </summary>
        public string GameRootPath { get; set; }

        /// <summary>
        /// 启动器所使用JAVA
        /// </summary>
        public Java Java { get; set; }

        /// <summary>
        /// 是否开启版本隔离
        /// </summary>
        public bool VersionIsolation { get; set; }

        private ArgumentsParser argumentsParser;
        private VersionReader versionReader;

        public LaunchHandler(string gamepath, Java java, bool isversionIsolation)
        {
            this.GameRootPath = gamepath;
            this.Java = java;
            this.VersionIsolation = isversionIsolation;
            versionReader = new VersionReader(new System.IO.DirectoryInfo(GameRootPath + @"\versions"));
            argumentsParser = new ArgumentsParser(this);
        }

        public LaunchResult Launch(LaunchSetting setting)
        {
            string arg = argumentsParser.Parse(setting);
            ProcessStartInfo startInfo = new ProcessStartInfo(Java.Path, arg)
            { RedirectStandardError = true, RedirectStandardOutput = true, UseShellExecute = false, WorkingDirectory = GameRootPath };
            var process = Process.Start(startInfo);
            return new LaunchResult() { Process = process, IsSuccess = true, LaunchArguments = arg };

        }

        public List<Modules.Version> GetVersions()
        {
            return versionReader.GetVersions();
        }

        public string GetGameVersionRootDir(Modules.Version ver)
        {
            if (VersionIsolation)
            {
                return string.Format(@"{0}\versions\{1}", GameRootPath, ver.ID);
            }
            else
            {
                return GameRootPath;
            }
            
        }

        public string GetLibraryPath(Modules.Library lib)
        {
            return String.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.jar",
                this.GameRootPath, lib.Package.Replace(".", @"\"), lib.Name, lib.Version);
        }

        public string GetNativePath(Native native)
        {
            return String.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}-{4}.jar",
                this.GameRootPath, native.Package.Replace(".", @"\"), native.Name, native.Version, native.NativeSuffix);
        }

        public string GetJarPath(Modules.Version ver)
        {
            return string.Format(@"{0}\versions\{1}\{1}.jar", this.GameRootPath, ver.ID);
        }
    }
}
