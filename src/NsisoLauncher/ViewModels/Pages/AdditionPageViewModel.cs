using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncher.ViewModels.Pages
{
    public class AdditionPageViewModel : INotifyPropertyChanged
    {
        public IVersionListMirror VersionListMirror { get; set; }
        public IFunctionalMirror FunctionalMirror { get; set; }
        public IDownloadableMirror DownloadableMirror { get; set; }

        public JWVersionLatest Latest { get; set; }
        public bool IsLoadingVersion { get; set; } = true;
        public ObservableCollection<JWVersion> DownloadVersionList { get; set; }
        public JWVersion SelectedDownloadVersion { get; set; }
        public ICollectionView DownloadVersionListView { get; set; }
        public ObservableCollection<JWForge> ForgeList { get; set; }

        public string VersionListFilterString { get; set; }

        public ICommand LoadedCmd { get; set; }
        public ICommand SearchVersionCmd { get; set; }
        public ICommand RefreshVersionCmd { get; set; }
        public ICommand DownloadVersionCmd { get; set; }

        private FunctionAPIHandler apiHandler;
        private NetRequester _netRequester;

        private MainWindowViewModel _mainWindow;
        private MainPageViewModel _mainPage;



        public AdditionPageViewModel()
        {
            if (App.NetHandler != null)
            {
                _netRequester = App.NetHandler.Requester;
                VersionListMirror = App.NetHandler.Mirrors.VersionListMirrorList.FirstOrDefault();
                FunctionalMirror = App.NetHandler.Mirrors.FunctionalMirrorList.FirstOrDefault();
                apiHandler = new FunctionAPIHandler(VersionListMirror, FunctionalMirror, _netRequester);
            }
            if (App.MainWindowVM != null)
            {
                _mainWindow = App.MainWindowVM;
            }
            if (App.MainPageVM != null)
            {
                _mainPage = App.MainPageVM;
            }
            DownloadVersionList = new ObservableCollection<JWVersion>();
            ForgeList = new ObservableCollection<JWForge>();

            DownloadVersionListView = CollectionViewSource.GetDefaultView(DownloadVersionList);
            DownloadVersionListView.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            DownloadVersionListView.Filter = FindVersion;

            LoadedCmd = new DelegateCommand(async (x) =>
            {
                await RefreshVersionList();
            });
            SearchVersionCmd = new DelegateCommand(async (x) =>
            {
                await RefreshVersionList();
            });
            RefreshVersionCmd = new DelegateCommand(async (x) =>
            {
                await RefreshVersionList();
            });
            DownloadVersionCmd = new DelegateCommand(async (x) =>
            {
                await DownloadVersion();
            });
        }
        private bool FindVersion(object obj)
        {
            JWVersion ver = (JWVersion)obj;
            if (string.IsNullOrWhiteSpace(VersionListFilterString))
            {
                return true;
            }
            else
            {
                return ver.Id.Contains(VersionListFilterString);
            }
        }


        private async Task RefreshVersionList()
        {
            IsLoadingVersion = true;
            try
            {
                VersionManifest manifest = await apiHandler.GetVersionManifest();
                Latest = manifest.Latest;
                DownloadVersionList.Clear();
                foreach (var item in manifest.Versions)
                {
                    DownloadVersionList.Add(item);
                }
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

        private async Task DownloadVersion()
        {
            if (SelectedDownloadVersion == null)
            {
                await _mainWindow.ShowMessageAsync("您未选中要下载的版本", "请在版本列表中选中您要下载的版本");
            }
            else
            {
                var loading = await _mainWindow.ShowProgressAsync("准备进行下载", string.Format("即将为您下载版本{0}", SelectedDownloadVersion.Id));
                loading.SetIndeterminate();

                JWVersion item = SelectedDownloadVersion;
                IDownloadableMirror mirror = DownloadableMirror;
                string url = mirror == null ? item.Url : mirror.DoDownloadUriReplace(item.Url);
                HttpResponseMessage jsonRespond = await _netRequester.Client.GetAsync(url);
                string json = null;
                if (jsonRespond.IsSuccessStatusCode)
                {
                    json = await jsonRespond.Content.ReadAsStringAsync();
                }
                if (string.IsNullOrWhiteSpace(json))
                {
                    await _mainWindow.ShowMessageAsync("获取版本Json失败", "请检查您的网络是否正常或更改下载源");
                    return;
                }
                VersionBase ver = App.Handler.JsonToVersion(json);
                string jsonPath = App.Handler.GetVersionJsonPath(ver.Id);

                string dir = Path.GetDirectoryName(jsonPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(jsonPath, json);
                App.VersionList.Add(ver);

                List<DownloadTask> tasks = new List<DownloadTask>();

                tasks.Add(new DownloadTask("资源引导", new StringUrl(ver.AssetIndex.Url), App.Handler.GetAssetsIndexPath(ver.Assets)));

                tasks.AddRange(await NsisoLauncherCore.Util.FileHelper.GetLostDependDownloadTaskAsync(App.Handler, ver, App.NetHandler.Mirrors.VersionListMirrorList, App.NetHandler.Requester));

                App.NetHandler.Downloader.AddDownloadTask(tasks);
                _mainPage.NavigateToDownloadPage();
                await App.NetHandler.Downloader.StartDownload();
                await loading.CloseAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
