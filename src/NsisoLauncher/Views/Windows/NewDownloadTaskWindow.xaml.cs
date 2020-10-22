using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using NsisoLauncherCore.Util.Installer;
using NsisoLauncherCore.Util.Installer.Forge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// NewDownloadTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewDownloadTaskWindow : MetroWindow
    {
        ObservableCollection<JWVersion> verList = new ObservableCollection<JWVersion>();
        ObservableCollection<JWForge> forgeList = new ObservableCollection<JWForge>();

        private FunctionAPIHandler apiHandler;
        private NetRequester _netRequester;

        public NewDownloadTaskWindow()
        {
            _netRequester = App.NetHandler.Requester;
            apiHandler = new FunctionAPIHandler(App.NetHandler.Mirrors.VersionListMirrorList, App.NetHandler.Mirrors.FunctionalMirrorList, _netRequester);
            InitializeComponent();
            versionListDataGrid.ItemsSource = verList;
            forgeListDataGrid.ItemsSource = forgeList;
            ICollectionView vwV = CollectionViewSource.GetDefaultView(verList);
            vwV.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            vwV.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            ICollectionView vwF = CollectionViewSource.GetDefaultView(forgeList);
            vwF.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<Version> vers = await App.Handler.GetVersionsAsync();
            verToInstallForgeComboBox.ItemsSource = vers.Where(x => string.IsNullOrWhiteSpace(x.InheritsFrom));
        }

        private async void RefreshVersion()
        {
            var loading = await this.ShowProgressAsync("获取版本列表中", "请稍后");
            loading.SetIndeterminate();
            List<JWVersion> result = null;
            try
            {
                result = await apiHandler.GetVersionList();
            }
            catch (WebException)
            {
                result = null;
            }
            await loading.CloseAsync();
            verList.Clear();
            if (result == null)
            {
                await this.ShowMessageAsync("获取版本列表失败", "请检查您的网络是否正常或更改下载源");
            }
            else
            {
                foreach (var item in result)
                {
                    switch (item.Type)
                    {
                        case "release":
                            item.Type = "1-正式版(Release)";
                            break;
                        case "snapshot":
                            item.Type = "2-版本快照(snapshot)";
                            break;
                        case "old_alpha":
                            item.Type = "3-旧alpha版本(old_alpha)";
                            break;
                        case "old_beta":
                            item.Type = "4-旧beta版本(old_beta)";
                            break;
                        default:
                            break;
                    }
                    verList.Add(item);
                }
            }
        }

        private async void RefreshForge()
        {
            Version ver = null;
            if (verToInstallForgeComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallForgeComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync("您未选择要安装Forge的版本", "您需要选择一个需要安装Forge的Minecraft本体");
                return;
            }
            var loading = await this.ShowProgressAsync("获取Forge列表中", "请稍后");
            loading.SetIndeterminate();
            forgeList.Clear();
            List<JWForge> result;
            try
            {
                result = await apiHandler.GetForgeList(ver);
            }
            catch (WebException)
            {
                await this.ShowMessageAsync("获取Forge列表失败", "请检查您的网络是否正常或稍后再试");
                return;
            }
            await loading.CloseAsync();
            if (result == null || result.Count == 0)
            {
                await this.ShowMessageAsync("没有匹配该版本的Forge", "貌似没有支持这个版本的Forge，请尝试更换另一个版本");
            }
            else
            {
                foreach (var item in result)
                {
                    forgeList.Add(item);
                }
            }
        }

        private async void DownloadVerButton_Click(object sender, RoutedEventArgs e)
        {
            IList selectItems = versionListDataGrid.SelectedItems;
            if (selectItems.Count == 0)
            {
                await this.ShowMessageAsync("您未选中要下载的版本", "请在版本列表中选中您要下载的版本");
            }
            else
            {
                var loading = await this.ShowProgressAsync("准备进行下载", string.Format("即将为您下载{0}个版本", selectItems.Count));
                loading.SetIndeterminate();
                await AppendVersionsDownloadTask(selectItems);
                await loading.CloseAsync();
                this.Close();
            }
        }

        //TODO:修复FORGE刷新不成功崩溃
        private async void DownloadForgeButton_Click(object sender, RoutedEventArgs e)
        {
            Version ver = null;
            if (verToInstallForgeComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallForgeComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync("您未选择要安装Forge的Minecraft", "您需要选择一个需要安装Forge的Minecraft本体");
                return;
            }

            JWForge forge = null;
            if (forgeListDataGrid.SelectedItem != null)
            {
                forge = (JWForge)forgeListDataGrid.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync("您未选择要安装的Forge", "您需要选择一个要安装Forge");
                return;
            }

            await AppendForgeDownloadTask(ver, forge);
            this.Close();
        }

        private async Task AppendVersionsDownloadTask(IList list)
        {
            try
            {
                foreach (JWVersion item in list)
                {
                    IDownloadableMirror mirror = (IDownloadableMirror)await MirrorHelper.ChooseBestMirror(App.NetHandler.Mirrors.DownloadableMirrorList);
                    string url;
                    if (mirror == null)
                    {
                        url = item.Url;
                    }
                    else
                    {
                        url = mirror.DoDownloadUriReplace(item.Url);
                    }
                    HttpResponseMessage jsonRespond = await _netRequester.Client.GetAsync(url);
                    string json = null;
                    if (jsonRespond.IsSuccessStatusCode)
                    {
                        json = await jsonRespond.Content.ReadAsStringAsync();
                    }
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        await this.ShowMessageAsync("获取版本Json失败", "请检查您的网络是否正常或更改下载源");
                        return;
                    }
                    NsisoLauncherCore.Modules.Version ver = App.Handler.JsonToVersion(json);
                    string jsonPath = App.Handler.GetJsonPath(ver.Id);

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
                    await App.NetHandler.Downloader.StartDownload();
                }
            }
            catch (WebException ex)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ShowMessageAsync("获取版本信息失败", "请检查您的网络是否正常或更改下载源/n原因:" + ex.Message);
                }));
            }
            catch (Exception ex)
            {
                AggregateExceptionArgs args = new AggregateExceptionArgs()
                {
                    AggregateException = new AggregateException(ex)
                };
                App.CatchAggregateException(this, args);
            }

        }

        private async Task AppendForgeDownloadTask(Version ver, JWForge forge)
        {
            IFunctionalMirror functionalMirror = (IFunctionalMirror)(await MirrorHelper.ChooseBestMirror(App.NetHandler.Mirrors.FunctionalMirrorList));
            if (functionalMirror == null)
            {
                throw new Exception("Functional Mirror is null");
            }
            string forgePath = NsisoLauncherCore.PathManager.TempDirectory + string.Format(@"\Forge_{0}-Installer.jar", forge.Build);
            DownloadTask dt = new DownloadTask("forge核心",
                new StringUrl(string.Format("{0}forge/download/{1}", functionalMirror.BaseUri, forge.Build)),
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
                        Java = App.Handler.Java
                    });
                    installer.BeginInstall(callback, cancelToken);
                    this.Dispatcher.Invoke(() =>
                    {
                        App.RefreshVersionList();
                    });
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            App.NetHandler.Downloader.AddDownloadTask(dt);
            await App.NetHandler.Downloader.StartDownload();

        }

        private void RefreshVerButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshVersion();
        }

        private void RefreshForgeButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshForge();
        }

        private void VerToInstallForgeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshForge();
        }
    }
}
