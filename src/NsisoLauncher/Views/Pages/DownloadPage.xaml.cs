using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Views.Windows;
using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NsisoLauncher.Views.Pages
{
    /// <summary>
    /// DownloadPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : Page
    {
        public DownloadPage()
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new NewDownloadTaskWindow().ShowDialog();
        }

        private async Task<MessageDialogResult> ShowMessageAsync(string title, string msg)
        {
            return await App.MainWindowVM.ShowMessageAsync(title, msg);
        }

        private async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
           MessageDialogStyle style, MetroDialogSettings settings)
        {
            return await App.MainWindowVM.ShowMessageAsync(title, message, style, settings);
        }
    }
}
