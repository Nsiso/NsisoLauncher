using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;
using User = NsisoLauncher.Config.User;

namespace NsisoLauncher.ViewModels.Pages
{
    public class UserPageViewModel : INotifyPropertyChanged
    {
        public Windows.MainWindowViewModel MainWindowVM { get; set; }
        public bool IsLoggedIn { get; set; } = false;
        public string LoggedInUsername { get; set; }
        public UserNode LoggedInUser { get; set; }
        public Brush StateColor { get; set; } = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        public string State { get; set; }
        public string AuthName { get; set; }
        public Dictionary<string, PlayerProfile> UUIDList { get; set; }

        public ICommand OfflineLoginCmd { get; set; }
        public ICommand MojangLoginCmd { get; set; }
        public ICommand OtherLoginCmd { get; set; }

        public ICommand LogoutCmd { get; set; }

        public User User { get; set; }

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

            if (User != null)
            {
                if (!string.IsNullOrEmpty(User.SelectedUser))
                {
                    this.LoggedInUser = User.GetSelectedUser();
                }
            }

            OfflineLoginCmd = new DelegateCommand(async (a) =>
            {
                await OfflineLogin();
            });

            MojangLoginCmd = new DelegateCommand(async (a) =>
            {
                await MojangLogin();
            });

            LogoutCmd = new DelegateCommand((a) =>
            {
                Logout();
            });

            //LoggedInUser = new UserNode()
            //{
            //    UserName = "nsisogf@163.com",
            //    SelectProfileUUID = "888",
            //    Profiles = new Dictionary<string, Uuid>()
            //{
            //    { "666", new Uuid() { PlayerName = "siso" } },
            //    { "777", new Uuid() { PlayerName = "2号角色22" } },
            //    { "888", new Uuid() { PlayerName = "3号角色33333" } },
            //    { "999", new Uuid() { PlayerName = "4号角色44444444" } },
            //}
            //};
        }

        private void UserPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoggedInUser")
            {
                if (LoggedInUser != null)
                {
                    IsLoggedIn = true;
                    UUIDList = LoggedInUser.Profiles;
                    LoggedInUsername = LoggedInUser.Username;
                    State = "在线";
                    if (User.AuthenticationDic.ContainsKey(LoggedInUser.AuthModule))
                    {
                        AuthName = User.AuthenticationDic[LoggedInUser.AuthModule].Name;
                    }
                    else
                    {
                        AuthName = "未知";
                    }
                    StateColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                else
                {
                    IsLoggedIn = false;
                    UUIDList = null;
                    LoggedInUsername = null;
                    State = null;
                    AuthName = null;
                    StateColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
            }
        }

        private async Task OfflineLogin()
        {
            string username = await App.MainWindowVM.ShowInputAsync("输入游戏用户名", "游戏中显示的名字将会是此用户名");
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }
            IEnumerable<UserNode> matchUsers = User.UserDatabase.Values.Where(x => ((x.Username == username) && (x.AuthModule == "offline")));
            if (matchUsers?.Count() == 0)
            {
                //不存在用户新建用户
                string uuidValue = Guid.NewGuid().ToString();
                string userId = Guid.NewGuid().ToString();
                UserNode userNode = new UserNode()
                {
                    Username = username,
                    AccessToken = Guid.NewGuid().ToString(),
                    AuthModule = "offline",
                    Profiles = new Dictionary<string, PlayerProfile>() { { uuidValue, new PlayerProfile() { PlayerName = username, Value = uuidValue } } },
                    SelectedProfileUuid = uuidValue,
                    UserData = new UserData() { ID = userId, Username = username}
                };
                User.UserDatabase.Add(userId, userNode);
                Login(userNode);
            }
            else
            {
                UserNode firstMatchUsrNode = matchUsers.FirstOrDefault();
                if (firstMatchUsrNode != null)
                {
                    var dialogResult = await App.MainWindowVM.ShowMessageAsync("输入的用户名已存在", "是否使用原有的用户登录？");
                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        Login(firstMatchUsrNode);
                    }
                }
            }
        }

        private async Task MojangLogin()
        {
            var mojangLoginDResult = await App.MainWindowVM.ShowLoginAsync(App.GetResourceString("String.Mainwindow.Auth.Mojang.Login"),
                                App.GetResourceString("String.Mainwindow.Auth.Mojang.Login2"),
                                loginDialogSettings);
            if (IsValidateLoginData(mojangLoginDResult))
            {
                var mYggAuthenticator = new YggdrasilAuthenticator(new Credentials()
                {
                    Username = mojangLoginDResult.Username,
                    Password = mojangLoginDResult.Password
                });
                mYggAuthenticator.ProxyAuthServerAddress = "https://authserver.mojang.com";
                string currentLoginType = string.Format("正在进行Mojang正版登录中...");
                string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
                var loader = await MainWindowVM.ShowProgressAsync(currentLoginType, loginMsg, true, null);

                loader.SetIndeterminate();
                var authResult = await mYggAuthenticator.DoAuthenticateAsync();
                await loader.CloseAsync();

                UserNode userNode = new UserNode()
                {
                    SelectedProfileUuid = authResult.SelectedProfile.Value,
                    AccessToken = authResult.AccessToken,
                    UserData = authResult.UserData,
                    AuthModule = "Mojang",
                    Username = mojangLoginDResult.Username
                };
                userNode.Profiles = new Dictionary<string, PlayerProfile>();
                foreach (var item in authResult.Profiles)
                {
                    userNode.Profiles.Add(item.Value, item);
                }
            }
            else
            {
                await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                return;
            }
        }

        private void Logout()
        {
            if (LoggedInUser.AuthModule == "offline")
            {
            }
            else
            {
                //todo 线上注销
            }
            User.SelectedUser = null;
            LoggedInUser = null;
        }

        private void Login(UserNode user)
        {
            LoggedInUser = user;
            User.SelectedUser = user.UserData.ID;
        }

        private bool IsValidateLoginData(LoginDialogData data)
        {
            if (data == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(data.Username))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(data.Password))
            {
                return false;
            }
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
