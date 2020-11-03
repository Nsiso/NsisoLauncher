using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
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

        /// <summary>
        /// 版本存档
        /// </summary>
        public ObservableCollection<SaveInfo> VerSaves { get; set; }

        public ExtendPageViewModel()
        {
            VerSaves = new ObservableCollection<SaveInfo>();
            if (App.VersionList != null)
            {
                this.VersionList = App.VersionList;
            }
            if (App.LauncherData != null)
            {
                App.LauncherData.PropertyChanged += LauncherData_PropertyChanged;
                this.SelectedVersion = App.LauncherData.SelectedVersion;
                UpdateVersionSaves();
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
                UpdateVersionSaves();
            }
        }

        private void UpdateVersionSaves()
        {
            VerSaves.Clear();
            if (this.SelectedVersion != null)
            {
                List<SaveInfo> saves = SaveHandler.GetSaves(App.Handler, this.SelectedVersion);
                if (saves != null)
                {
                    foreach (var item in saves)
                    {
                        VerSaves.Add(item);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
