using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel;

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
            ViewModels.Windows.MainWindowViewModel vm = new ViewModels.Windows.MainWindowViewModel()
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
            if (App.NetHandler.Downloader.IsBusy)
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
                    App.NetHandler.Downloader.RequestCancel();
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
