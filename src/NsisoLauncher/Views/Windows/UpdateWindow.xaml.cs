using MahApps.Metro.Controls;
using NsisoLauncherCore.Net.PhalAPI;
using System.Diagnostics;
using System.Windows;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// UpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        public NsisoLauncherVersionResponse VersionResponse { get; set; }
        public UpdateWindow(NsisoLauncherVersionResponse response)
        {
            InitializeComponent();
            VersionResponse = response;
            versionLabel.Text = VersionResponse.Version.ToString();
            timeLabel.Text = VersionResponse.ReleaseTime.ToShortDateString();
            descriptionLabel.Text = VersionResponse.Description;
            infoTextbox.Text = VersionResponse.UpdateInformation;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //当两个下载直链都为空时用手动链接
            //if (string.IsNullOrWhiteSpace(VersionResponse.DownloadSource_1) && string.IsNullOrWhiteSpace(VersionResponse.DownloadSource_2))
            //{

            //}
            //else
            //{
            //    string path = string.Format("NsisoLauncher-{0}.exe", VersionResponse.Version);
            //    DownloadTask task = new DownloadTask("新版本启动器", VersionResponse.DownloadSource_1, path);
            //    if (!string.IsNullOrWhiteSpace(VersionResponse.MD5))
            //    {
            //        task.Checker = new MD5Checker() { CheckSum = VersionResponse.MD5, FilePath = path };
            //    }
            //    DownloadWindow downloadWindow = new DownloadWindow();
            //    App.NetHandler.Downloader.StartDownload();
            //    downloadWindow.Show();
            //    App.NetHandler.Downloader.DownloadCompleted += (a, b) =>
            //    {
            //        this.show
            //    }
            //}
            Process.Start(new ProcessStartInfo(VersionResponse.DownloadSource_manual));
            if ((bool)noCheckBox.IsChecked)
            {
                App.Config.MainConfig.Launcher.CheckUpdate = false;
                App.Config.Save();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)noCheckBox.IsChecked)
            {
                App.Config.MainConfig.Launcher.CheckUpdate = false;
                App.Config.Save();
            }
            this.Close();
        }
    }
}
