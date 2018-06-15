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
using NsisoLauncher.Core.Net.MojangApi.Responses;

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
            App.logHandler.AppendDebug("启动器主窗体已载入");
            Refresh();
        }

        private async void Refresh()
        {
            launchVersionCombobox.ItemsSource = await App.handler.GetVersionsAsync();
            this.playerNameTextBox.Text = App.config.MainConfig.User.UserName;
            this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
            App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
        }

        private async void launchButton_Click(object sender, RoutedEventArgs e)
        {
            await LaunchGameFromWindow();
        }

        private async Task LaunchGameFromWindow()
        {
            string userName = playerNameTextBox.Text;

            #region 检查有效数据
            if (string.IsNullOrWhiteSpace(userName))
            {
                await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyUsername"),
                    App.GetResourceString("String.Message.EmptyUsername2"));
                return;
            }
            if (launchVersionCombobox.SelectedItem == null)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.Message.EmptyLaunchVersion"),
                    App.GetResourceString("String.Message.EmptyLaunchVersion2"));
                return;
            }
            #endregion

            LaunchSetting launchSetting = new LaunchSetting()
            {
                Version = (Core.Modules.Version)launchVersionCombobox.SelectedItem
            };

            #region 检查游戏完整
            var losts = Core.Util.GetLost.GetLostDependDownloadTask(Core.Net.DownloadSource.BMCLAPI, App.handler, (Core.Modules.Version)launchVersionCombobox.SelectedItem);
            if (losts.Count != 0)
            {
                App.downloader.SetDownloadTasks(losts);
                App.downloader.StartDownload();
                new Windows.DownloadWindow().Show();
                return;
            }
            #endregion

            #region 验证

            //在线验证
            if ((bool)isOnlineLogin.IsChecked)
            {
                //如果记住登陆
                if ((!string.IsNullOrWhiteSpace(App.config.MainConfig.User.AccessToken)) && (App.config.MainConfig.User.AuthenticationUUID != null))
                {
                    var loader = await this.ShowProgressAsync(App.GetResourceString("String.Mainwindow.AutoVerifying"),
                        App.GetResourceString("String.Mainwindow.AutoVerifying2"));
                    loader.SetIndeterminate();
                    Validate validate = new Validate(App.config.MainConfig.User.AccessToken);
                    var validateResult = await validate.PerformRequestAsync();
                    if (validateResult.IsSuccess)
                    {
                        launchSetting.AuthenticateAccessToken = App.config.MainConfig.User.AccessToken;
                        launchSetting.AuthenticateUUID = App.config.MainConfig.User.AuthenticationUUID;
                        await loader.CloseAsync();
                    }
                    else
                    {
                        Refresh refresher = new Refresh(App.config.MainConfig.User.AccessToken);
                        var refreshResult = await refresher.PerformRequestAsync();
                        await loader.CloseAsync();
                        if (refreshResult.IsSuccess)
                        {
                            App.config.MainConfig.User.AccessToken = refreshResult.AccessToken;

                            launchSetting.AuthenticateUUID = App.config.MainConfig.User.AuthenticationUUID;
                            launchSetting.AuthenticateAccessToken = refreshResult.AccessToken;
                        }
                        else
                        {
                            App.config.MainConfig.User.AccessToken = string.Empty;
                            App.config.Save();
                            await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.AutoVerificationFailed"),
                                App.GetResourceString("String.Mainwindow.AutoVerificationFailed2"));
                            return;
                        }
                    }
                }
                //账号密码验证
                else
                {
                    var loginMsgResult = await this.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Login"),
                        App.GetResourceString("String.Mainwindow.Login2"),
                    new LoginDialogSettings()
                    {
                        NegativeButtonText = "取消",
                        AffirmativeButtonText = "登陆",
                        RememberCheckBoxText = "记住登陆状态",
                        InitialUsername = userName,
                        RememberCheckBoxVisibility = Visibility,
                        EnablePasswordPreview = true,
                        PasswordWatermark = "密码"
                    });
                    if (loginMsgResult == null)
                    {
                        return;
                    }
                    var loader = await this.ShowProgressAsync(App.GetResourceString("String.Mainwindow.Verifying"),
                        App.GetResourceString("String.Mainwindow.Verifying2"));
                    loader.SetIndeterminate();
                    Authenticate authenticate = new Authenticate(new Credentials() { Username = loginMsgResult.Username, Password = loginMsgResult.Password });
                    var loginResult = await authenticate.PerformRequestAsync();
                    await loader.CloseAsync();
                    if (loginResult.IsSuccess)
                    {
                        if (loginMsgResult.ShouldRemember)
                        {
                            App.config.MainConfig.User.AccessToken = loginResult.AccessToken;
                        }
                        App.config.MainConfig.User.AuthenticationUserData = loginResult.User;
                        App.config.MainConfig.User.AuthenticationUUID = loginResult.SelectedProfile;

                        launchSetting.AuthenticateAccessToken = loginResult.AccessToken;
                        launchSetting.AuthenticateUUID = loginResult.SelectedProfile;
                        launchSetting.AuthenticationUserData = loginResult.User;
                    }
                    else
                    {
                        await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.VerificationFailed"),
                            loginResult.Error.ErrorMessage);
                        return;
                    }
                }
            }
            //离线验证
            else
            {
                var loginResult = Core.Auth.OfflineAuthenticator.DoAuthenticate(userName);
                launchSetting.AuthenticateAccessToken = loginResult.Item1;
                launchSetting.AuthenticateUUID = loginResult.Item2;
            }

            #endregion

            #region 配置文件处理
            App.config.MainConfig.User.UserName = userName;
            App.config.Save();
            #endregion

            #region 启动
            this.loadingGrid.Visibility = Visibility.Visible;
            this.loadingRing.IsActive = true;

            var result = await App.handler.LaunchAsync(launchSetting);
            App.handler.GameLog += (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b; }); };

            if (!result.IsSuccess)
            {
                await this.ShowMessageAsync("启动失败:" + result.LaunchException.Title, result.LaunchException.Message);
                this.loadingGrid.Visibility = Visibility.Hidden;
                this.loadingRing.IsActive = false;
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                await Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (result.Process.HasExited)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.ShowMessageAsync("游戏进程在未显示出窗口前退出", "可能启动遇到错误");
                            }));
                            break;
                        }
                        else
                        {
                            if (result.Process.MainWindowHandle.ToInt32() != 0)
                            {
                                break;
                            }
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
            #endregion
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            new Windows.DownloadWindow().Show();
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new Windows.SettingWindow().ShowDialog();
            Refresh();
        }
    }
}
