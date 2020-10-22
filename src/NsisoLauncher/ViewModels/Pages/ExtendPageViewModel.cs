using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncher.ViewModels.Pages
{
    public class ExtendPageViewModel : INotifyPropertyChanged
    {
        public NsisoLauncherCore.Modules.Version SelectedVersion { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public ObservableCollection<NsisoLauncherCore.Modules.Version> VersionList { get; private set; }

        public ExtendPageViewModel()
        {
            if (App.VersionList != null)
            {
                this.VersionList = App.VersionList;
            }
            if (App.LauncherData != null)
            {
                this.SelectedVersion = App.LauncherData.SelectedVersion;
                App.LauncherData.PropertyChanged += LauncherData_PropertyChanged;
            }
            this.PropertyChanged += ExtendPageViewModel_PropertyChanged;
        }

        private void LauncherData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVersion")
            {
                this.SelectedVersion = App.LauncherData.SelectedVersion;
            }
        }

        private void ExtendPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVersion")
            {
                App.LauncherData.SelectedVersion = this.SelectedVersion;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
