using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using NsisoLauncher.Core.Net;
using MahApps.Metro.Controls.Dialogs;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : MetroWindow
    {
        public DownloadWindow()
        {
            InitializeComponent();
            downloadList.ItemsSource = App.downloader.TasksObservableCollection;
            App.downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            App.downloader.DownloadSpeedChanged += Downloader_DownloadSpeedChanged;
            App.downloader.DownloadCompleted += Downloader_DownloadCompleted;
        }

        private void Downloader_DownloadCompleted(object sender, Utils.DownloadCompletedArg e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                speedTextBlock.Text = "0Kb/s";
                progressBar.Value = 0;
                progressPerTextBlock.Text = "000%";
                if (e.ErrorList.Count == 0)
                {
                    this.ShowMessageAsync("下载完成", "所有任务已成功下载");
                }
                else
                {
                    this.ShowMessageAsync("下载完成但发生错误", string.Format("一共有{0}个任务下载失败,其中一个原因:{1}", e.ErrorList.Count, e.ErrorList.First().Value.Message));
                }

            }));
        }

        private void Downloader_DownloadSpeedChanged(object sender, Utils.DownloadSpeedChangedArg e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                speedTextBlock.Text = e.SpeedValue.ToString() + e.SpeedUnit;
            }));
        }

        private void Downloader_DownloadProgressChanged(object sender, Utils.DownloadProgressChangedArg e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.progressBar.Maximum = e.TaskCount;
                this.progressBar.Value = e.TaskCount - e.LastTaskCount;
                this.progressPerTextBlock.Text = ((double)(e.TaskCount - e.LastTaskCount) / (double)e.TaskCount).ToString("0%");
            }));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("您确定要取消下载?", "现在后悔还来得及", 
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings() { AffirmativeButtonText = "确定", NegativeButtonText = "取消" });
            if (result == MessageDialogResult.Affirmative)
            {
                App.downloader.RequestStop();
                this.progressBar.Value = 0;
            }
        }
    }
}
