using System.IO;

namespace NsisoLauncherCore.Modules
{
    public class ModInfo
    {
        /// <summary>
        /// Mod file name
        /// </summary>
        public string ModPath { get; set; }

        public string Name { get => Path.GetFileNameWithoutExtension(ModPath); }

        ///// <summary>
        ///// Mod ID
        ///// </summary>
        //[JsonProperty("modid")]
        //public string Id { get; set; }

        ///// <summary>
        ///// Mod名称
        ///// </summary>
        //public string Name { get; set; }

        ///// <summary>
        ///// mod描述
        ///// </summary>
        //public string Description { get; set; }

        ///// <summary>
        ///// mod版本
        ///// </summary>
        //public string Version { get; set; }

        ///// <summary>
        ///// mod对应mc版本
        ///// </summary>
        //[JsonProperty("mcversion")]
        //public string McVersion { get; set; }

        ///// <summary>
        ///// mod项目连接
        ///// </summary>
        //public string Url { get; set; }

        ///// <summary>
        ///// mod更新url
        ///// </summary>
        //public string UpdateUrl { get; set; }

        ///// <summary>
        ///// mod作者列表
        ///// </summary>
        //public List<string> Authors { get; set; }

        ///// <summary>
        ///// mod支持名单
        ///// </summary>
        //public string Credits { get; set; }

        ///// <summary>
        ///// logo文件
        ///// </summary>
        //public string LogoFile { get; set; }
    }
}
