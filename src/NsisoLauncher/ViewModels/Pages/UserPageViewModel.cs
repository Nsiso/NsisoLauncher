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
                if (!string.IsNullOrEmpty(User.SelectedUserUuid))
                {
                    this.LoggedInUser = User.SelectedUser;
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
                    UserData = new UserData() { ID = userId, Username = username }
                };
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
            if (mojangLoginDResult == null)
            {
                return;
            }
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

                switch (authResult.State)
                {
                    case AuthState.SUCCESS:
                        #region 验证成功

                        #region 无选中角色处理
                        if (authResult.Profiles == null || authResult.Profiles.Count == 0)
                        {
                            await MainWindowVM.ShowMessageAsync("警告：您没有可用的游戏角色（Profile）",
                            "您可能还未购买游戏本体，请联系Mojang客服");
                            return;
                        }
                        #endregion

                        UserNode userNode = new UserNode()
                        {
                            SelectedProfileUuid = authResult.SelectedProfile?.Value,
                            AccessToken = authResult.AccessToken,
                            UserData = authResult.UserData,
                            AuthModule = "online",
                            Username = mojangLoginDResult.Username
                        };
                        foreach (var item in authResult.Profiles)
                        {
                            userNode.Profiles.Add(item.Value, item);
                        }
                        Login(userNode);
                        #endregion
                        break;

                    case AuthState.ERR_INVALID_CRDL:
                        #region 错误的验证信息/禁止访问
                        await MainWindowVM.ShowMessageAsync("验证失败：您的登录账号或密码错误",
                            string.Format("请您确认您输入的账号密码正确。也有可能是因为验证请求频率过高，被服务器暂时禁止访问。具体信息：{0}", authResult.Error.ErrorMessage));
                        #endregion
                        break;

                    case AuthState.ERR_NOTFOUND:
                        #region 页面未找到
                        await MainWindowVM.ShowMessageAsync("验证失败：Mojang验证Uri不存在",
                        string.Format("Mojang的验证URI返回404，请联系Mojang客服或启动器开发者寻求帮助，开发者请检查验证URI是否可用。具体信息：{0}",
                        authResult.Error.ErrorMessage));
                        #endregion
                        break;

                    case AuthState.ERR_METHOD_NOT_ALLOW:
                        #region 错误的验证信息/禁止访问
                        await MainWindowVM.ShowMessageAsync("验证失败：方法不允许",
                            string.Format("可能使用了错误的验证Uri或提交了post之外的内容，也有可能是因为验证请求频率过高，被服务器暂时禁止访问。具体信息：{0}",
                            authResult.Error.ErrorMessage));
                        #endregion
                        break;

                    case AuthState.ERR_OTHER:
                        #region 其他错误
                        await MainWindowVM.ShowMessageAsync("验证失败：其他错误",
                            string.Format("具体信息：{0}", authResult.Error.ErrorMessage));
                        #endregion
                        break;

                    case AuthState.ERR_INSIDE:
                        #region 内部错误
                        if (authResult.Error.Exception != null)
                        {
                            App.LogHandler.AppendFatal(authResult.Error.Exception);
                        }
                        else
                        {
                            await MainWindowVM.ShowMessageAsync("验证失败：启动器内部错误(无exception对象)",
                                string.Format("建议您联系启动器开发者进行解决。具体信息：{0}：\n\r{1}",
                                authResult.Error.ErrorMessage, authResult.Error.Exception?.ToString()));
                        }
                        #endregion
                        break;

                    default:
                        #region default
                        await MainWindowVM.ShowMessageAsync("验证失败：未知错误",
                            "建议您联系启动器开发者进行解决。");
                        #endregion
                        break;
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
            User.SelectedUserUuid = null;
            LoggedInUser = null;
        }

        private void Login(UserNode user)
        {
            if (user.UserData == null || string.IsNullOrWhiteSpace(user.UserData.ID))
            {
                throw new ArgumentNullException("User's UserData is null or empty");
            }
            if (!User.UserDatabase.ContainsKey(user.UserData.ID))
            {
                User.UserDatabase.Add(user.UserData.ID, user);
            }
            else
            {
                User.UserDatabase[user.UserData.ID] = user;
            }
            LoggedInUser = user;
            User.SelectedUserUuid = user.UserData.ID;
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
