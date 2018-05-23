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
using NsisoLauncher.Core;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Core.Modules;

namespace NsisoLauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Refresh();
        }
            
        private async void Refresh()
        {
            launchVersionCombobox.ItemsSource = await App.handler.GetVersionsAsync();
        }

        private async void launchButton_Click(object sender, RoutedEventArgs e)
        {
            this.loadingGrid.Visibility = Visibility.Visible;
            this.loadingRing.IsActive = true;

            var auth = Core.Auth.OfflineAuthenticator.OfflineAuthenticate("Nsiso");
            LaunchSetting launchSetting = new LaunchSetting()
            {
                Version = (Core.Modules.Version)launchVersionCombobox.SelectedItem,
                MaxMemory = 1024,
                AuthenticateResponse = auth.Item2,
                AuthenticateSelectedUUID = auth.Item1
            };
            var result = await App.handler.LaunchAsync(launchSetting);
            if (!result.IsSuccess)
            {
                await this.ShowMessageAsync("启动失败:" + result.LaunchException.Title, result.LaunchException.Message);
            }
            else
            {
                await Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (result.Process.MainWindowHandle.ToInt32() != 0)
                        {
                            break;
                        }
                    }
                });
                this.loadingGrid.Visibility = Visibility.Hidden;
                this.loadingRing.IsActive = false;
                this.WindowState = WindowState.Minimized;

                await Task.Factory.StartNew(() =>
                {
                    result.Process.WaitForExit();
                });
                this.WindowState = WindowState.Normal;
            }
        }
    }
}
