using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncher.ViewModels.Pages
{
    public class AdditionPageViewModel : INotifyPropertyChanged
    {
        public JWVersionLatest Latest { get; set; }
        public bool IsLoadingVersion { get; set; } = true;
        public List<JWVersion> VersionList { get; set; }
        public List<JWForge> ForgeList { get; set; }

        public ICommand LoadedCmd { get; set; }

        private FunctionAPIHandler apiHandler;
        private NetRequester _netRequester;

        private MainWindowViewModel _mainWindow;



        public AdditionPageViewModel()
        {
            if (App.NetHandler?.Requester != null)
            {
                _netRequester = App.NetHandler.Requester;
            }
            if (App.MainWindowVM != null)
            {
                _mainWindow = App.MainWindowVM;
            }

            apiHandler = new FunctionAPIHandler(App.NetHandler.Mirrors.VersionListMirrorList, App.NetHandler.Mirrors.FunctionalMirrorList, _netRequester);

            LoadedCmd = new DelegateCommand(async (x) =>
            {
                await RefreshVersionList();
            });
        }

        private async Task RefreshVersionList()
        {
            IsLoadingVersion = true;
            try
            {
                VersionManifest manifest = await apiHandler.GetVersionManifest();
                Latest = manifest.Latest;
                VersionList = manifest.Versions;
            }
            catch (Exception ex)
            {
                await _mainWindow.ShowMessageAsync(string.Format("获得版本列表发生错误:{0}", ex.Message), ex.ToString());
            }
            finally
            {
                IsLoadingVersion = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
