using System.IO;

namespace NsisoLauncherCore.Component.ResourcePack
{
    public class ResourcePackInfo : IComponent
    {
        /// <summary>
        /// Mod file name
        /// </summary>
        public string Path { get; set; }

        public string Name { get => System.IO.Path.GetFileNameWithoutExtension(Path); }

        public ComponentState State
        {
            get
            {
                string ext = System.IO.Path.GetExtension(this.Path);
                return ext == "zip" ? ComponentState.ENABLE : ComponentState.DISABLE;
            }
        }

        public ResourcePackInfo()
        {

        }

        public ResourcePackInfo(string comp_path)
        {
            this.Path = comp_path;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
