using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using NsisoLauncher.Core.Net;
using NsisoLauncher.Core.Util;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// InstallHelperWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InstallHelperWindow : MetroWindow
    {
        public InstallHelperWindow()
        {
            InitializeComponent();
        }

        //private async void JavaButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (App.javaList.Count != 0)
        //    {
        //        var diResult = await this.ShowMessageAsync("当前电脑已经安装了JAVA", "您确认要继续安装？", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
        //        {
        //            AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
        //            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
        //            DefaultButtonFocus = MessageDialogResult.Affirmative
        //        });
        //        if (diResult == MessageDialogResult.Canceled)
        //        {
        //            return;
        //        }
        //    }
        //    InstallJavaOptions options = new InstallJavaOptions()
        //    {
        //        SilentInstall = (bool)javaSilentInstallSwitch.IsChecked
        //    };
        //    if (!(bool)javaDefaultDirSwitch.IsChecked)
        //    {
        //        System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();
        //        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            options.InstallDir = openFileDialog.SelectedPath;
        //        }
        //    }
        //    var arch = SystemTools.GetSystemArch();
        //    Process p = null;
        //    switch (arch)
        //    {
        //        case ArchEnum.x32:
        //            App.downloader.SetDownloadTasks(new DownloadTask("32位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x86.exe", "jre_x86.exe"));
        //            App.downloader.StartDownload();
        //            await new DownloadWindow().ShowWhenDownloading();
        //            p = Java.InstallJava("jre_x86.exe", options);
        //            break;
        //        case ArchEnum.x64:
        //            //App.downloader.SetDownloadTasks(new DownloadTask("64位JAVA安装包", @"https://bmclapi.bangbang93.com/java/jre_x64.exe", "jre_x64.exe"));
        //            //App.downloader.StartDownload();
        //            //await new DownloadWindow().ShowWhenDownloading();
        //            p = Java.InstallJava("jre_x64.exe", options);
        //            break;
        //        default:
        //            break;
        //    }
        //    if (p!=null)
        //    {
        //        p.BeginOutputReadLine();
        //        p.OutputDataReceived += (a, b) =>
        //        {
        //            this.Dispatcher.Invoke(new Action(() =>
        //            {
        //                this.javaLogTextBox.AppendText(b.Data + "\n");
        //            }));
        //        };
        //    }
        //}
    }
}
