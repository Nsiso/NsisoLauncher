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
using NsisoLauncher.Core.Net.MojangApi.Endpoints;

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
            this.playerNameTextBox.Text = App.config.MainConfig.User.UserName;
        }

        private async void launchButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = playerNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(userName))
            {
                await this.ShowMessageAsync("您的用户名未填写", "请在用户名栏填写您的用户名");
                return;
            }
            if (launchVersionCombobox.SelectedItem == null)
            {
                await this.ShowMessageAsync("您未选择启动版本", "请在启动版本栏选择您要启动的版本");
                return;
            }

            LaunchSetting launchSetting = new LaunchSetting()
            {
                Version = (Core.Modules.Version)launchVersionCombobox.SelectedItem
            };

            if ((bool)isOnlineLogin.IsChecked)
            {
                var loginMsgResult = await this.ShowLoginAsync("请确认登陆信息", "您启用了在线验证",
                    new LoginDialogSettings()
                    {
                        NegativeButtonText = "取消",
                        AffirmativeButtonText = "登陆",
                        RememberCheckBoxText = "记住登陆状态",
                        InitialUsername = userName
                    });
                var loader = await this.ShowProgressAsync("进行在线验证中...,", "这可能需要几秒的时间，我们正在进行处理");
                loader.SetIndeterminate();
                Authenticate authenticate = new Authenticate(new Credentials() { Username = loginMsgResult.Username, Password = loginMsgResult.Password});
                var loginResult = await authenticate.PerformRequestAsync();
                await loader.CloseAsync();
                if (loginResult.IsSuccess)
                {
                    App.config.MainConfig.User.AccessToken = loginResult.AccessToken;
                }
                else
                {
                    await this.ShowMessageAsync("在线验证失败", loginResult.Error.ErrorMessage);
                    return;
                }
            }
            else
            {
                var loginResult = Core.Auth.OfflineAuthenticator.DoAuthenticate(userName);
                launchSetting.AuthenticateResponse = loginResult.Item1;
                launchSetting.AuthenticateSelectedUUID = loginResult.Item2;
            }

            App.config.MainConfig.User.UserName = userName;
            App.config.Save();

            this.loadingGrid.Visibility = Visibility.Visible;
            this.loadingRing.IsActive = true;

            
            
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
