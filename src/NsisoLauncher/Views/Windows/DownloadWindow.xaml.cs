using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net;
using System.Windows;

namespace NsisoLauncher.Views.Windows
{

    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : MetroWindow
    {
        public DownloadWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.NetHandler.Downloader.IsBusy)
            {
                var result = await this.ShowMessageAsync(App.GetResourceString("String.Downloadwindow.MakesureCancel"),
               App.GetResourceString("String.Downloadwindow.MakesureCancel"),
               MessageDialogStyle.AffirmativeAndNegative,
               new MetroDialogSettings()
               {
                   AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                   NegativeButtonText = App.GetResourceString("String.Base.Cancel")
               });
                if (result == MessageDialogResult.Affirmative)
                {
                    App.NetHandler.Downloader.RequestCancel();
                    this.progressBar.Value = 0;
                }
            }
            else
            {
                await this.ShowMessageAsync("没有需要取消下载的任务", "当前下载器并没有在工作");
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.NetHandler.Downloader.IsBusy)
            {
                this.ShowModalMessageExternal("正在下载中", "将会在后台进行下载，再次打开下载窗口能查看或取消下载");
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetRequester.NetProxy != null)
            {
                this.ShowMessageAsync("您开启了下载代理", "请注意您现在正在使用代理进行下载，若代理设置异常可能会导致下载错误。");
            }
        }
    }
}
