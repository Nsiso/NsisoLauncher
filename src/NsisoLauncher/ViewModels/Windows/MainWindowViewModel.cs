using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Views.Pages;
using NsisoLauncherCore;

namespace NsisoLauncher.ViewModels.Windows
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     窗口对话
        /// </summary>
        private readonly IDialogCoordinator instance;

        public MainWindowViewModel(IDialogCoordinator instance)
        {
            this.instance = instance;
            if (App.Handler != null) App.Handler.GameExit += Handler_GameExit;
        }

        /// <summary>
        ///     窗口状态
        /// </summary>
        public WindowState WindowState { get; set; }

        /// <summary>
        ///     窗口标题
        /// </summary>
        public string WindowTitle { get; set; } = "Nsiso Launcher";

        public NavigationService NavigationService { get; set; }

        /// <summary>
        ///     关闭窗口
        /// </summary>
        public Action CloseWindow { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InitializeMainPage()
        {
            NavigationService.Navigate(new WelcomePage(this));
        }

        private async void Handler_GameExit(object sender, GameExitArg arg)
        {
            WindowState = WindowState.Normal;
            if (!arg.IsNormalExit())
                await ShowMessageAsync("游戏非正常退出",
                    string.Format("这很有可能是因为游戏崩溃导致的，退出代码:{0}，游戏持续时间:{1}",
                        arg.ExitCode, arg.Duration));
        }

        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            return await instance.ShowMessageAsync(this, title, message, style, settings);
        }

        public async Task<LoginDialogData> ShowLoginAsync(string title, string message,
            LoginDialogSettings settings = null)
        {
            return await instance.ShowLoginAsync(this, title, message, settings);
        }

        public async Task<ProgressDialogController> ShowProgressAsync(string title, string message,
            bool isCancelable = false, MetroDialogSettings settings = null)
        {
            return await instance.ShowProgressAsync(this, title, message, isCancelable, settings);
        }

        public async Task ShowMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings settings = null)
        {
            await instance.ShowMetroDialogAsync(this, dialog, settings);
        }

        public async Task HideMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings settings = null)
        {
            await instance.HideMetroDialogAsync(this, dialog, settings);
        }
    }
}