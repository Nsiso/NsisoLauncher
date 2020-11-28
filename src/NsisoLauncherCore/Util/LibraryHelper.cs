using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Util
{
    public static class LibraryHelper
    {
        /// <summary>
        /// 检查是否可在本地使用
        /// </summary>
        /// <param name="lib">库文件</param>
        /// <returns>是否使用</returns>
        public static bool CheckAllowed(Library lib)
        {
            if (lib.Rules == null)
            {
                return true;
            }
            else
            {
                return RuleChecker.CheckAllowed(lib.Rules);
            }
        }

        /// <summary>
        /// 是否有游戏内jar依赖
        /// </summary>
        /// <param name="lib">库文件</param>
        /// <returns>是否有</returns>
        public static bool HaveArtifact(Library lib)
        {
            if (lib.Downloads == null)
            {
                return true;
            }
            else
            {
                return lib.Downloads.Artifact != null;
            }
        }

        /// <summary>
        /// 是否有本地依赖
        /// </summary>
        /// <param name="lib">库文件</param>
        /// <returns>是否有</returns>
        public static bool HaveNative(Library lib)
        {
            string key = GetNativeKey(lib);
            return !string.IsNullOrEmpty(key);
        }

        /// <summary>
        /// 获得本机库Key
        /// </summary>
        /// <param name="lib">库文件</param>
        /// <returns>本机库文件key</returns>
        public static string GetNativeKey(Library lib)
        {
            if (lib.Natives == null)
            {
                return null;
            }
            OperatingSystemEnum os = SystemTools.GetOperatingSystem();
            string key;
            switch (os)
            {
                case OperatingSystemEnum.Windows:
                    key = lib.Natives.Windows;
                    break;
                case OperatingSystemEnum.Osx:
                    key = lib.Natives.Osx;
                    break;
                case OperatingSystemEnum.Linux:
                    key = lib.Natives.Linux;
                    break;
                default:
                    return null;
            }

            return key.Replace("${arch}", SystemTools.GetSystemArch() == ArchEnum.x64 ? "64" : "32");
        }

        /// <summary>
        /// 获得游戏依赖jar下载信息
        /// </summary>
        /// <param name="lib">库文件</param>
        /// <returns>依赖jar下载信息</returns>
        public static PathSha1SizeUrl GetArtifactDownloadInfo(Library lib)
        {
            return lib.Downloads?.Artifact;
        }

        /// <summary>
        /// 获得本机库下载信息
        /// </summary>
        /// <param name="lib">库文件</param>
        /// <returns>本机库下载信息</returns>
        public static PathSha1SizeUrl GetNativeDownloadInfo(Library lib)
        {
            string nativeKey = GetNativeKey(lib);
            if (lib.Downloads.Classifiers != null && lib.Downloads.Classifiers.ContainsKey(nativeKey))
            {
                return lib.Downloads.Classifiers[nativeKey];
            }
            else
            {
                return null;
            }
        }

        public static bool CheckSupportCurrentOs(Library lib)
        {
            return RuleChecker.CheckAllowed(lib.Rules);
        }

        public static string GetArtifactBasePath(Library lib)
        {
            PathSha1SizeUrl info = GetArtifactDownloadInfo(lib);
            if ((info != null) && (!string.IsNullOrWhiteSpace(info.Path)))
            {
                return info.Path;
            }
            else
            {
                return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Artifact.Package.Replace(".", "\\"), lib.Artifact.Name, lib.Artifact.Version);
            }
        }

        public static string GetNativeBasePath(Library lib)
        {
            PathSha1SizeUrl info = GetNativeDownloadInfo(lib);
            if ((info != null) && (!string.IsNullOrWhiteSpace(info.Path)))
            {
                return info.Path;
            }
            else
            {
                return string.Format(@"{0}\{1}\{2}\{1}-{2}.jar", lib.Artifact.Package.Replace(".", "\\"), lib.Artifact.Name, lib.Artifact.Version);
            }
        }
    }
}
