using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NsisoLauncherCore.Modules;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncher.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AuthenticationNode> AuthNodes { get; set; }
        public ObservableCollection<AuthenticationNode> Versions { get; set; }
        public ObservableCollection<AuthenticationNode> Users { get; set; }

        #region Commands
        public ICommand LaunchCmd { get; set; }
        public ICommand OpenSettingCmd { get; set; }
        public ICommand OpenDownloadingCmd { get; set; }
        #endregion

        #region Launch Data
        public Version LaunchVersion { get; set; }
        public UserNode LaunchUser { get; set; }
        public AuthenticationNode LaunchAuthNode { get; set; }
        public string LaunchUserNameText { get; set; }
        #endregion

        public IDialogCoordinator Instance { get; set; }

        public MainWindowViewModel()
        {
            AuthNodes = new ObservableCollection<AuthenticationNode>()
            {
                new AuthenticationNode() { Name = "666", AuthType = AuthenticationType.MOJANG }
            };
            LaunchCmd = new DelegateCommand(async (obj) => await LaunchFromVM());
        }

        public async Task LaunchFromVM()
        {
            await Instance.ShowMessageAsync(this, LaunchUserNameText, LaunchAuthNode.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
