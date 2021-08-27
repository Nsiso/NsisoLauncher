using Cyotek.Data.Nbt;
using Cyotek.Data.Nbt.Serialization;
using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Component.Save
{
    public class SaveManager : IComponentManager<SaveInfo>
    {
        public LaunchHandler LaunchHandler { get; set; }

        public VersionBase Version { get; set; }

        public ObservableCollection<SaveInfo> Items { get; set; }

        public string SavesDir { get => LaunchHandler.GetVersionSavesDir(Version); }

        public SaveManager(VersionBase ver, LaunchHandler handler)
        {
            Items = new ObservableCollection<SaveInfo>();
            this.Version = ver ?? throw new ArgumentNullException("The mod manager's version arg is null");
            this.LaunchHandler = handler ?? throw new ArgumentNullException("The mod manager's launch handler arg is null");
        }

        public void Add(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(string.Format("The save dir {0} is not found.", path));
            }
            DirectoryInfo source_dir = new DirectoryInfo(path);
            string dest = Path.Combine(SavesDir, source_dir.Name);
            Util.FileHelper.DirectoryCopy(path, dest, true, false);

            string levelPath = dest + "\\level.dat";
            if (File.Exists(levelPath))
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
                    SaveInfo map = new SaveInfo(dest, tagDic);
                    Items.Add(map);
                }
            }
        }

        public void Disable(SaveInfo item)
        {
            throw new NotImplementedException();
        }

        public void Enable(SaveInfo item)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            Items.Clear();
            if (!Directory.Exists(SavesDir))
            {
                return;
            }
            string[] allSaves = Directory.GetDirectories(SavesDir);
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
                            Items.Add(map);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }
            }
        }

        public void Remove(SaveInfo item)
        {
            Directory.Delete(item.Path, true);
            Items.Remove(item);
        }
    }
}
