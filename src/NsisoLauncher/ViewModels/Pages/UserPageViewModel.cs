using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.Net.MicrosoftLogin;
using NsisoLauncherCore.Authenticator;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using User = NsisoLauncher.Config.User;

namespace NsisoLauncher.ViewModels.Pages
{
    public class UserPageViewModel : INotifyPropertyChanged
    {
        public Windows.MainWindowViewModel MainWindowVM { get; set; }

        public ICommand LoginCmd { get; set; }

        public ICommand LogoutCmd { get; set; }

        public ICommand AddAuthNodeCmd { get; set; }

        public ICommand GoAccountManagementCmd { get; set; }

        public ICommand ChangeSkinCmd { get; set; }

        public User User { get; set; }

        public bool IsLoggedIn { get => User?.SelectedAuthenticator?.SelectedUser != null; }

        public SolidColorBrush StateColor
        {
            get
            {
                if (User?.SelectedAuthenticator?.IsOnline != null && User.SelectedAuthenticator.IsOnline)
                {
                    return Brushes.LawnGreen;
                }
                else
                {
                    return Brushes.Gray;
                }
            }
        }

        #region 多语言支持变量
        private LoginDialogSettings loginDialogSettings = new LoginDialogSettings()
        {
            NegativeButtonText = App.GetResourceString("String.Base.Cancel"),
            AffirmativeButtonText = App.GetResourceString("String.Base.Login"),
            RememberCheckBoxText = App.GetResourceString("String.Base.ShouldRememberLogin"),
            UsernameWatermark = App.GetResourceString("String.Base.Username"),
            RememberCheckBoxVisibility = Visibility.Visible,
            EnablePasswordPreview = true,
            PasswordWatermark = App.GetResourceString("String.Base.Password"),
            NegativeButtonVisibility = Visibility.Visible
        };
        #endregion

        public UserPageViewModel()
        {
            if (App.MainWindowVM != null)
            {
                MainWindowVM = App.MainWindowVM;
            }

            this.PropertyChanged += UserPageViewModel_PropertyChanged;

            User = App.Config?.MainConfig?.User;

            LoginCmd = new DelegateCommand(async (a) =>
            {
                await Login();
            });

            LogoutCmd = new DelegateCommand(async (a) =>
            {
                await Logout();
            });

            // AddAuthNodeCmd = new DelegateCommand((a) =>
            // {
            //     AuthNodeWindow nodeWindow = new AuthNodeWindow();
            //     nodeWindow.Show();
            // });

            // ChangeSkinCmd = new DelegateCommand((a) =>
            // {
            //     Process.Start("https://www.minecraft.net/zh-hans/profile/skin");
            // });

            // GoAccountManagementCmd = new DelegateCommand((a) =>
            // {
            //     Process.Start("https://www.minecraft.net/zh-hans/profile");
            // });
        }

        private void UserPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private async Task Login()
        {
            AuthenticateResult result = await User?.SelectedAuthenticator?.AuthenticateAsync(default);
            if (result.IsSuccess)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
            else
            {
                await MainWindowVM.ShowMessageAsync(string.Format("登录失败:{0}", result.ErrorTag), result.ErrorMessage);
            }
        }

        private async Task Logout()
        {
            IAuthenticator authenticator = User?.SelectedAuthenticator;
            if (authenticator != null && authenticator.AllowInvalidate)
            {
                await authenticator.InvalidateAsync(default);
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
