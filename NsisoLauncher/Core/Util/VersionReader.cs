using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

                    #region 处理新版本引导
                    if (obj.ContainsKey("arguments"))
                    {
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
                                                    else if(arg["value"].Type == JTokenType.Array)
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
                    }
                    else
                    {
                        ver.JvmArguments = "-Djava.library.path=${natives_directory} -cp ${classpath}";
                    }
                    #endregion

                    versions.Add(ver);
                }
            }
            return versions;
        }
    }
}
