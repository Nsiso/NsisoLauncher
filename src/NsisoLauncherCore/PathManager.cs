using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.IO;

namespace NsisoLauncherCore
{
    /// <summary>
    /// 全局路径管理器
    /// </summary>
    public static class PathManager
    {
        /// <summary>
        /// 启动器名称（用作配置文件路径组成，请使用英文）
        /// </summary>
        public static string LauncherName { get; set; } = "NsisoLauncher";

        /// <summary>
        /// 启动器当前真实目录
        /// </summary>
        public static string CurrentLauncherDirectory { get => Environment.CurrentDirectory; }

        private static string _baseStorageDirectory;
        /// <summary>
        /// 基本的启动器仓库目录（存放配置文件，资源，缓存等。默认当前目录加启动器名称）
        /// </summary>
        public static string BaseStorageDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_baseStorageDirectory))
                {
                    _baseStorageDirectory = CurrentLauncherDirectory + '\\' + LauncherName;
                }
                return _baseStorageDirectory;
            }
            set
            {
                _baseStorageDirectory = value;
            }
        }

        public static string RuntimeDirectory { get => CurrentLauncherDirectory + "\\runtime"; }

        public static string ConfigDirectory { get => BaseStorageDirectory + "\\Config"; }

        public static string TempDirectory { get => BaseStorageDirectory + "\\temp"; }

        public static string ImageDirectory { get => BaseStorageDirectory + "\\Image"; }

        public static string MusicDirectory { get => BaseStorageDirectory + "\\Music"; }

        public static string VideoDirectory { get => BaseStorageDirectory + "\\Video"; }

        public static void InitLauncherDirectory()
        {
            if (!Directory.Exists(BaseStorageDirectory))
            {
                Directory.CreateDirectory(BaseStorageDirectory);
            }
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
            if (!Directory.Exists(ImageDirectory))
            {
                Directory.CreateDirectory(ImageDirectory);
            }
            if (!Directory.Exists(MusicDirectory))
            {
                Directory.CreateDirectory(MusicDirectory);
            }
            if (!Directory.Exists(VideoDirectory))
            {
                Directory.CreateDirectory(VideoDirectory);
            }
        }

        #region The path method
        /// <summary>
        /// Get the root path of all versions.
        /// </summary>
        /// <param name="gameRootPath">Game root path</param>
        /// <returns>root path of all version</returns>
        public static string GetVersionsRoot(string gameRootPath)
        {
            return string.Format(@"{0}\versions", gameRootPath);
        }

        /// <summary>
        /// Get specified version root by it's id.
        /// </summary>
        /// <param name="gameRootPath">Game root path</param>
        /// <param name="id">The version id string</param>
        /// <returns>This version's root path</returns>
        public static string GetVersionRoot(string gameRootPath, string id)
        {
            return string.Format(@"{0}\{1}", GetVersionsRoot(gameRootPath), id);
        }

        /// <summary>
        /// Get specified version root by it's instance.
        /// </summary>
        /// <param name="gameRootPath">Game root path</param>
        /// <param name="ver">The version instance</param>
        /// <returns>This version's root path</returns>
        public static string GetVersionRoot(string gameRootPath, VersionBase ver)
        {
            return GetVersionRoot(gameRootPath, ver.Id);
        }

        /// <summary>
        /// Get the specified version's index json file path.
        /// </summary>
        /// <param name="gameRootPath">Game root path</param>
        /// <param name="id">The version id string</param>
        /// <returns>This version's index json file path</returns>
        public static string GetVersionJsonPath(string gameRootPath, string id)
        {
            return string.Format(@"{0}\{1}.json", GetVersionRoot(gameRootPath, id), id);
        }

        /// <summary>
        /// Get the specified version's core jar file path by it's id.
        /// </summary>
        /// <param name="gameRootPath">Game root path</param>
        /// <param name="id">The version id string</param>
        /// <returns>The version's core jar file path</returns>
        public static string GetVersionJarPath(string gameRootPath, string id)
        {
            return string.Format(@"{0}\{1}.jar", GetVersionRoot(gameRootPath, id), id);
        }

        /// <summary>
        /// Get the specified version's core jar file path by it's instance.
        /// </summary>
        /// <param name="gameRootPath">Game root path</param>
        /// <param name="ver">The version's instance</param>
        /// <returns>The version's core jar file path</returns>
        public static string GetVersionJarPath(string gameRootPath, VersionBase ver)
        {
            return ver.Jar != null ? GetVersionJarPath(gameRootPath, ver.Jar) : GetVersionJarPath(gameRootPath, ver.Id);
        }

        /// <summary>
        /// 获取游戏特定版本工作目录
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="ver">版本</param>
        /// <returns>特定版本工作目录路径</returns>
        public static string GetVersionWorkspaceDir(bool versionIsolation, string gameRootPath, VersionBase ver)
        {
            return versionIsolation ? GetVersionRoot(gameRootPath, ver) : gameRootPath;
        }

        /// <summary>
        /// 获取库文件根目录
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <returns>库文件根目录路径</returns>
        public static string GetLibrariesRoot(string gameRootPath)
        {
            return string.Format(@"{0}\libraries", gameRootPath);
        }

        /// <summary>
        /// 获取Artifact（字符串形式）路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="artifactStr">obj</param>
        /// <returns>路径</returns>
        public static string GetLibraryPath(string gameRootPath, string artifactStr)
        {
            Artifact art = new Artifact(artifactStr);
            return Path.Combine(GetLibrariesRoot(gameRootPath), art.Path);
        }

        /// <summary>
        /// 获取Artifact路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="artifactStr">obj</param>
        /// <returns>路径</returns>
        public static string GetLibraryPath(string gameRootPath, Artifact artifact)
        {
            return Path.Combine(GetLibrariesRoot(gameRootPath), artifact.Path);
        }

        /// <summary>
        /// 获取Library路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="artifact">obj</param>
        /// <returns>路径</returns>
        public static string GetLibraryPath(string gameRootPath, Library lib)
        {
            return Path.Combine(GetLibrariesRoot(gameRootPath), lib.Path);
        }

        /// <summary>
        /// Get assets folder root path
        /// </summary>
        /// <param name="gameRootPath"></param>
        /// <returns></returns>
        public static string GetAssetsRoot(string gameRootPath)
        {
            return string.Format(@"{0}\assets", gameRootPath);
        }

        /// <summary>
        /// 获取资源引导文件路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="assetsID">版本ID</param>
        /// <returns></returns>
        public static string GetAssetsIndexPath(string gameRootPath, string assetsID)
        {
            return string.Format(@"{0}\indexes\{1}.json", GetAssetsRoot(gameRootPath), assetsID);
        }

        /// <summary>
        /// 获取资源文件路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="assetsInfo">资源文件Obj</param>
        /// <returns></returns>
        public static string GetAssetsPath(string gameRootPath, JAssetInfo assetsInfo)
        {
            return string.Format(@"{0}\objects\{1}\{2}", GetAssetsRoot(gameRootPath), assetsInfo.Hash.Substring(0, 2), assetsInfo.Hash);
        }

        /// <summary>
        /// 获取NIDE8核心路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <returns>NIDE8路径</returns>
        public static string GetNide8JarPath(string gameRootPath)
        {
            return string.Format(@"{0}\nide8auth.jar", gameRootPath);
        }

        /// <summary>
        /// 获取Authlib-injector核心路径
        /// </summary>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <returns>NIDE8路径</returns>
        public static string GetAIJarPath(string gameRootPath)
        {
            return string.Format(@"{0}\Authlib-injector.jar", gameRootPath);
        }

        /// <summary>
        /// 获取版本设置文件路径
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="version">版本</param>
        /// <returns>版本配置文件路径</returns>
        public static string GetVersionOptionsPath(bool versionIsolation, string gameRootPath, VersionBase version)
        {
            string verRoot = GetVersionWorkspaceDir(versionIsolation, gameRootPath, version);
            return verRoot + "\\options.txt";
        }

        /// <summary>
        /// 获取版本存档路径
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="version">版本</param>
        /// <returns>版本存档路径</returns>
        public static string GetVersionSavesDir(bool versionIsolation, string gameRootPath, VersionBase version)
        {
            string verRoot = GetVersionWorkspaceDir(versionIsolation, gameRootPath, version);
            return verRoot + "\\saves";
        }


        /// <summary>
        /// 获取版本模组路径
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="version">版本</param>
        /// <returns>版本模组文件夹路径</returns>
        public static string GetVersionModsDir(bool versionIsolation, string gameRootPath, VersionBase version)
        {
            string verRoot = GetVersionWorkspaceDir(versionIsolation, gameRootPath, version);
            return verRoot + "\\mods";
        }

        /// <summary>
        /// 获取版本存档路径
        /// </summary>
        /// <param name="versionIsolation">版本是否隔离</param>
        /// <param name="gameRootPath">游戏根目录</param>
        /// <param name="version">版本</param>
        /// <returns>版本mod文件夹路径</returns>
        public static string GetVersionResourcePacksDir(bool versionIsolation, string gameRootPath, VersionBase version)
        {
            string verRoot = GetVersionWorkspaceDir(versionIsolation, gameRootPath, version);
            return verRoot + "\\resourcepacks";
        }
        #endregion
    }
}