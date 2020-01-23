using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NsisoLauncherCore.Modules;
using Version = NsisoLauncherCore.Modules.Version;
using System.Windows;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Util;
using System.IO;
using NsisoLauncher.Views.Windows;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using NsisoLauncherCore;
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

        public NavigationService NavigationService { get; set; }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public Action CloseWindow { get; set; }

        /// <summary>
        /// 窗口对话
        /// </summary>
        private IDialogCoordinator instance;

        public MainWindowViewModel(IDialogCoordinator instance)
        {
            this.instance = instance;
            if (App.Handler != null)
            {
                App.Handler.GameExit += Handler_GameExit;
            }
        }

        public void InitializeMainPage()
        {
            this.NavigationService.Navigate(new Views.Pages.WelcomePage(this));
        }

        private async void Handler_GameExit(object sender, NsisoLauncherCore.GameExitArg arg)
        {
            this.WindowState = WindowState.Normal;
            if (!arg.IsNormalExit())
            {
                await ShowMessageAsync("游戏非正常退出",
                    string.Format("这很有可能是因为游戏崩溃导致的，退出代码:{0}，游戏持续时间:{1}",
                    arg.ExitCode, arg.Duration));
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
