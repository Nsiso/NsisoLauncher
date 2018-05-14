using NsisoLauncher.Core.Util;
using System;
using System.Collections.Generic;
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

        private VersionReader versionReader;

        public LaunchHandler(string gamepath, Java java)
        {
            this.GameRootPath = gamepath;
            this.Java = java;
            versionReader = new VersionReader(new System.IO.DirectoryInfo(GameRootPath + @"\versions"));
        }

        public List<Modules.Version> GetVersions()
        {
            return versionReader.GetVersions();
        }
    }
}
