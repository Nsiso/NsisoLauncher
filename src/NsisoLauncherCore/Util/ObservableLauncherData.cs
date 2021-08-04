using NsisoLauncherCore.Modules;
using System.ComponentModel;

namespace NsisoLauncherCore.Util
{
    public class ObservableLauncherData : INotifyPropertyChanged
    {
        /// <summary>
        /// 选中的版本
        /// </summary>
        public VersionBase SelectedVersion { get; set; }

        ///// <summary>
        ///// 登录的用户
        ///// </summary>
        //public User LoggedUser { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
