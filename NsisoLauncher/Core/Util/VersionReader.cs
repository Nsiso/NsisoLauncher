using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NsisoLauncher.Core.Util
{
    public class VersionReader
    {
        DirectoryInfo dirInfo;
        public VersionReader(DirectoryInfo directory)
        {
            dirInfo = directory;

        }

        public List<Modules.Version> GetVersions()
        {
            var dirs = dirInfo.EnumerateDirectories();
            List<Modules.Version> versions = new List<Modules.Version>();
            foreach (var item in dirs)
            {
                var jsonFiles = item.GetFiles(item.Name + ".json");
                if (jsonFiles.Length != 0)
                {
                    string jsonPath = jsonFiles.First().FullName;
                    string jsonStr = File.ReadAllText(jsonPath, Encoding.UTF8);
                    var obj = JsonConvert.DeserializeObject<JObject>(jsonStr);
                    Modules.Version ver = new Modules.Version();
                    ver = obj.ToObject<Modules.Version>();
                    if (obj.ContainsKey("arguments"))
                    {
                        #region 处理新版本引导

                        JToken gameArg = obj["arguments"]["game"];
                        StringBuilder gameArgBuilder = new StringBuilder();
                        foreach (var arg in gameArg)
                        {
                            if (arg.Type == JTokenType.String)
                            {
                                gameArgBuilder.AppendFormat("{0} ", arg.ToString());
                            }
                        }
                        ver.MinecraftArguments = gameArgBuilder.ToString();

                        JToken jvmArg = obj["arguments"]["jvm"];
                        StringBuilder jvmArgBuilder = new StringBuilder();
                        foreach (var arg in jvmArg)
                        {
                            if (arg.Type == JTokenType.String)
                            {
                                jvmArgBuilder.AppendFormat("{0} ", arg.ToString());
                            }
                            else if (arg.Type == JTokenType.Object)
                            {
                                JToken rules = arg["rules"];
                                foreach (var rule in rules)
                                {
                                    if (rule["action"].ToString() == "allow")
                                    {
                                        if (rule["os"]["name"].ToString() == "windows")
                                        {
                                            if (rule["os"].Contains("version"))
                                            {
                                                if (Regex.Match(Environment.OSVersion.VersionString, rule["os"]["version"].ToString()).Success)
                                                {
                                                    if (arg["value"].Type == JTokenType.String)
                                                    {
                                                        jvmArgBuilder.AppendFormat("{0} ", arg["value"].ToString());
                                                    }
                                                    else if (arg["value"].Type == JTokenType.Array)
                                                    {
                                                        foreach (var str in arg["value"])
                                                        {
                                                            jvmArgBuilder.AppendFormat("{0} ", str);
                                                        }
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                if (arg["value"].Type == JTokenType.String)
                                                {
                                                    jvmArgBuilder.AppendFormat("{0} ", arg["value"].ToString());
                                                }
                                                else if (arg["value"].Type == JTokenType.Array)
                                                {
                                                    foreach (var str in arg["value"])
                                                    {
                                                        jvmArgBuilder.AppendFormat("{0} ", str);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ver.JvmArguments = jvmArgBuilder.ToString();
                        #endregion
                    }
                    else
                    {
                        ver.JvmArguments = "-Djava.library.path=${natives_directory} -cp ${classpath}";
                    }

                    #region 处理库文件
                    ver.Libraries = new List<Modules.Library>();
                    ver.Natives = new List<Modules.Native>();
                    var libToken = obj["libraries"];
                    foreach (JToken lib in libToken)
                    {
                        var libObj = lib.ToObject<Library>();
                        var parts = libObj.Name.Split(':');
                        if (libObj.Natives == null)
                        {
                            if (CheckAllowed(libObj.Rules))
                            {
                                ver.Libraries.Add(new Modules.Library()
                                {
                                    Package = parts[0],
                                    Name = parts[1],
                                    Version = parts[2]
                                });
                            }
                        }
                        else
                        {
                            if (CheckAllowed(libObj.Rules))
                            {
                                var native = new Modules.Native
                                {
                                    Package = parts[0],
                                    Name = parts[1],
                                    Version = parts[2],
                                    NativeSuffix = libObj.Natives["windows"].Replace("${arch}", SystemTools.GetSystemArch() == ArchEnum.x64 ? "64" : "32")
                                };
                                ver.Natives.Add(native);
                                if (libObj.Extract != null)
                                {
                                    //native.Options = new UnzipOptions { Exclude = lib.Extract.Exculde };
                                }
                            }
                        }
                    }
                    #endregion

                    versions.Add(ver);
                }
            }
            return versions;
        }

        /// <summary>
		/// 判断规则
		/// </summary>
		/// <param name="rules">规则列表</param>
		/// <returns>是否启用</returns>
		public bool CheckAllowed(List<Rule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                return true;
            }
            var allowed = false;
            foreach (var rule in rules)
            {
                if (rule.OS == null)
                {
                    allowed = rule.Action == "allow";
                }
                else if (rule.OS.Name == "windows")
                {
                    allowed = rule.Action == "allow";
                }
            }
            return allowed;
        }
    }

    public class Library
    {
        /// <summary>
        /// 库名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Native列表
        /// </summary>
        [JsonProperty("natives")]
        public Dictionary<string, string> Natives { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        [JsonProperty("rules")]
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// 解压声明
        /// </summary>
        [JsonProperty("extract")]
        public Extract Extract { get; set; }
    }

    public class Rule
    {
        /// <summary>
        /// action
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        [JsonProperty("os")]
        public OperatingSystem OS { get; set; }
    }

    public class OperatingSystem
    {
        /// <summary>
        /// 系统名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Extract
    {
        /// <summary>
        /// 排除列表
        /// </summary>
        [JsonProperty("exclude")]
        public List<string> Exculde { get; set; }
    }
}
