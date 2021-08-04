using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NsisoLauncherCore
{
    public class VersionReader
    {
        public JsonSerializer JsonSerializer { get; set; } = new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public event EventHandler<Log> VersionReaderLog;

        private DirectoryInfo dirInfo;
        private LaunchHandler handler;
        private object locker = new object();
        public VersionReader(LaunchHandler launchHandler)
        {
            dirInfo = new DirectoryInfo(launchHandler.GameRootPath + @"\versions");
            handler = launchHandler;
        }

        //public VersionBaseJsonToVersion(JObject obj)
        //{
        //    VersionBasever = new Modules.Version();
        //    //JObject innerVer = null;
        //    //if (ver.InheritsFrom != null)
        //    //{
        //    //    SendDebugLog(string.Format("检测到\"{0}\"继承于\"{1}\"", ver.Id, ver.InheritsFrom));
        //    //    string innerJsonStr = GetVersionJsonText(ver.InheritsFrom);
        //    //    if (innerJsonStr != null)
        //    //    {
        //    //        innerVer = JObject.Parse(innerJsonStr);
        //    //    }
        //    //}

        //    if (obj.ContainsKey("arguments"))
        //    {
        //        #region 处理新版本引导

        //        SendDebugLog(string.Format("检测到\"{0}\"使用新版本启动参数", ver.Id));

        //        VersionV2 v2 = obj.ToObject<VersionV2>();

        //        #region JVM参数
        //        ////版本继承
        //        //if (innerVer != null && innerVer.ContainsKey("arguments"))
        //        //{
        //        //    JObject innerVerArg = (JObject)innerVer["arguments"];
        //        //    if (innerVerArg.ContainsKey("jvm"))
        //        //    {
        //        //        ver.JvmArguments += string.Format("{0} ", ParseJvmArgFromJson(innerVerArg["jvm"]));
        //        //    }
        //        //}
        //        if (v2.Arguments.Jvm != null)
        //        {
        //            ver.JvmArguments += string.Format("{0} ", ParseJvmArgFromJson(v2.Arguments.Jvm));
        //        }
        //        #endregion

        //        #region 游戏参数
        //        ////版本继承
        //        //if (innerVer != null && innerVer.ContainsKey("arguments"))
        //        //{
        //        //    JObject innerVerArg = (JObject)innerVer["arguments"];
        //        //    if (innerVerArg.ContainsKey("game"))
        //        //    {
        //        //        ver.MinecraftArguments += string.Format("{0} ", ParseGameArgFromJson(innerVerArg["game"]));
        //        //    }
        //        //}
        //        if (v2.Arguments.Game != null)
        //        {
        //            ver.MinecraftArguments += string.Format("{0} ", ParseGameArgFromJson(v2.Arguments.Game));
        //        }
        //        #endregion

        //        ver = 

        //        #endregion
        //    }
        //    else if (obj.ContainsKey("minecraftArguments"))
        //    {
        //        #region 旧版本添加默认JVM参数
        //        SendDebugLog(string.Format("检测到\"{0}\"使用旧版本启动参数", ver.Id));
        //        ver.JvmArguments = "-Djava.library.path=${natives_directory} -cp ${classpath}";
        //        #endregion
        //    }
        //    else
        //    {
        //        #region 异常版本
        //        return null;
        //        //throw new Exception(string.Format("Unsupported version {0}", obj["id"]));
        //        #endregion
        //    }

        //    #region 处理依赖
        //    ver.Libraries = new List<Library>();
        //    ver.Natives = new List<Native>();
        //    var libToken = obj["libraries"];
        //    foreach (JToken lib in libToken)
        //    {
        //        var libObj = lib.ToObject<JLibrary>(JsonSerializer);

        //        if (CheckAllowed(libObj.Rules))
        //        {
        //            if (libObj.Natives == null)
        //            {
        //                //不为native
        //                Library library = new Library(libObj.Name);
        //                if (!string.IsNullOrWhiteSpace(libObj.Url))
        //                {
        //                    library.Url = libObj.Url;
        //                }
        //                if (libObj.Downloads?.Artifact != null)
        //                {
        //                    library.LibDownloadInfo = libObj.Downloads.Artifact;
        //                }
        //                ver.Libraries.Add(library);
        //            }
        //            else
        //            {
        //                //为native
        //                Native native = new Native(libObj.Name, libObj.Natives["windows"].Replace("${arch}", SystemTools.GetSystemArch() == ArchEnum.x64 ? "64" : "32"));
        //                if (libObj.Extract != null)
        //                {
        //                    native.Exclude = libObj.Extract.Exculde;
        //                }
        //                if (libObj.Downloads?.Artifact != null)
        //                {
        //                    native.LibDownloadInfo = libObj.Downloads.Artifact;
        //                }
        //                if (libObj.Downloads?.Classifiers?.Natives != null)
        //                {
        //                    native.NativeDownloadInfo = libObj.Downloads.Classifiers.Natives;
        //                }
        //                ver.Natives.Add(native);
        //            }
        //        }
        //    }
        //    #endregion

        //    #region 处理版本继承
        //    if (innerVer != null)
        //    {
        //        var iv = JsonToVersion(innerVer);
        //        if (iv != null)
        //        {
        //            ver.Assets = iv.Assets;
        //            ver.AssetIndex = iv.AssetIndex;
        //            ver.Natives.AddRange(iv.Natives);
        //            ver.Libraries.AddRange(iv.Libraries);
        //            //todo 不知道这的jar干啥的
        //            // ver.Jar = iv.Id;
        //        }
        //    }
        //    #endregion

        //    return ver;
        //}

        public VersionBase JsonToVersion(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<VersionBase>(jsonStr);
        }

        

        public VersionBase GetVersion(string ID)
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
            string jsonPath = handler.GetJsonPath(id);
            if (!File.Exists(jsonPath))
            {
                return null;
            }
            return File.ReadAllText(jsonPath, Encoding.UTF8);
        }

        public async Task<VersionBase> GetVersionAsync(string ID)
        {
            return await Task.Factory.StartNew(() =>
            {
                return GetVersion(ID);
            });
        }

        public List<VersionBase> GetVersions()
        {
            dirInfo.Refresh();
            if (!dirInfo.Exists)
            {
                return new List<VersionBase>();
            }
            var dirs = dirInfo.EnumerateDirectories();
            List<VersionBase> versions = new List<VersionBase>();
            foreach (var item in dirs)
            {
                var ver = GetVersion(item.Name);
                if (ver != null)
                {
                    versions.Add(ver);
                }
            }
            return versions;
        }

        public async Task<List<VersionBase>> GetVersionsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                return GetVersions();
            });
        }

        private void SendDebugLog(string str)
        {
            VersionReaderLog?.Invoke(this, new Log(LogLevel.DEBUG, str));
        }
    }
}

