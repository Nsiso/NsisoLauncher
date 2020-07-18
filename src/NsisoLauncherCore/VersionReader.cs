using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncherCore
{
    public class VersionReader
    {
        private readonly DirectoryInfo dirInfo;
        private readonly LaunchHandler handler;
        private readonly object locker = new object();

        public VersionReader(LaunchHandler launchHandler)
        {
            dirInfo = new DirectoryInfo(launchHandler.GameRootPath + @"\versions");
            handler = launchHandler;
        }

        public event EventHandler<Log> VersionReaderLog;

        public Version JsonToVersion(JObject obj)
        {
            var ver = new Version();
            ver = obj.ToObject<Version>();
            JObject innerVer = null;
            if (ver.InheritsVersion != null)
            {
                SendDebugLog(string.Format("检测到\"{0}\"继承于\"{1}\"", ver.ID, ver.InheritsVersion));
                var innerJsonStr = GetVersionJsonText(ver.InheritsVersion);
                if (innerJsonStr != null) innerVer = JObject.Parse(innerJsonStr);
            }

            if (obj.ContainsKey("arguments"))
            {
                #region 处理新版本引导

                SendDebugLog(string.Format("检测到\"{0}\"使用新版本启动参数", ver.ID));

                #region 处理版本继承

                if (innerVer != null && innerVer.ContainsKey("arguments"))
                {
                    var innerVerArg = (JObject) innerVer["arguments"];
                    if (innerVerArg.ContainsKey("game"))
                        ver.MinecraftArguments += string.Format("{0} ", ParseGameArgFromJson(innerVerArg["game"]));
                    if (innerVerArg.ContainsKey("jvm"))
                        ver.JvmArguments += string.Format("{0} ", ParseJvmArgFromJson(innerVerArg["jvm"]));
                }

                #endregion

                var verArg = (JObject) obj["arguments"];

                #region 游戏参数

                if (verArg.ContainsKey("game"))
                {
                    var gameArg = verArg["game"];
                    ver.MinecraftArguments += ParseGameArgFromJson(gameArg);
                }

                #endregion

                #region JVM参数

                if (verArg.ContainsKey("jvm"))
                {
                    var jvmArg = verArg["jvm"];
                    ver.JvmArguments += ParseJvmArgFromJson(jvmArg);
                }

                #endregion
            }

            #endregion

            else
            {
                #region 旧版本添加默认JVM参数

                SendDebugLog(string.Format("检测到\"{0}\"使用旧版本启动参数", ver.ID));
                ver.JvmArguments = "-Djava.library.path=${natives_directory} -cp ${classpath}";

                #endregion
            }

            #region 处理依赖

            ver.Libraries = new List<Library>();
            ver.Natives = new List<Native>();
            var libToken = obj["libraries"];
            foreach (var lib in libToken)
            {
                var libObj = lib.ToObject<JLibrary>();

                if (CheckAllowed(libObj.Rules))
                {
                    if (libObj.Natives == null)
                    {
                        //不为native
                        var library = new Library(libObj.Name);
                        if (!string.IsNullOrWhiteSpace(libObj.Url)) library.Url = libObj.Url;
                        if (libObj.Downloads?.Artifact != null) library.LibDownloadInfo = libObj.Downloads.Artifact;
                        ver.Libraries.Add(library);
                    }
                    else
                    {
                        //为native
                        var native = new Native(libObj.Name,
                            libObj.Natives["windows"].Replace("${arch}",
                                SystemTools.GetSystemArch() == ArchEnum.x64 ? "64" : "32"));
                        if (libObj.Extract != null) native.Exclude = libObj.Extract.Exculde;
                        if (libObj.Downloads?.Artifact != null) native.LibDownloadInfo = libObj.Downloads.Artifact;
                        if (libObj.Downloads?.Classifiers?.Natives != null)
                            native.NativeDownloadInfo = libObj.Downloads.Classifiers.Natives;
                        ver.Natives.Add(native);
                    }
                }
            }

            #endregion

            #region 处理版本继承

            if (innerVer != null)
            {
                var iv = JsonToVersion(innerVer);
                if (iv != null)
                {
                    ver.Assets = iv.Assets;
                    ver.AssetIndex = iv.AssetIndex;
                    ver.Natives.AddRange(iv.Natives);
                    ver.Libraries.AddRange(iv.Libraries);
                    ver.Jar = iv.ID;
                }
            }

            #endregion

            return ver;
        }

        public Version JsonToVersion(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return null;
            var obj = JObject.Parse(jsonStr);
            return JsonToVersion(obj);
        }

        public Version GetVersion(string ID)
        {
            lock (locker)
            {
                try
                {
                    return JsonToVersion(GetVersionJsonText(ID));
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public string GetVersionJsonText(string id)
        {
            var jsonPath = handler.GetJsonPath(id);
            if (!File.Exists(jsonPath)) return null;
            return File.ReadAllText(jsonPath, Encoding.UTF8);
        }

        public async Task<Version> GetVersionAsync(string ID)
        {
            return await Task.Factory.StartNew(() => { return GetVersion(ID); });
        }

        public List<Version> GetVersions()
        {
            dirInfo.Refresh();
            if (!dirInfo.Exists) return new List<Version>();
            var dirs = dirInfo.EnumerateDirectories();
            var versions = new List<Version>();
            foreach (var item in dirs)
            {
                var ver = GetVersion(item.Name);
                if (ver != null) versions.Add(ver);
            }

            return versions;
        }

        public async Task<List<Version>> GetVersionsAsync()
        {
            return await Task.Factory.StartNew(() => { return GetVersions(); });
        }

        /// <summary>
        ///     判断规则
        /// </summary>
        /// <param name="rules">规则列表</param>
        /// <returns>是否启用</returns>
        private bool CheckAllowed(List<JRule> rules)
        {
            if (rules == null || rules.Count == 0) return true;
            var allowed = false;
            foreach (var rule in rules)
                if (rule.OS == null)
                    allowed = rule.Action == "allow";
                else if (rule.OS.Name == "windows") allowed = rule.Action == "allow";
            return allowed;
        }

        private void SendDebugLog(string str)
        {
            VersionReaderLog?.Invoke(this, new Log {LogLevel = LogLevel.DEBUG, Message = str});
        }

        #region ArgReader

        private string ParseGameArgFromJson(JToken gameArg)
        {
            var gameArgBuilder = new StringBuilder();
            foreach (var arg in gameArg)
                if (arg.Type == JTokenType.String)
                {
                    gameArgBuilder.AppendFormat("{0} ", arg);
                }
                else if (arg.Type == JTokenType.Object)
                {
                    var argObj = (JObject) arg;
                    if (argObj.ContainsKey("rules"))
                    {
                    }
                    else
                    {
                        var valueJtoken = argObj["value"];
                        if (valueJtoken.Type == JTokenType.String)
                            gameArgBuilder.AppendFormat("{0} ", valueJtoken);
                        else if (valueJtoken.Type == JTokenType.Array)
                            foreach (var item in valueJtoken)
                                gameArgBuilder.AppendFormat("{0} ", item);
                    }
                }

            return gameArgBuilder.ToString().Trim();
        }

        private string ParseJvmArgFromJson(JToken jvmArg)
        {
            var jvmArgBuilder = new StringBuilder();
            foreach (var arg in jvmArg)
                if (arg.Type == JTokenType.String)
                {
                    jvmArgBuilder.AppendFormat("{0} ", arg);
                }
                else if (arg.Type == JTokenType.Object)
                {
                    var argObj = (JObject) arg;
                    if (argObj.ContainsKey("rules"))
                    {
                        var rules = arg["rules"];

                        #region 规则参数处理

                        foreach (var rule in rules)
                            if (rule["action"].ToString() == "allow")
                            {
                                #region 判断是否合法

                                var OS = (JObject) rule["os"];
                                if (OS.ContainsKey("arch"))
                                {
                                    var arch = OS["arch"].ToString();
                                    switch (SystemTools.GetSystemArch())
                                    {
                                        case ArchEnum.x32:
                                            if (arch != "x86") continue;
                                            break;
                                        case ArchEnum.x64:
                                            if (arch != "x64") continue;
                                            break;
                                        default:
                                            continue;
                                    }
                                }

                                if (OS.ContainsKey("name"))
                                {
                                    if (OS["name"].ToString() == "windows")
                                    {
                                        if (OS.ContainsKey("version"))
                                            if (!Regex.Match(Environment.OSVersion.Version.ToString(),
                                                OS["version"].ToString()).Success)
                                                continue;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                #endregion

                                if (arg["value"].Type == JTokenType.String)
                                {
                                    var value = arg["value"].ToString();
                                    if (value.Contains(" "))
                                        jvmArgBuilder.AppendFormat("\"{0}\" ", value);
                                    else
                                        jvmArgBuilder.AppendFormat("{0} ", value);
                                }
                                else if (arg["value"].Type == JTokenType.Array)
                                {
                                    foreach (var str in arg["value"])
                                    {
                                        var value = str.ToString();
                                        if (value.Contains(" "))
                                            jvmArgBuilder.AppendFormat("\"{0}\" ", value);
                                        else
                                            jvmArgBuilder.AppendFormat("{0} ", value);
                                    }
                                }
                            }

                        #endregion
                    }
                    else
                    {
                        var valueJtoken = argObj["value"];
                        if (valueJtoken.Type == JTokenType.String)
                            jvmArgBuilder.AppendFormat("{0} ", valueJtoken);
                        else if (valueJtoken.Type == JTokenType.Array)
                            foreach (var item in valueJtoken)
                                jvmArgBuilder.AppendFormat("{0} ", item);
                    }
                }

            return jvmArgBuilder.ToString().Trim();
        }

        #endregion
    }

    public class JLibrary
    {
        /// <summary>
        ///     库名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Native列表
        /// </summary>
        [JsonProperty("natives")]
        public Dictionary<string, string> Natives { get; set; }

        /// <summary>
        ///     规则
        /// </summary>
        [JsonProperty("rules")]
        public List<JRule> Rules { get; set; }

        /// <summary>
        ///     解压声明
        /// </summary>
        [JsonProperty("extract")]
        public JExtract Extract { get; set; }

        /// <summary>
        ///     下载引导
        /// </summary>
        [JsonProperty("downloads")]
        public JDownloads Downloads { get; set; }

        /// <summary>
        ///     下载URL第一种表达
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class JRule
    {
        /// <summary>
        ///     action
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        ///     操作系统
        /// </summary>
        [JsonProperty("os")]
        public JOperatingSystem OS { get; set; }
    }

    public class JOperatingSystem
    {
        /// <summary>
        ///     系统名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class JExtract
    {
        /// <summary>
        ///     排除列表
        /// </summary>
        [JsonProperty("exclude")]
        public List<string> Exculde { get; set; }
    }

    public class JDownloads
    {
        /// <summary>
        ///     库文件
        /// </summary>
        [JsonProperty("artifact")]
        public PathSha1SizeUrl Artifact { get; set; }

        /// <summary>
        ///     本地组件文件
        /// </summary>
        [JsonProperty("classifiers")]
        public JClassifiers Classifiers { get; set; }
    }

    public class JClassifiers
    {
        /// <summary>
        ///     库文件
        /// </summary>
        [JsonProperty("natives-windows")]
        public PathSha1SizeUrl Natives { get; set; }
    }
}