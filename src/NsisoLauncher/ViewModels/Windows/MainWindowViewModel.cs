using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// 启动中信号
        /// </summary>
        public bool IsLaunching { get; set; }

        /// <summary>
        /// 窗口高
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 窗口宽
        /// </summary>
        public double Width { get; set; }

        #region ElementsState

        public double Volume { get; set; } = 0.5;

        public string ImageSource { get; set; } = "../../Resource/home-hero-1200x600.jpg";

        public string MediaSource { get; set; }

        public bool IsPlaying { get; set; } = true;
        #endregion

        public MainWindowViewModel()
        {
            this.instance = DialogCoordinator.Instance;

            if (App.Handler != null)
            {
                App.Handler.GameExit += Handler_GameExit;

            }
            if (App.Config?.MainConfig != null)
            {
                CustomizeRefresh();

                if (App.Config.MainConfig.Launcher?.LauncherWindowSize != null)
                {
                    Height = App.Config.MainConfig.Launcher.LauncherWindowSize.Height;
                    Width = App.Config.MainConfig.Launcher.LauncherWindowSize.Width;
                }


                App.Config.MainConfig.Customize.PropertyChanged += Customize_PropertyChanged;
            }
            if (App.LaunchSignal != null)
            {
                App.LaunchSignal.PropertyChanged += LaunchSignal_PropertyChanged;
                this.IsLaunching = App.LaunchSignal.IsLaunching;
            }
        }

        private void LaunchSignal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLaunching")
            {
                this.IsLaunching = App.LaunchSignal.IsLaunching;
            }
        }

        private void Customize_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CustomizeRefresh();
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
                var result = await ShowMessageAsync("游戏非正常退出",
                    string.Format("这很有可能是因为游戏崩溃导致的，退出代码:{0}，游戏持续时间:{1}，查看崩溃前最后日志？",
                    arg.ExitCode, arg.Duration), MessageDialogStyle.AffirmativeAndNegative, null);
                switch (result)
                {
                    case MessageDialogResult.Negative:
                        break;
                    case MessageDialogResult.Affirmative:
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            DebugWindow debugWindow = new DebugWindow();
                            debugWindow.Show();
                            while (arg.Instance.LatestLogQueue.Count > 0)
                            {
                                debugWindow.AppendLog(this, new Log(LogLevel.GAME, arg.Instance.LatestLogQueue.Dequeue()));
                            }
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
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

        private void CustomizeRefresh()
        {
            //自定义窗口标题
            if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.LauncherTitle))
            {
                this.WindowTitle = App.Config.MainConfig.Customize.LauncherTitle;
            }

            //自定义窗口背景图片
            if (App.Config.MainConfig.Customize.CustomBackGroundPicture)
            {
                if (Directory.Exists(PathManager.ImageDirectory))
                {
                    string[] files = Directory.GetFiles(PathManager.ImageDirectory);
                    if (files.Count() != 0)
                    {
                        Random random = new Random();
                        ImageSource = files[random.Next(files.Count())];
                    }
                }
            }
            else
            {
                ImageSource = "../../Resource/home-hero-1200x600.jpg";
            }

            //自定义背景视频（覆盖音乐设置）
            if (App.Config.MainConfig.Customize.CustomBackGroundVideo)
            {
                if (Directory.Exists(PathManager.VideoDirectory))
                {
                    string[] files = Directory.GetFiles(PathManager.VideoDirectory);
                    if (files.Count() != 0)
                    {
                        Random random = new Random();
                        MediaSource = files[random.Next(files.Count())];
                        Volume = 0.5;
                    }
                }
            }
            //自定义背景音乐
            else if (App.Config.MainConfig.Customize.CustomBackGroundMusic)
            {
                if (Directory.Exists(PathManager.MusicDirectory))
                {
                    string[] files = Directory.GetFiles(PathManager.MusicDirectory);
                    if (files.Count() != 0)
                    {
                        Random random = new Random();
                        MediaSource = files[random.Next(files.Count())];
                        Volume = 0.5;
                    }
                }
            }
            else
            {
                MediaSource = null;
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


        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
