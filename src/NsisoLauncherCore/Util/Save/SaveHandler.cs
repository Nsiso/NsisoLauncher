using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cyotek.Data.Nbt;
using Cyotek.Data.Nbt.Serialization;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Util.Save
{
    public class SaveHandler
    {
        LaunchHandler _launchHandler;

        public SaveHandler(LaunchHandler handler)
        {
            _launchHandler = handler;
        }

        public List<SaveInfo> GetSaves(Modules.Version version)
        {
            if (_launchHandler == null)
            {
                throw new ArgumentNullException("The launch handler is null, can't find saves");
            }

            if (version == null)
            {
                throw new ArgumentNullException("The version is null, can't find saves");
            }

            string savesDir = _launchHandler.GetVersionSavesDir(version);
            if (!Directory.Exists(savesDir))
            {
                return null;
            }
            string[] allSaves = Directory.GetDirectories(savesDir);
            List<SaveInfo> maps = new List<SaveInfo>();
            foreach (var item in allSaves)
            {
                try
                {
                    string levelPath = item + "\\level.dat";
                    if ((!string.IsNullOrWhiteSpace(item)) && File.Exists(levelPath))
                    {
                        TagDictionary tagDic;
                        using (FileStream fileStream = new FileStream(levelPath, FileMode.Open))
                        {
                            TagReader reader = new BinaryTagReader(fileStream);
                            var doc = reader.ReadDocument();
                            tagDic = (TagDictionary)doc.Value[0].GetValue();
                        }
                        if (tagDic != null)
                        {
                            SaveInfo map = new SaveInfo(item, tagDic);
                            maps.Add(map);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }
            }
            return maps;
        }

        public void DeleteSave(SaveInfo save)
        {
            save.DeleteSave();
        }

        public void AddSave(Modules.Version version, string path)
        {
            if (_launchHandler == null)
            {
                throw new ArgumentNullException("The launch handler is null, can't add save");
            }
            if (version == null)
            {
                throw new ArgumentNullException("The version is null, can't add save");
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The save path to add is null, can't add save");
            }

            string savesDir = _launchHandler.GetVersionSavesDir(version);
        }
    }
}
