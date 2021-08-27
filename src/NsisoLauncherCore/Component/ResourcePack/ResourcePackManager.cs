using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Component.ResourcePack
{
    public class ResourcePackManager : IComponentManager<ResourcePackInfo>
    {
        public LaunchHandler LaunchHandler { get; set; }

        public VersionBase Version { get; set; }

        public ObservableCollection<ResourcePackInfo> Items { get; set; }

        public string ResourcePacksDir { get => LaunchHandler.GetVersionResourcePacksDir(Version); }

        private const string disable_ext = "disable";

        public ResourcePackManager(VersionBase ver, LaunchHandler handler)
        {
            Items = new ObservableCollection<ResourcePackInfo>();
            this.Version = ver ?? throw new ArgumentNullException("The resource pack manager's version arg is null");
            this.LaunchHandler = handler ?? throw new ArgumentNullException("The resource pack manager's launch handler arg is null");
        }

        public void Add(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("The resource pack file {0} is not found.", path));
            }
            string ext = Path.GetExtension(path);
            if (ext != ".zip")
            {
                throw new Exception(string.Format("The resource pack file {0} is unsupported file extension (not .zip).", path));
            }
            string name = Path.GetFileName(path);
            string dest = Path.Combine(ResourcePacksDir, name);
            File.Copy(path, dest);
            ResourcePackInfo info = new ResourcePackInfo(dest);
            Items.Add(info);
        }

        public void Disable(ResourcePackInfo item)
        {
            string ext = Path.GetExtension(item.Path);
            string name = Path.GetFileNameWithoutExtension(item.Path);
            string dir = Path.GetDirectoryName(item.Path);
            string dest = Path.Combine(dir, string.Format("{0}.{1}", name, disable_ext));
            if (ext == ".zip")
            {
                File.Move(item.Path, dest);
            }
        }

        public void Enable(ResourcePackInfo item)
        {
            string ext = Path.GetExtension(item.Path);
            string name = Path.GetFileNameWithoutExtension(item.Path);
            string dir = Path.GetDirectoryName(item.Path);
            string dest = Path.Combine(dir, string.Format("{0}.zip", name));
            if (ext == "disable")
            {
                File.Move(item.Path, dest);
            }
        }

        public void Refresh()
        {
            Items.Clear();
            if (!Directory.Exists(ResourcePacksDir))
            {
                return;
            }
            string[] modsPath = Directory.GetFiles(ResourcePacksDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var item in modsPath)
            {
                string ext = Path.GetExtension(item);
                if (ext != ".zip")
                {
                    continue;
                }
                Items.Add(new ResourcePackInfo(item));
            }
        }

        public void Remove(ResourcePackInfo item)
        {
            File.Delete(item.Path);
            Items.Remove(item);
        }
    }
}
