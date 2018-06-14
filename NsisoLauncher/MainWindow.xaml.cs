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
                await this.ShowMessageAsync("您的用户名未填写", "请在用户名栏填写您的用户名");
                return;
            }
            if (launchVersionCombobox.SelectedItem == null)
            {
                await this.ShowMessageAsync("您未选择启动版本", "请在启动版本栏选择您要启动的版本");
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
                    var loader = await this.ShowProgressAsync("在线验证中(自动验证)...", "这可能需要几秒的时间，我们正在进行处理。若您想关闭自动验证，请在设置窗口中进行设置");
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
                            await this.ShowMessageAsync("验证失败", "验证信息已过期，请重新登陆");
                            return;
                        }
                    }
                }
                //账号密码验证
                else
                {
                    var loginMsgResult = await this.ShowLoginAsync("请确认登陆信息", "您启用了在线验证",
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
                    var loader = await this.ShowProgressAsync("在线验证中...", "这可能需要几秒的时间，我们正在进行处理");
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
                        await this.ShowMessageAsync("在线验证失败", loginResult.Error.ErrorMessage);
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
