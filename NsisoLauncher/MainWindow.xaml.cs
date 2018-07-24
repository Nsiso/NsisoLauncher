using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Core.Modules;
using NsisoLauncher.Core.Net.MojangApi.Endpoints;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using NsisoLauncher.Core.Util;

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
            App.handler.GameExit += Handler_GameExit;
            Refresh();
        }

        private void Handler_GameExit(object sender, int ret)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.WindowState = WindowState.Normal;
            }));
        }

        private async void Refresh()
        {
            launchVersionCombobox.ItemsSource = await App.handler.GetVersionsAsync();
            this.playerNameTextBox.Text = App.config.MainConfig.User.UserName;
            this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
            await CustomizeRefresh();
            App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
        }

        #region 自定义
        private async Task CustomizeRefresh()
        {
            if (!string.IsNullOrWhiteSpace(App.config.MainConfig.Customize.LauncherTitle))
            {
                this.Title = App.config.MainConfig.Customize.LauncherTitle;
            }
            if (App.config.MainConfig.Customize.CustomBackGroundPicture)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "bgpic_?.png");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    ImageBrush brush = new ImageBrush(new BitmapImage(new Uri(files[random.Next(files.Count())])))
                    { TileMode = TileMode.FlipXY, AlignmentX = AlignmentX.Right, Stretch = Stretch.UniformToFill };
                    this.Background = brush;
                }
            }

            if (App.config.MainConfig.Customize.CustomBackGroundMusic)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.config.MainConfigPath), "bgmusic_?.mp3");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    mediaElement.Source = new Uri(files[random.Next(files.Count())]);
                    this.volumeButton.Visibility = Visibility.Visible;
                    mediaElement.Play();
                    mediaElement.Volume = 0;
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            for (int i = 0; i < 50; i++)
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.mediaElement.Volume += 0.01;
                                }));
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception ex)
                        {
                            AggregateExceptionArgs args = new AggregateExceptionArgs()
                            {
                                AggregateException = new AggregateException(ex)
                            };
                            App.CatchAggregateException(this, args);
                        }
                    });
                }
            }

            if (App.config.MainConfig.Server.ShowServerInfo)
            {
                Server server = new Server() { Address = App.config.MainConfig.Server.Address, Port = App.config.MainConfig.Server.Port };
                await ShowServerInfo(server);
            }
            else
            {
                serverInfoGrid.Visibility = Visibility.Hidden;
            }
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            this.mediaElement.IsMuted = !this.mediaElement.IsMuted;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            mediaElement.Play();
        }
        #endregion

        #region 显示服务器信息
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>  
        /// 从bitmap转换成ImageSource  
        /// </summary>  
        /// <param name="icon"></param>  
        /// <returns></returns>  
        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            //Bitmap bitmap = icon.ToBitmap();  
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;

        }

        private async Task ShowServerInfo(Server info)
        {
            Core.Net.Server.ServerInfo serverInfo = new Core.Net.Server.ServerInfo(info) { ServerName = App.config.MainConfig.Server.ServerName };
            serverInfoGrid.Visibility = Visibility.Visible;
            serverLoadingBar.Visibility = Visibility.Visible;
            serverLoadingBar.IsIndeterminate = true;
            await serverInfo.StartGetServerInfoAsync();
            serverLoadingBar.IsIndeterminate = false;
            serverLoadingBar.Visibility = Visibility.Hidden;
            this.serverNameTextBlock.Text = serverInfo.ServerName;
            switch (serverInfo.State)
            {
                case Core.Net.Server.ServerInfo.StateType.GOOD:
                    this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.CheckCircleSolid;
                    this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Green;
                    this.serverPeopleTextBlock.Text = string.Format("人数:[{0}/{1}]", serverInfo.CurrentPlayerCount, serverInfo.MaxPlayerCount);
                    this.serverVersionTextBlock.Text = serverInfo.GameVersion;
                    this.serverVersionTextBlock.ToolTip = serverInfo.GameVersion;
                    this.serverPingTextBlock.Text = string.Format("延迟:{0}ms", serverInfo.Ping);
                    this.serverMotdTextBlock.Text = serverInfo.MOTD;
                    this.serverMotdTextBlock.ToolTip = serverInfo.MOTD;
                    if (serverInfo.OnlinePlayersName != null)
                    {
                        this.serverPeopleTextBlock.ToolTip = string.Join("\n", serverInfo.OnlinePlayersName);
                    }
                    if (serverInfo.IconData != null)
                    {
                        using (MemoryStream ms = new MemoryStream(serverInfo.IconData))
                        {
                            this.serverIcon.Fill = new ImageBrush(ChangeBitmapToImageSource(new Bitmap(ms)));
                        }

                    }
                    break;

                case Core.Net.Server.ServerInfo.StateType.NO_RESPONSE:
                    this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
                    this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Red;
                    this.serverPeopleTextBlock.Text = "获取失败";
                    this.serverVersionTextBlock.Text = "获取失败";
                    this.serverPingTextBlock.Text = "获取失败";
                    this.serverMotdTextBlock.Text = "服务器没有响应，可能网络或服务器发生异常";
                    break;

                case Core.Net.Server.ServerInfo.StateType.BAD_CONNECT:
                    this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
                    this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Red;
                    this.serverPeopleTextBlock.Text = "获取失败";
                    this.serverVersionTextBlock.Text = "获取失败";
                    this.serverPingTextBlock.Text = "获取失败";
                    this.serverMotdTextBlock.Text = "服务器连接失败，服务器可能不存在或网络异常";
                    break;

                case Core.Net.Server.ServerInfo.StateType.EXCEPTION:
                    this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
                    this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Red;
                    this.serverPeopleTextBlock.Text = "获取失败";
                    this.serverVersionTextBlock.Text = "获取失败";
                    this.serverPingTextBlock.Text = "获取失败";
                    this.serverMotdTextBlock.Text = "启动器获取服务器信息时发生内部异常";
                    break;
                default:
                    break;
            }
        }
        #endregion

        private async void launchButton_Click(object sender, RoutedEventArgs e)
        {
            await LaunchGameFromWindow();
        }

        private async Task LaunchGameFromWindow()
        {
            try
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
                if (App.handler.Java == null)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
                    return;
                }
                #endregion

                LaunchSetting launchSetting = new LaunchSetting()
                {
                    Version = (Core.Modules.Version)launchVersionCombobox.SelectedItem
                };

                this.loadingGrid.Visibility = Visibility.Visible;
                this.loadingRing.IsActive = true;

                #region 检查游戏完整
                List<Core.Net.DownloadTask> losts = new List<Core.Net.DownloadTask>();

                if (App.config.MainConfig.Environment.DownloadLostDepend)
                {
                    MessageDialogResult downDependResult = await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.NeedDownloadDepend"),
                        App.GetResourceString("String.Mainwindow.NeedDownloadDepend2"),
                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
                        {
                            AffirmativeButtonText = App.GetResourceString("String.Base.Download"),
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            FirstAuxiliaryButtonText = App.GetResourceString("String.Base.Unremember"),
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });
                    switch (downDependResult)
                    {
                        case MessageDialogResult.Affirmative:
                            losts.AddRange(GetLost.GetLostDependDownloadTask(App.config.MainConfig.Download.DownloadSource,
                                App.handler,
                                launchSetting.Version));
                            break;
                        case MessageDialogResult.FirstAuxiliary:
                            App.config.MainConfig.Environment.DownloadLostDepend = false;
                            break;
                        default:
                            break;
                    }

                }
                if (App.config.MainConfig.Environment.DownloadLostAssets)
                {
                    losts.AddRange(GetLost.GetLostAssetsDownloadTask(App.config.MainConfig.Download.DownloadSource,
                    App.handler,
                    await App.handler.GetAssetsAsync(launchSetting.Version)));
                }

                if (losts.Count != 0)
                {
                    App.downloader.SetDownloadTasks(losts);
                    App.downloader.StartDownload();
                    await new Windows.DownloadWindow().ShowWhenDownloading();
                }

                #endregion

                #region 验证

                //在线验证
                if ((bool)isOnlineLogin.IsChecked)
                {
                    Core.Net.MojangApi.Api.Requester.ClientToken = App.config.MainConfig.User.ClientToken;
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
                            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
                            AffirmativeButtonText = App.GetResourceString("String.Base.Login"),
                            RememberCheckBoxText = App.GetResourceString("String.Base.ShouldRememberLogin"),
                            UsernameWatermark = App.GetResourceString("String.Base.Username"),
                            InitialUsername = userName,
                            RememberCheckBoxVisibility = Visibility,
                            EnablePasswordPreview = true,
                            PasswordWatermark = App.GetResourceString("String.Base.Password")
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

                #region 根据配置文件设置
                launchSetting.AdvencedGameArguments = App.config.MainConfig.Environment.AdvencedGameArguments;
                launchSetting.AdvencedJvmArguments = App.config.MainConfig.Environment.AdvencedJvmArguments;
                launchSetting.GCArgument = App.config.MainConfig.Environment.GCArgument;
                launchSetting.GCEnabled = App.config.MainConfig.Environment.GCEnabled;
                launchSetting.GCType = App.config.MainConfig.Environment.GCType;
                launchSetting.JavaAgent = App.config.MainConfig.Environment.JavaAgent;
                if (App.config.MainConfig.Server.LaunchToServer)
                {
                    launchSetting.LaunchToServer = new Server() { Address = App.config.MainConfig.Server.Address, Port = App.config.MainConfig.Server.Port };
                }
                if (App.config.MainConfig.Environment.AutoMemory)
                {
                    var m = Core.Util.SystemTools.GetBestMemory(App.handler.Java);
                    App.config.MainConfig.Environment.MaxMemory = m;
                    launchSetting.MaxMemory = m;
                }
                launchSetting.VersionType = App.config.MainConfig.Customize.VersionInfo;
                launchSetting.WindowSize = App.config.MainConfig.Environment.WindowSize;
                #endregion

                #region 配置文件处理
                App.config.MainConfig.User.UserName = userName;
                App.config.Save();
                #endregion

                #region 启动

                App.handler.GameLog += (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b; }); };
                var result = await App.handler.LaunchAsync(launchSetting);
                App.handler.GameLog -= (a, b) => { this.Invoke(() => { launchInfoBlock.Text = b; }); };

                if (!result.IsSuccess)
                {
                    await this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.LaunchError") + result.LaunchException.Title, result.LaunchException.Message);
                    App.logHandler.AppendError(result.LaunchException);
                }
                else
                {
                    await Task.Factory.StartNew(() =>
                    {
                        result.Process.WaitForInputIdle();
                        //while (true)
                        //{
                        //    if (result.Process.HasExited)
                        //    {
                        //        this.Dispatcher.Invoke(new Action(() =>
                        //        {
                        //            this.ShowMessageAsync(App.GetResourceString("String.Mainwindow.GameExitWithNoWindow"),
                        //                App.GetResourceString("String.Mainwindow.GameExitWithNoWindow2"));
                        //        }));
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        if (result.Process.MainWindowHandle.ToInt32() != 0)
                        //        {
                        //            break;
                        //        }
                        //    }
                        //}
                    });
                    if (App.config.MainConfig.Environment.ExitAfterLaunch)
                    {
                        Application.Current.Shutdown();
                    }
                    this.WindowState = WindowState.Minimized;

                    //自定义处理
                    if (!string.IsNullOrWhiteSpace(App.config.MainConfig.Customize.GameWindowTitle))
                    {
                        GameHelper.SetGameTitle(result, App.config.MainConfig.Customize.GameWindowTitle);
                    }
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            for (int i = 0; i < 50; i++)
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.mediaElement.Volume -= 0.01;
                                }));
                                Thread.Sleep(50);
                            }
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.mediaElement.Stop();
                            }));
                        }
                        catch (Exception ex)
                        {
                            AggregateExceptionArgs args = new AggregateExceptionArgs()
                            {
                                AggregateException = new AggregateException(ex)
                            };
                            App.CatchAggregateException(this, args);
                        }
                    });
                }
                #endregion
            }
            finally
            {
                this.loadingGrid.Visibility = Visibility.Hidden;
                this.loadingRing.IsActive = false;
            }
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            new Windows.DownloadWindow().ShowDialog();
            Refresh();
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            new Windows.SettingWindow().ShowDialog();
            Refresh();
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.handler.Java == null)
            {
                await this.ShowMessageAsync(App.GetResourceString("String.Message.NoJava"), App.GetResourceString("String.Message.NoJava2"));
            }
        }
    }
}
