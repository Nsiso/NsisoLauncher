using Newtonsoft.Json;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Modules
{
    [JsonConverter(typeof(VersionBaseJsonConverter))]
    public abstract class VersionBase
    {
        /// <summary>
        /// 资源引导
        /// </summary>
        public AssetIndex AssetIndex { get; set; }

        /// <summary>
        /// 资源ID
        /// </summary>
        public string Assets { get; set; }

        /// <summary>
        /// Compliance等级
        /// </summary>
        public string ComplianceLevel { get; set; }

        /// <summary>
        /// 下载引导
        /// </summary>
        public CoreDownloads Downloads { get; set; }

        /// <summary>
        /// 版本ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 继承版本
        /// </summary>
        public string InheritsFrom { get; set; }

        /// <summary>
        /// 继承版本实例
        /// </summary>
        public VersionBase InheritsFromInstance { get; set; }

        /// <summary>
        /// 使用Java版本
        /// </summary>
        public JavaVersion JavaVersion { get; set; }

        /// <summary>
        /// 启动主类
        /// </summary>
        public string MainClass { get; set; }

        /// <summary>
        /// 指定JAR包
        /// </summary>
        public string Jar { get; set; }

        /// <summary>
        /// 库列表
        /// </summary>
        public List<Library> Libraries { get; set; }

        /// <summary>
        /// 启动器最低能启动版本号
        /// </summary>
        public int MinimumLauncherVersion { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime ReleaseTime { get; set; }

        /// <summary>
        /// 版本类型
        /// </summary>
        public string Type { get; set; }

        public virtual List<Library> GetAllLibraries()
        {
            HashSet<Library> lib_hash_set = new HashSet<Library>(new LibraryNameEqualityComparer());

            if (this.Libraries != null)
            {
                foreach (var item in this.Libraries)
                {
                    if (item.IsEnable())
                    {
                        lib_hash_set.Add(item);
                    }

                }
            }

            if (InheritsFromInstance != null)
            {
                List<Library> base_libs = InheritsFromInstance.GetAllLibraries();
                foreach (var item in base_libs)
                {
                    if (item.IsEnable())
                    {
                        lib_hash_set.Add(item);
                    }
                }
            }

            return lib_hash_set.ToList();
        }

        public abstract string GetJvmLaunchArguments();

        public abstract string GetGameLaunchArguments();
    }


    #region assetsIndex
    public class AssetIndex : Sha1SizeUrl
    {
        /// <summary>
        /// 资源ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 总大小
        /// </summary>
        public long TotalSize { get; set; }
    }
    #endregion

    #region coreDownloads
    public class CoreDownloads
    {
        /// <summary>
        /// 客户端下载信息
        /// </summary>
        [JsonProperty("client")]
        public Sha1SizeUrl Client { get; set; }

        /// <summary>
        /// 服务端下载信息
        /// </summary>
        [JsonProperty("server")]
        public Sha1SizeUrl Server { get; set; }

        /// <summary>
        /// mapping download info
        /// </summary>
        [JsonProperty("client_mappings")]
        public Sha1SizeUrl ClientMappings { get; set; }
    }
    #endregion

    #region Java
    public class JavaVersion
    {
        public string Component { get; set; }

        public int MajorVersion { get; set; }
    }
    #endregion


}
