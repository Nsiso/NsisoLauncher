using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net.FunctionAPI;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;
using System.IO;
using NsisoLauncherCore.Net;
using System.Net;
using NsisoLauncherCore.Modules;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// NewDownloadTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewDownloadTaskWindow : MetroWindow
    {
        private FunctionAPIHandler apiHandler;

        public NewDownloadTaskWindow()
        {
            apiHandler = new FunctionAPIHandler(App.config.MainConfig.Download.DownloadSource);
            InitializeComponent();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<Version> vers = await App.handler.GetVersionsAsync();
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
            if (result == null)
            {
                await this.ShowMessageAsync("获取版本列表失败", "请检查您的网络是否正常或更改下载源");
            }
            else
            {
                versionListDataGrid.ItemsSource = result;
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
            try
            {
                result = await apiHandler.GetForgeList(ver);
            }
            catch (WebException)
            {
                result = null;
            }
            await loading.CloseAsync();
            if (result == null)
            {
                await this.ShowMessageAsync("获取Forge列表失败", "请检查您的网络是否正常或更改下载源");
            }
            else
            {
                forgeListDataGrid.ItemsSource = result;

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
            List<JWLiteloader> result = new List<JWLiteloader>();
            try
            {
                result.Add(await apiHandler.GetLiteloaderList(ver));
            }
            catch (WebException)
            {
                result = null;
            }
            await loading.CloseAsync();
            if (result == null)
            {
                await this.ShowMessageAsync("获取Liteloader列表失败", "请检查您的网络是否正常或更改下载源");
            }
            else
            {
                liteloaderListDataGrid.ItemsSource = result;

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
                var loading = await this.ShowProgressAsync("准备进行下载", string.Format("即将为您下载{0}个版本",selectItems.Count));
                loading.SetIndeterminate();
                await AppendVersionsDownloadTask(selectItems);
                await loading.CloseAsync();
                this.Close();
            }
        }

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

            var loading = await this.ShowProgressAsync("准备进行下载", string.Format("即将为版本{0}下载安装Forge:{1}", ver.ID, forge.Version));
            loading.SetIndeterminate();
            //await AppendVersionsDownloadTask(selectItems);
            await loading.CloseAsync();
            this.Close();
        }

        private async void DownloadLiteloaderButton_Click(object sender, RoutedEventArgs e)
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

        private async Task AppendVersionsDownloadTask(IList list)
        {
            try
            {
                foreach (JWVersion item in list)
                {
                    string json = await APIRequester.HttpGetStringAsync(apiHandler.DoURLReplace(item.Url));
                    NsisoLauncherCore.Modules.Version ver = App.handler.JsonToVersion(json);
                    string jsonPath = App.handler.GetJsonPath(ver.ID);

                    string dir = Path.GetDirectoryName(jsonPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(jsonPath, json);

                    List<DownloadTask> tasks = new List<DownloadTask>();

                    tasks.Add(new DownloadTask("资源引导", apiHandler.DoURLReplace(ver.AssetIndex.URL), App.handler.GetAssetsIndexPath(ver.Assets)));

                    tasks.AddRange(await NsisoLauncherCore.Util.FileHelper.GetLostDependDownloadTaskAsync(App.config.MainConfig.Download.DownloadSource, App.handler, ver));

                    App.downloader.SetDownloadTasks(tasks);
                    App.downloader.StartDownload();
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
            try
            {
                //string json = await APIRequester.HttpGetStringAsync(apiHandler.DoURLReplace(item.Url));
                //Version ver = App.handler.JsonToVersion(json);
                //string jsonPath = App.handler.GetJsonPath(ver.ID);

                //string dir = Path.GetDirectoryName(jsonPath);
                //if (!Directory.Exists(dir))
                //{
                //    Directory.CreateDirectory(dir);
                //}

                //File.WriteAllText(jsonPath, json);

                //List<DownloadTask> tasks = new List<DownloadTask>();

                //tasks.Add(new DownloadTask("资源引导", apiHandler.DoURLReplace(ver.AssetIndex.URL), App.handler.GetAssetsIndexPath(ver.Assets)));

                //tasks.AddRange(await NsisoLauncherCore.Util.FileHelper.GetLostDependDownloadTaskAsync(App.config.MainConfig.Download.DownloadSource, App.handler, ver));

                //App.downloader.SetDownloadTasks(tasks);
                //App.downloader.StartDownload();
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
    }
}
