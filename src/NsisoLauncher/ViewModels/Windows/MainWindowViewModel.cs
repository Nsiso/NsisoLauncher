using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace NsisoLauncher.ViewModels.Windows
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 窗口状态
        /// </summary>
        public WindowState WindowState { get; set; }

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string WindowTitle { get; set; } = "Nsiso Launcher";

        /// <summary>
        /// 导航服务实例
        /// </summary>
        public NavigationService NavigationService { get; set; }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public Action CloseWindow { get; set; }

        /// <summary>
        /// 窗口对话
        /// </summary>
        private readonly IDialogCoordinator instance;

        #region ElementsState

        public double Volume { get; set; } = 0.5;

        public string StaticMediaSource { get; set; } = /*@"C:\Users\nsiso\Desktop\a.jpg"*/"../../Resource/home-hero-1200x600.jpg";

        public string MediaSource { get; set; }/* = @"C:\Users\nsiso\Desktop\ME\mp4\view.mp4";*/

        public bool IsPlaying { get; set; } = true;
        #endregion

        public MainWindowViewModel()
        {
            this.instance = DialogCoordinator.Instance;

            App.MainWindowVM = this;

            if (App.Handler != null)
            {
                App.Handler.GameExit += Handler_GameExit;
                _ = CustomizeRefresh();
            }
        }

        public void InitializeMainPage()
        {
            this.NavigationService.Navigate(new Views.Pages.WelcomePage());
        }

        private async void Handler_GameExit(object sender, GameExitArg arg)
        {
            this.WindowState = WindowState.Normal;
            if (!arg.IsNormalExit())
            {
                await ShowMessageAsync("游戏非正常退出",
                    string.Format("这很有可能是因为游戏崩溃导致的，退出代码:{0}，游戏持续时间:{1}",
                    arg.ExitCode, arg.Duration));
            }
        }

        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message)
        {
            return await instance.ShowMessageAsync(this, title, message);
        }

        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle style, MetroDialogSettings settings)
        {
            return await instance.ShowMessageAsync(this, title, message, style, settings);
        }

        public async Task<LoginDialogData> ShowLoginAsync(string title, string message,
            LoginDialogSettings settings)
        {
            return await instance.ShowLoginAsync(this, title, message, settings);
        }

        public async Task<LoginDialogData> ShowLoginAsync(string title, string message)
        {
            return await instance.ShowLoginAsync(this, title, message);
        }

        public async Task<ProgressDialogController> ShowProgressAsync(string title, string message,
            bool isCancelable, MetroDialogSettings settings)
        {
            return await instance.ShowProgressAsync(this, title, message, isCancelable, settings);
        }

        public async Task<ProgressDialogController> ShowProgressAsync(string title, string message)
        {
            return await instance.ShowProgressAsync(this, title, message);
        }

        public async Task ShowMetroDialogAsync(BaseMetroDialog dialog)
        {
            await instance.ShowMetroDialogAsync(this, dialog);
        }

        public async Task ShowMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings settings)
        {
            await instance.ShowMetroDialogAsync(this, dialog, settings);
        }

        public async Task HideMetroDialogAsync(BaseMetroDialog dialog)
        {
            await instance.HideMetroDialogAsync(this, dialog);
        }

        public async Task HideMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings settings)
        {
            await instance.HideMetroDialogAsync(this, dialog, settings);
        }

        public async Task<string> ShowInputAsync(string title, string msg)
        {
            return await instance.ShowInputAsync(this, title, msg);
        }

        public async Task<string> ShowInputAsync(string title, string msg, MetroDialogSettings settings)
        {
            return await instance.ShowInputAsync(this, title, msg, settings);
        }

        private async Task CustomizeRefresh()
        {
            if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.LauncherTitle))
            {
                this.WindowTitle = App.Config.MainConfig.Customize.LauncherTitle;
            }
            if (App.Config.MainConfig.Customize.CustomBackGroundPicture)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "bgpic_?.png");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    MediaSource = files[random.Next(files.Count())];
                    //ImageBrush brush = new ImageBrush(new BitmapImage(new Uri()))
                    //{ TileMode = TileMode.FlipXY, AlignmentX = AlignmentX.Right, Stretch = Stretch.UniformToFill };
                    //this.Background = brush;
                }
            }

            //undo app back server info control
            //if (App.Config.MainConfig.User.Nide8ServerDependence)
            //{
            //    try
            //    {
            //        var lockAuthNode = App.Config.MainConfig.User.GetLockAuthNode();
            //        if ((lockAuthNode != null) &&
            //            (lockAuthNode.AuthType == AuthenticationType.NIDE8))
            //        {
            //            Config.Server nide8Server = new Config.Server() { ShowServerInfo = true };
            //            var nide8ReturnResult = await (new NsisoLauncherCore.Net.Nide8API.APIHandler(lockAuthNode.Property["nide8ID"])).GetInfoAsync();
            //            if (!string.IsNullOrWhiteSpace(nide8ReturnResult.Meta.ServerIP))
            //            {
            //                string[] serverIp = nide8ReturnResult.Meta.ServerIP.Split(':');
            //                if (serverIp.Length == 2)
            //                {
            //                    nide8Server.Address = serverIp[0];
            //                    nide8Server.Port = ushort.Parse(serverIp[1]);
            //                }
            //                else
            //                {
            //                    nide8Server.Address = nide8ReturnResult.Meta.ServerIP;
            //                    nide8Server.Port = 25565;
            //                }
            //                nide8Server.ServerName = nide8ReturnResult.Meta.ServerName;
            //                serverInfoControl.SetServerInfo(nide8Server);
            //            }
            //        }

            //    }
            //    catch (Exception)
            //    { }
            //}
            //else if (App.Config.MainConfig.Server != null)
            //{
            //    serverInfoControl.SetServerInfo(App.Config.MainConfig.Server);
            //}

            if (App.Config.MainConfig.Customize.CustomBackGroundMusic)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(App.Config.MainConfigPath), "bgmusic_?.mp3");
                if (files.Count() != 0)
                {
                    Random random = new Random();
                    MediaSource = files[random.Next(files.Count())];
                    Volume = 0;
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            for (int i = 0; i < 50; i++)
                            {
                                Volume += 0.01;
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception)
                        {
                            //可忽略的错误
                        }
                    });
                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
