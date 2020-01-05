using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.FunctionAPI;
using NsisoLauncherCore.Util.Installer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
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
        ObservableCollection<JWLiteloader> liteloaderList = new ObservableCollection<JWLiteloader>();

        private FunctionAPIHandler apiHandler;

        public NewDownloadTaskWindow()
        {
            apiHandler = new FunctionAPIHandler(App.Config.MainConfig.Download.DownloadSource);
            InitializeComponent();
            versionListDataGrid.ItemsSource = verList;
            forgeListDataGrid.ItemsSource = forgeList;
            liteloaderListDataGrid.ItemsSource = liteloaderList;
            ICollectionView vwV = CollectionViewSource.GetDefaultView(verList);
            vwV.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            vwV.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            ICollectionView vwF = CollectionViewSource.GetDefaultView(forgeList);
            vwF.SortDescriptions.Add(new SortDescription("Version", ListSortDirection.Descending));
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<Version> vers = await App.Handler.GetVersionsAsync();
            verToInstallForgeComboBox.ItemsSource = vers;
            verToInstallLiteComboBox.ItemsSource = vers;
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
            List<JWForge> result = null;
            forgeList.Clear();
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

        private async void RefreshLiteloader()
        {
            Version ver = null;
            if (verToInstallLiteComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallLiteComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync("您未选择要安装Liteloader的版本", "您需要选择一个需要安装Liteloader的Minecraft本体");
                return;
            }
            var loading = await this.ShowProgressAsync("获取Liteloader列表中", "请稍后");
            loading.SetIndeterminate();
            JWLiteloader result = new JWLiteloader();
            liteloaderList.Clear();
            try
            {
                result = await apiHandler.GetLiteloaderList(ver);
            }
            catch (WebException)
            {
                await this.ShowMessageAsync("获取Liteloader列表失败", "请检查您的网络是否正常或稍后再试");
                return;
            }
            await loading.CloseAsync();
            if (result == null)
            {
                await this.ShowMessageAsync("没有匹配该版本的Liteloader", "貌似没有支持这个版本的Liteloader，请尝试更换另一个版本");
            }
            else
            {
                liteloaderList.Add(result);
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

            AppendForgeDownloadTask(ver, forge);
            this.Close();
        }

        private async void DownloadLiteloaderButton_Click(object sender, RoutedEventArgs e)
        {
            Version ver = null;
            if (verToInstallLiteComboBox.SelectedItem != null)
            {
                ver = (Version)verToInstallLiteComboBox.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync("您未选择要安装Liteloader的Minecraft", "您需要选择一个需要安装Liteloader的Minecraft本体");
                return;
            }

            JWLiteloader liteloader = null;
            if (liteloaderListDataGrid.SelectedItem != null)
            {
                liteloader = (JWLiteloader)liteloaderListDataGrid.SelectedItem;
            }
            else
            {
                await this.ShowMessageAsync("您未选择要安装的Liteloader", "您需要选择一个要安装Liteloader");
                return;
            }

            AppendLiteloaderDownloadTask(ver, liteloader);
            this.Close();
        }

        private async Task AppendVersionsDownloadTask(IList list)
        {
            try
            {
                foreach (JWVersion item in list)
                {
                    string json = await APIRequester.HttpGetStringAsync(apiHandler.DoURLReplace(item.Url));
                    NsisoLauncherCore.Modules.Version ver = App.Handler.JsonToVersion(json);
                    string jsonPath = App.Handler.GetJsonPath(ver.ID);

                    string dir = Path.GetDirectoryName(jsonPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(jsonPath, json);

                    List<DownloadTask> tasks = new List<DownloadTask>();

                    tasks.Add(new DownloadTask("资源引导", apiHandler.DoURLReplace(ver.AssetIndex.URL), App.Handler.GetAssetsIndexPath(ver.Assets)));

                    tasks.AddRange(await NsisoLauncherCore.Util.FileHelper.GetLostDependDownloadTaskAsync(App.Config.MainConfig.Download.DownloadSource, App.Handler, ver));

                    App.Downloader.AddDownloadTask(tasks);
                    App.Downloader.StartDownload();
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

        private void AppendForgeDownloadTask(Version ver, JWForge forge)
        {
            string forgePath = NsisoLauncherCore.PathManager.TempDirectory + string.Format(@"\Forge_{0}-Installer.jar", forge.Build);
            DownloadTask dt = new DownloadTask("forge核心",
                string.Format("https://bmclapi2.bangbang93.com/forge/download/{0}", forge.Build),
                forgePath);
            dt.Todo = new Func<Exception>(() =>
            {
                try
                {
                    CommonInstaller installer = new CommonInstaller(forgePath, new CommonInstallOptions() { GameRootPath = App.Handler.GameRootPath });
                    installer.BeginInstall();
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            App.Downloader.AddDownloadTask(dt);
            App.Downloader.StartDownload();

        }

        private void AppendLiteloaderDownloadTask(Version ver, JWLiteloader liteloader)
        {
            string liteloaderPath = NsisoLauncherCore.PathManager.TempDirectory + string.Format(@"\Liteloader_{0}-Installer.jar", liteloader.Version);
            DownloadTask dt = new DownloadTask("liteloader核心",
                string.Format("https://bmclapi2.bangbang93.com/liteloader/download?version={0}", liteloader.Version),
                liteloaderPath);
            dt.Todo = new Func<Exception>(() =>
            {
                try
                {
                    CommonInstaller installer = new CommonInstaller(liteloaderPath, new CommonInstallOptions() { GameRootPath = App.Handler.GameRootPath });
                    installer.BeginInstall();
                    return null;
                }
                catch (Exception ex)
                { return ex; }
            });
            App.Downloader.AddDownloadTask(dt);
            App.Downloader.StartDownload();

        }

        private async Task InstallCommonExtend(string path)
        {
            CommonInstaller installer = new CommonInstaller(path, new CommonInstallOptions() { GameRootPath = App.Handler.GameRootPath });
            await installer.BeginInstallAsync();
        }

        private void RefreshVerButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshVersion();
        }

        private void RefreshForgeButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshForge();
        }

        private void RefresLiteButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLiteloader();
        }

        private void VerToInstallForgeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshForge();
        }

        private void VerToInstallLiteComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshLiteloader();
        }
    }
}
