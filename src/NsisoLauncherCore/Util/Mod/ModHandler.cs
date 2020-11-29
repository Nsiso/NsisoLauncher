using ICSharpCode.SharpZipLib.Zip;
using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NsisoLauncherCore.Util.Mod
{
    public class ModHandler
    {
        LaunchHandler _launchHandler;

        public ModHandler(LaunchHandler handler)
        {
            _launchHandler = handler;
        }

        public List<ModInfo> GetMods(Modules.Version version)
        {
            if (_launchHandler == null)
            {
                throw new ArgumentNullException("The launch handler is null, can't find mods");
            }

            if (version == null)
            {
                throw new ArgumentNullException("The version is null, can't find mods");
            }

            string modsDir = _launchHandler.GetVersionModsDir(version);
            if (!Directory.Exists(modsDir))
            {
                return null;
            }
            string[] modsPath = Directory.GetFiles(modsDir, "*.jar");
            List<ModInfo> mods = new List<ModInfo>();

            foreach (var item in modsPath)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    ZipFile zipFile = new ZipFile(item);
                    ZipEntry infoEntry = zipFile.GetEntry("mcmod.info");
                    if (infoEntry != null)
                    {
                        string json;
                        using (Stream zipStream = zipFile.GetInputStream(infoEntry))
                        {
                            using (StreamReader reader = new StreamReader(zipStream))
                            {
                                json = reader.ReadToEnd();
                            }
                        }

                        JToken jobj = JToken.Parse(json);
                        ModInfo info;
                        if (jobj.Type == JTokenType.Array)
                        {
                            info = jobj.ToObject<ModInfo[]>()[0];
                        }
                        else
                        {
                            info = jobj["modList"].ToObject<ModInfo[]>()[0];
                        }

                        if (info != null)
                        {
                            mods.Add(info);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }
            }
            return mods;
        }
    }
}
