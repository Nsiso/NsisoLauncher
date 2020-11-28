using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.LaunchException
{
    public class LibraryNotFoundException
    {
        /// <summary>
        /// 丢失的库文件
        /// </summary>
        public Library LostLibrary { get; private set; }

        /// <summary>
        /// 是否丢失本机库
        /// </summary>
        public bool IsLostNative { get; private set; }

        /// <summary>
        /// 是否丢失游戏jar依赖
        /// </summary>
        public bool IsLostArtifact { get; private set; }

        /// <summary>
        /// 内部错误
        /// </summary>
        public Exception InnerException { get; private set; }

        public LibraryNotFoundException(Library lost, bool isLostNative, bool isLostArtifact)
        {
            this.LostLibrary = lost;
            this.IsLostArtifact = isLostArtifact;
            this.IsLostNative = isLostNative;
        }
    }
}
