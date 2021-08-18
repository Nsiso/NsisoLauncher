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

        private VersionBase ReadVersion(JObject obj)
        {
            VersionBase version = obj.ToObject<VersionBase>();
            SendDebugLog(string.Format("The version {0} is loaded.", version.Id));

            //处理版本继承
            if (!string.IsNullOrEmpty(version.InheritsFrom))
            {
                SendDebugLog(string.Format("The version {0} is inherited from version {1}.", version.Id, version.InheritsFrom));
                VersionBase base_ver = GetVersion(version.InheritsFrom);
                if (base_ver == null)
                {
                    SendDebugLog(string.Format("The version {0} is mising it's inhertis version.", version.Id));
                    throw new Exception("The inherit version is mising.");
                }

                version.InheritsFromInstance = base_ver;
            }

            return version;

        }

        public VersionBase JsonToVersion(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return null;
            }
            JObject ver_obj = JObject.Parse(jsonStr);
            return ReadVersion(ver_obj);
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
            string jsonPath = handler.GetVersionJsonPath(id);
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

