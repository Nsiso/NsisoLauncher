using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.LaunchException
{
    public class LibraryNotFoundException : LaunchException
    {
        /// <summary>
        /// 丢失的库文件
        /// </summary>
        public Library LostLibrary { get; private set; }

        /// <summary>
        /// The lost jar file path.
        /// </summary>
        public string LostPath { get; set; }

        public LibraryNotFoundException(Library lost, string path) : base("缺失Library文件", string.Format("无法找到指定的Librarty文件{0},游戏是否完整?路径:{1}", lost.Name.Name, path))
        {
            this.LostLibrary = lost;
            this.LostPath = path;
        }
    }
}
