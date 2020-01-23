using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {

            //THIS IS NOT MVVM
            ViewModels.Windows.MainWindowViewModel vm = new ViewModels.Windows.MainWindowViewModel(DialogCoordinator.Instance)
            {
                CloseWindow = new Action(() => this.Close())
            };
            InitializeComponent();
            this.DataContext = vm;

            //THIS IS NOT MVVM
            vm.NavigationService = frame.NavigationService;
            vm.InitializeMainPage();
            App.LogHandler.AppendDebug("启动器主窗体已载入");
        }


        #region MainWindow event
        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (App.Downloader.IsBusy)
            {
                var choose = this.ShowModalMessageExternal("后台正在下载中", "是否确认关闭程序？这将会取消下载"
                , MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText = App.GetResourceString("String.Base.Yes"),
                    NegativeButtonText = App.GetResourceString("String.Base.Cancel")
                });
                if (choose == MessageDialogResult.Affirmative)
                {
                    App.Downloader.RequestCancel();
                }
                else
                {
                    e.Cancel = true;
                }
            }

        }
        #endregion
    }
}
