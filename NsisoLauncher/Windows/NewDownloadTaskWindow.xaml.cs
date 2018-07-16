using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Core.Modules;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NsisoLauncher.Core.Net.FunctionAPI;
using static NsisoLauncher.Core.Net.FunctionAPI.APIModules;
using System.IO;
using NsisoLauncher.Core.Net;

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

        private async void RefreshVersion()
        {
            var loading = await this.ShowProgressAsync("获取版本列表中", "请稍后");
            loading.SetIndeterminate();
            var result = await Task.Factory.StartNew(() =>
            {
                return apiHandler.GetVersionList();
            });
            versionListDataGrid.ItemsSource = result;
            await loading.CloseAsync();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
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

        private async Task AppendVersionsDownloadTask(IList list)
        {
            await Task.Factory.StartNew(() =>
            {
                foreach (JWVersion item in list)
                {
                    string json = FunctionAPIHandler.HttpGet(item.Url);
                    Core.Modules.Version ver = App.handler.JsonToVersion(json);
                    string jsonPath = App.handler.GetJsonPath(ver.ID);

                    string dir = System.IO.Path.GetDirectoryName(jsonPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(jsonPath, json);

                    List<DownloadTask> tasks = new List<DownloadTask>();

                    tasks.Add(new DownloadTask("游戏核心", apiHandler.DoURLReplace(ver.Downloads.Client.URL), App.handler.GetJarPath(ver)));

                    tasks.Add(new DownloadTask("资源引导", apiHandler.DoURLReplace(ver.AssetIndex.URL), App.handler.GetAssetsIndexPath(ver.Assets)));

                    tasks.AddRange(Core.Util.GetLost.GetLostDependDownloadTask(App.config.MainConfig.Download.DownloadSource, App.handler, ver));

                    App.downloader.SetDownloadTasks(tasks);
                    App.downloader.StartDownload();
                }
            });
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RefreshVersion();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshVersion();
        }
    }
}
