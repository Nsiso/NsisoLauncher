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
using NsisoLauncherCore.Util;
using NsisoLauncherCore.Util.Installer;
using NsisoLauncherCore.Util.Installer.Fabric;
using NsisoLauncherCore.Util.Installer.Forge;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public ObservableCollection<VersionBase> LocalVersionList { get; set; }
        public VersionBase SelectedLocalVersion { get; set; }
        public ObservableCollection<JWVersion> DownloadVersionList { get; set; }
        public JWVersion SelectedDownloadVersion { get; set; }

        public ObservableCollection<JWForge> ForgeList { get; set; }
        public ObservableCollection<JWFabric> FabricList { get; set; }

        public JWForge SelectedForge { get; set; }
        public JWFabric SelectedFabric { get; set; }

        public ICollectionView DownloadVersionListView { get; set; }

        public string VersionListFilterString { get; set; }

        public ICommand LoadedCmd { get; set; }
        public ICommand SearchVersionCmd { get; set; }
        public ICommand RefreshVersionCmd { get; set; }
        public ICommand DownloadVersionCmd { get; set; }

        public ICommand RefreshForgeListCmd { get; set; }
        public ICommand RefreshFabricListCmd { get; set; }
        public ICommand DownloadForgeCmd { get; set; }
        public ICommand DownloadFabricCmd { get; set; }

        private MainWindowViewModel _mainWindow;
        private MainPageViewModel _mainPage;

        public AdditionPageViewModel()
        {
            if (App.NetHandler != null)
            {
                VersionListMirror = App.NetHandler.Mirrors.VersionListMirrorList.FirstOrDefault();
                FunctionalMirror = App.NetHandler.Mirrors.FunctionalMirrorList.FirstOrDefault();
            }
            if (App.MainWindowVM != null)
            {
                _mainWindow = App.MainWindowVM;
            }
            if (App.MainPageVM != null)
            {
                _mainPage = App.MainPageVM;
            }
            if (App.VersionList != null)
            {
                LocalVersionList = App.VersionList;
            }
            DownloadVersionList = new ObservableCollection<JWVersion>();
            ForgeList = new ObservableCollection<JWForge>();
            FabricList = new ObservableCollection<JWFabric>();

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
            RefreshForgeListCmd = new DelegateCommand(async (x) =>
            {
                await RefreshForgeList();
            });
            DownloadForgeCmd = new DelegateCommand(async (x) =>
            {
                await DownloadForge();
            });
            RefreshFabricListCmd = new DelegateCommand(async (x) =>
            {
                await RefreshFabricList();
            });
            DownloadFabricCmd = new DelegateCommand(async (x) =>
            {
                await DownloadFabric();
            });

            this.PropertyChanged += AdditionPageViewModel_PropertyChanged;
        }

        private async void AdditionPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedLocalVersion")
            {
                await RefreshForgeList();
            }
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
                VersionManifest manifest = await FunctionAPIHandler.GetVersionManifest(VersionListMirror);
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

        private async Task RefreshForgeList()
        {
            try
            {
                VersionBase ver = SelectedLocalVersion;
                if (ver == null)
                {
                    await _mainWindow.ShowMessageAsync("您没有选择要安装的Minecraft版本", "请选择您要安装Forge的Minecraft版本");
                    ForgeList.Clear();
                    return;
                }
                if (ver.InheritsFrom != null)
                {
                    await _mainWindow.ShowMessageAsync("您选择的Minecraft不可安装Forge", "该版本已经继承于一个原有的版本，不可再安装");
                    ForgeList.Clear();
                    return;
                }
                var loading = await _mainWindow.ShowProgressAsync("获取Forge列表中", "请稍后");
                loading.SetIndeterminate();
                ForgeList.Clear();
                List<JWForge> result = await FunctionAPIHandler.GetForgeList(FunctionalMirror, ver);
                await loading.CloseAsync();
                if (result == null || result.Count == 0)
                {
                    await _mainWindow.ShowMessageAsync("没有匹配该版本的Forge", "貌似没有支持这个版本的Forge，请尝试更换另一个版本");
                }
                else
                {
                    foreach (var item in result)
                    {
                        ForgeList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                await _mainWindow.ShowMessageAsync(string.Format("获取Forge列表失败：{0}", ex.Message), ex.ToString());
                return;
            }

        }

        private async Task RefreshFabricList()
        {
            try
            {
                VersionBase ver = SelectedLocalVersion;
                if (ver == null)
                {
                    await _mainWindow.ShowMessageAsync("您没有选择要安装的Minecraft版本", "请选择您要安装Fabric的Minecraft版本");
                    FabricList.Clear();
                    return;
                }
                if (ver.InheritsFrom != null)
                {
                    await _mainWindow.ShowMessageAsync("您选择的Minecraft不可安装Fabric", "该版本已经继承于一个原有的版本，不可再安装");
                    FabricList.Clear();
                    return;
                }
                var loading = await _mainWindow.ShowProgressAsync("获取Fabric列表中", "请稍后");
                loading.SetIndeterminate();
                FabricList.Clear();
                List<JWFabric> result = await FunctionAPIHandler.GetFabricList(FunctionalMirror, ver);
                await loading.CloseAsync();
                if (result == null || result.Count == 0)
                {
                    await _mainWindow.ShowMessageAsync("没有匹配该版本的Fabric", "貌似没有支持这个版本的Fabric，请尝试更换另一个版本");
                }
                else
                {
                    foreach (var item in result)
                    {
                        FabricList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                await _mainWindow.ShowMessageAsync(string.Format("获取Fabric列表失败：{0}", ex.Message), ex.ToString());
                return;
            }

        }

        private async Task DownloadVersion()
        {
            try
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
                    HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(url);
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

                    tasks.AddRange(await FileHelper.GetLostDependDownloadTaskAsync(App.Handler, ver, App.NetHandler.Mirrors.VersionListMirrorList));

                    App.NetHandler.Downloader.AddDownloadTask(tasks);
                    _mainPage.NavigateToDownloadPage();
                    await App.NetHandler.Downloader.StartDownload();
                    await loading.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                await _mainWindow.ShowMessageAsync(string.Format("下载版本失败：{0}", ex.Message), ex.ToString());
                return;
            }

        }

        private async Task DownloadForge()
        {
            try
            {
                VersionBase ver = SelectedLocalVersion;
                if (ver == null)
                {
                    await _mainWindow.ShowMessageAsync("您未选择要安装Forge的Minecraft", "您需要选择一个需要安装Forge的Minecraft本体");
                    return;
                }

                JWForge forge = SelectedForge;
                if (forge == null)
                {
                    await _mainWindow.ShowMessageAsync("您未选择要安装的Forge", "您需要选择一个要安装Forge");
                    return;
                }

                if (FunctionalMirror == null)
                {
                    throw new Exception("Functional Mirror is null");
                }
                string forgePath = $"{NsisoLauncherCore.PathManager.TempDirectory}/forge-{ver.Id}-{forge.Version}-installer.jar";
                DownloadTask dt = new DownloadTask("forge核心",
                    new StringUrl(FunctionAPIHandler.GetForgeDownload(FunctionalMirror, ver, forge)),
                    forgePath);
                IDownloadableMirror mirror = (IDownloadableMirror)await MirrorHelper.ChooseBestMirror(App.NetHandler.Mirrors.DownloadableMirrorList);
                dt.DownloadObject.Todo = new Func<ProgressCallback, CancellationToken, Exception>((callback, cancelToken) =>
                {
                    try
                    {
                        IInstaller installer = new ForgeInstaller(forgePath, new CommonInstallOptions()
                        {
                            GameRootPath = App.Handler.GameRootPath,
                            IsClient = true,
                            VersionToInstall = ver,
                            Mirror = mirror,
                            Java = Java.GetSuitableJava(App.JavaList, ver)
                        });
                        installer.BeginInstall(callback, cancelToken);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.RefreshVersionList();
                        });
                        return null;
                    }
                    catch (Exception ex)
                    { return ex; }
                });
                App.NetHandler.Downloader.AddDownloadTask(dt);
                _mainPage.NavigateToDownloadPage();
                await App.NetHandler.Downloader.StartDownload();
            }
            catch (Exception ex)
            {
                await _mainWindow.ShowMessageAsync($"下载Forge失败：{ex.Message}", ex.ToString());
                return;
            }

        }

        private async Task DownloadFabric()
        {
            try
            {
                VersionBase ver = SelectedLocalVersion;
                if (ver == null)
                {
                    await _mainWindow.ShowMessageAsync("您未选择要安装Fabric的Minecraft", "您需要选择一个需要安装Fabric的Minecraft本体");
                    return;
                }

                JWFabric fabric = SelectedFabric;
                if (fabric == null)
                {
                    await _mainWindow.ShowMessageAsync("您未选择要安装的Fabric", "您需要选择一个要安装Fabric");
                    return;
                }

                if (FunctionalMirror == null)
                {
                    throw new Exception("Functional Mirror is null");
                }
                string forgePath = $"{NsisoLauncherCore.PathManager.TempDirectory}/fabric-installer.jar";
                DownloadTask dt = new DownloadTask("fabric安装核心",
                    new StringUrl(await FunctionAPIHandler.GetFabricDownload(FunctionalMirror)),
                    forgePath);
                IDownloadableMirror mirror = (IDownloadableMirror)await MirrorHelper.ChooseBestMirror(App.NetHandler.Mirrors.DownloadableMirrorList);
                dt.DownloadObject.Todo = new Func<ProgressCallback, CancellationToken, Exception>((callback, cancelToken) =>
                {
                    try
                    {
                        IInstaller installer = new FabricInstaller(forgePath, new FabricInstallOptions()
                        {
                            GameRootPath = App.Handler.GameRootPath,
                            IsClient = true,
                            VersionToInstall = ver,
                            Fabric = fabric,
                            Mirror = mirror,
                            Java = Java.GetSuitableJava(App.JavaList, ver)
                        });
                        installer.BeginInstall(callback, cancelToken);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            App.RefreshVersionList();
                        });
                        return null;
                    }
                    catch (Exception ex)
                    { return ex; }
                });
                App.NetHandler.Downloader.AddDownloadTask(dt);
                _mainPage.NavigateToDownloadPage();
                await App.NetHandler.Downloader.StartDownload();
            }
            catch (Exception ex)
            {
                await _mainWindow.ShowMessageAsync(string.Format("下载Forge失败：{0}", ex.Message), ex.ToString());
                return;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
