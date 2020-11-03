using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cyotek.Data.Nbt;
using Cyotek.Data.Nbt.Serialization;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Util
{
    public static class SaveHandler
    {
        public static List<SaveInfo> GetSaves(LaunchHandler handler, Modules.Version version)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("The launch handler is null, can't find saves");
            }

            if (version == null)
            {
                throw new ArgumentNullException("The version is null, can't find saves");
            }

            string savesDir = handler.GetVersionSavesDir(version);
            if (!Directory.Exists(savesDir))
            {
                return null;
            }
            string[] allSaves = Directory.GetDirectories(savesDir);
            List<SaveInfo> maps = new List<SaveInfo>();
            foreach (var item in allSaves)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    TagDictionary tagDic;
                    using (FileStream fileStream = new FileStream(item + "\\level.dat", FileMode.Open))
                    {
                        TagReader reader = new BinaryTagReader(fileStream);
                        tagDic = (TagDictionary)reader.ReadDocument().Value[0].GetValue();
                    }
                    if (tagDic != null)
                    {
                        SaveInfo map = new SaveInfo(item, tagDic);
                        maps.Add(map);
                    }
                }
            }
            return maps;
        }
    }
}
