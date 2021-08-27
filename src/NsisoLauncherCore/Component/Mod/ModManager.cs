using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Component.Mod
{
    public class ModManager : IComponentManager<ModInfo>
    {
        public LaunchHandler LaunchHandler { get; set; }

        public VersionBase Version { get; set; }

        public ObservableCollection<ModInfo> Items { get; set; }

        public string ModsDir { get => LaunchHandler.GetVersionModsDir(Version); }

        private const string disable_ext = "disable";

        public ModManager(VersionBase ver, LaunchHandler handler)
        {
            Items = new ObservableCollection<ModInfo>();
            this.Version = ver ?? throw new ArgumentNullException("The mod manager's version arg is null");
            this.LaunchHandler = handler ?? throw new ArgumentNullException("The mod manager's launch handler arg is null");
        }

        public void Add(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("The mod file {0} is not found.", path));
            }
            string ext = Path.GetExtension(path);
            if (ext != ".jar" && ext != ".zip")
            {
                throw new Exception(string.Format("The mod file {0} is unsupported mod file extension (not .jar or .zip).", path));
            }
            string name = Path.GetFileName(path);
            string dest = Path.Combine(ModsDir, name);
            File.Copy(path, dest);
            ModInfo info = new ModInfo(dest);
            Items.Add(info);
        }

        public void Disable(ModInfo item)
        {
            string ext = Path.GetExtension(item.Path);
            string name = Path.GetFileNameWithoutExtension(item.Path);
            string dir = Path.GetDirectoryName(item.Path);
            string dest = Path.Combine(dir, string.Format("{0}.{1}", name, disable_ext));
            if (ext == ".jar" || ext == ".zip")
            {
                File.Move(item.Path, dest);
            }
        }

        public void Enable(ModInfo item)
        {
            string ext = Path.GetExtension(item.Path);
            string name = Path.GetFileNameWithoutExtension(item.Path);
            string dir = Path.GetDirectoryName(item.Path);
            string dest = Path.Combine(dir, string.Format("{0}.jar", name));
            if (ext == "disable")
            {
                File.Move(item.Path, dest);
            }
        }

        public void Refresh()
        {
            Items.Clear();
            if (!Directory.Exists(ModsDir))
            {
                return;
            }
            string[] modsPath = Directory.GetFiles(ModsDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var item in modsPath)
            {
                string ext = Path.GetExtension(item);
                if (ext != ".jar" && ext != ".zip")
                {
                    continue;
                }
                Items.Add(new ModInfo(item));
            }
        }

        public void Remove(ModInfo item)
        {
            File.Delete(item.Path);
            Items.Remove(item);
        }
    }
}
