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
using NsisoLauncherCore.User;

namespace NsisoLauncher.ViewModels.Pages
{
    public class UserPageViewModel : INotifyPropertyChanged
    {
        public Windows.MainWindowViewModel MainWindowVM { get; set; }

        public ObservableDictionary<string, AuthenticationNode> AuthenticationDic { get; set; }

        /// <summary>
        /// 选中的验证模型节点
        /// </summary>
        public AuthenticationNode SelectedAuthenticationNode
        {
            get
            {
                if (!string.IsNullOrEmpty(SelectedAuthenticationNodeId) && App.Config?.MainConfig?.User?.AuthenticationDic.ContainsKey(SelectedAuthenticationNodeId) == true)
                {
                    return App.Config?.MainConfig?.User?.AuthenticationDic[SelectedAuthenticationNodeId];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 选中的验证模型节点
        /// </summary>
        public string SelectedAuthenticationNodeId { get; set; }

        /// <summary>
        /// 输入的用户名
        /// </summary>
        public string InputUsername { get; set; }

        /// <summary>
        /// 输入的密码
        /// </summary>
        public string InputPassword { get; set; }

        /// <summary>
        /// 选中记住密码
        /// </summary>
        public bool SelectedRememberPassword { get; set; }

        /// <summary>
        /// 选中是否自动登录
        /// </summary>
        public bool SelectedAutoLogin { get; set; }

        public bool IsLoggedIn { get; set; } = false;
        public string LoggedInUsername { get; set; }
        public UserNode LoggedInUser { get; set; }
        //public BitmapImage SkinImage { get; set; } /*= new Uri("/NsisoLauncher;component/Resource/PlayerSkins/steve.png", UriKind.Relative);*/
        //public ObservableCollection<ISkin> Skins { get; set; } = new ObservableCollection<ISkin>();
        public Brush StateColor { get; set; } = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        public string State { get; set; }
        public string AuthName { get; set; }

        #region YggdrasilUser
        public bool IsYggdrasil { get; set; }
        #endregion


        public ICommand LoginCmd { get; set; }

        public ICommand LogoutCmd { get; set; }

        public ICommand AddAuthNodeCmd { get; set; }

        public ICommand GoAccountManagementCmd { get; set; }

        public ICommand ChangeSkinCmd { get; set; }

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
            AuthenticationDic = App.Config?.MainConfig?.User?.AuthenticationDic;

            if (User != null)
            {
                if (!string.IsNullOrEmpty(User.SelectedUserUuid))
                {
                    this.LoggedInUser = User.SelectedUser;
                }
            }

            LoginCmd = new DelegateCommand(async (a) =>
            {
                await Login();
            });

            LogoutCmd = new DelegateCommand(async (a) =>
           {
               await Logout();
           });

            AddAuthNodeCmd = new DelegateCommand((a) =>
            {
                AuthNodeWindow nodeWindow = new AuthNodeWindow();
                nodeWindow.Show();
            });

            ChangeSkinCmd = new DelegateCommand((a) =>
            {
                Process.Start("https://www.minecraft.net/zh-hans/profile/skin");
            });

            GoAccountManagementCmd = new DelegateCommand((a) =>
            {
                Process.Start("https://www.minecraft.net/zh-hans/profile");
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
                    LoggedInUsername = LoggedInUser.User.DisplayUsername;
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
                    LoggedInUsername = null;
                    State = null;
                    AuthName = null;
                    StateColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
            }
        }

        private async Task Login()
        {
            if (SelectedAuthenticationNode == null)
            {
                await App.MainWindowVM.ShowMessageAsync("您没有选择登录类型", "请在登录类型选择框中选择你要进行登录的版本。");
                return;
            }
            switch (SelectedAuthenticationNode.AuthType)
            {
                case AuthenticationType.OFFLINE:
                    await OfflineLogin();
                    break;
                case AuthenticationType.MOJANG:
                    await YggdrasilLogin();
                    break;
                case AuthenticationType.NIDE8:
                    await YggdrasilLogin();
                    break;
                case AuthenticationType.AUTHLIB_INJECTOR:
                    await YggdrasilLogin();
                    break;
                case AuthenticationType.CUSTOM_SERVER:
                    await YggdrasilLogin();
                    break;
                case AuthenticationType.MICROSOFT:
                    MicrosoftLogin();
                    break;
                default:
                    break;
            }
        }

        private async Task OfflineLogin()
        {
            string username = InputUsername;
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }
            string real_username = username + "@offline";

            IEnumerable<UserNode> matchUsers = User.UserDatabase.Values.Where(
                x => x.User is YggdrasilUser user &&
                (user.Username == real_username) &&
                (x.AuthModule == "offline"));

            OfflineAuthenticator offlineAuthenticator;

            if (matchUsers == null || matchUsers.Count() == 0)
            {
                //不存在用户，新建用户
                offlineAuthenticator = new OfflineAuthenticator(real_username, username);
            }
            else
            {
                UserNode firstMatchUsrNode = matchUsers.First();
                var dialogResult = await App.MainWindowVM.ShowMessageAsync("输入的用户名已存在", "是否使用原有的用户登录？", MessageDialogStyle.AffirmativeAndNegative);
                if (dialogResult != MessageDialogResult.Affirmative)
                {
                    return;
                }
                offlineAuthenticator = new OfflineAuthenticator((YggdrasilUser)firstMatchUsrNode.User);
            }

            AuthenticateResponse authResult = await offlineAuthenticator.Authenticate(null);

            UserNode userNode = new UserNode()
            {
                AuthModule = SelectedAuthenticationNodeId,
                User = offlineAuthenticator.User
            };

            LoginNode(userNode);



            //string uuidValue = Guid.NewGuid().ToString();
            //string userId = Guid.NewGuid().ToString();
            //YggdrasilUser user = new YggdrasilUser()
            //{
            //    Username = real_username,
            //    AccessToken = Guid.NewGuid().ToString(),
            //    Profiles = new Dictionary<string, PlayerProfile>() { { uuidValue, new PlayerProfile() { PlayerName = username, Id = uuidValue } } },
            //    SelectedProfileUuid = uuidValue,
            //    UserData = new UserData() { ID = userId, Username = username }
            //};
            //UserNode userNode = new UserNode()
            //{
            //    AuthModule = "offline",
            //    User = user
            //};
            //LoginNode(userNode);
        }

        private async Task MicrosoftLogin()
        {
            MicrosoftAuthenticator authenticator = new MicrosoftAuthenticator();
            var result = await authenticator.LoginGetMinecraftToken();
            if (loginWindow.LoggedInUser != null)
            {
                UserNode node = new UserNode();
                node.AuthModule = "microsoft";
                node.User = loginWindow.LoggedInUser;
                LoginNode(node);

                ////设置皮肤
                //Skins.Clear();
                //if (loginWindow.LoggedInUser.Skins != null)
                //{
                //    foreach (var item in loginWindow.LoggedInUser.Skins)
                //    {
                //        Skins.Add(item);
                //    }
                //}

            }
            else
            {
                //todo 提醒用户登录未成功
            }
        }

        private async Task YggdrasilLogin()
        {
            if (SelectedAuthenticationNode == null)
            {
                await App.MainWindowVM.ShowMessageAsync("您没有选择登录类型", "请在登录类型选择框中选择你要进行登录的版本。");
                return;
            }
            AuthenticationNode selectedAuthNode = SelectedAuthenticationNode;
            string selectedAuthNodeId = SelectedAuthenticationNodeId;
            YggdrasilAuthenticator authenticator = null;
            switch (selectedAuthNode.AuthType)
            {
                case AuthenticationType.MOJANG:
                    authenticator = new MojangAuthenticator();
                    break;
                case AuthenticationType.NIDE8:
                    authenticator = new Nide8Authenticator(selectedAuthNode.Property["nide8ID"]);
                    break;
                case AuthenticationType.AUTHLIB_INJECTOR:
                    authenticator = new AuthlibInjectorAuthenticator(selectedAuthNode.Property["authserver"]);
                    break;
                case AuthenticationType.CUSTOM_SERVER:
                    authenticator = new YggdrasilAuthenticator(selectedAuthNode.Property["authserver"]);
                    break;
                default:
                    throw new Exception("Using yggdrasil authenticator but the auth node auth type is not yggdrasil.");
            }

            string username = InputUsername;

            bool older_success = false;
            var older = App.Config.MainConfig.User.UserDatabase.FirstOrDefault(x => x.Value.User is YggdrasilUser yggdrasilUser && x.Value.AuthModule == selectedAuthNodeId && yggdrasilUser.Username == username);


            if (!string.IsNullOrEmpty(older.Key) && older.Value != null && !string.IsNullOrEmpty(older.Value.User.LaunchAccessToken))
            {
                AccessClientTokenPair tokens = new AccessClientTokenPair()
                {
                    AccessToken = older.Value.User.LaunchAccessToken,
                    ClientToken = App.Config.MainConfig.User.ClientToken
                };
                string refreshTitle = string.Format("正在进行{0}登录中（重新登录）...", selectedAuthNode.Name);
                string refreshMsg = "这需要联网进行操作，可能需要一分钟的时间";

                CancellationTokenSource olderCancellationSource = new CancellationTokenSource();
                var progress = await MainWindowVM.ShowProgressAsync(refreshTitle, refreshMsg, true, null);
                progress.Canceled += (obj, arg) =>
                {
                    olderCancellationSource.Cancel();
                };

                var result = await authenticator.Validate(tokens, olderCancellationSource.Token);
                if (result.IsSuccess)
                {
                    older_success = true;
                }
                else
                {
                    var refresh_result = await authenticator.Refresh(new RefreshRequest(tokens), olderCancellationSource.Token);
                    if (refresh_result.IsSuccess)
                    {
                        if (older.Value.User is YggdrasilUser ygg)
                        {
                            ygg.AccessToken = refresh_result.Data.AccessToken;
                            older_success = true;
                        }
                    }
                }

                await progress.CloseAsync();

                if (older_success)
                {
                    LoginNode(older.Value);
                    return;
                }
            }

            string password = InputPassword;
            bool rememberPassword = SelectedRememberPassword;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                return;
            }

            string currentLoginType = string.Format("正在进行{0}登录中...", selectedAuthNode.Name);
            string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";

            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            var loader = await MainWindowVM.ShowProgressAsync(currentLoginType, loginMsg, true, null);
            loader.Canceled += (obj, arg) =>
            {
                cancellationSource.Cancel();
            };

            loader.SetIndeterminate();
            AuthenticateResponse authResult = await authenticator.Authenticate(
                new AuthenticateRequest(username, password, App.Config.MainConfig.User.ClientToken), cancellationSource.Token);
            await loader.CloseAsync();

            if (authResult.IsSuccess)
            {
                #region 验证成功

                #region 无选中角色处理
                if (authResult.Data.AvailableProfiles == null || authResult.Data.AvailableProfiles.Count == 0)
                {
                    await MainWindowVM.ShowMessageAsync("警告：您没有可用的游戏角色（Profile）",
                    "您可能还未购买游戏本体，请联系Mojang客服");
                    return;
                }
                #endregion

                YggdrasilUser user;
                if (App.Config.MainConfig.User.UserDatabase.ContainsKey(authResult.Data.User.ID))
                {
                    user = (YggdrasilUser)App.Config.MainConfig.User.UserDatabase[authResult.Data.User.ID].User;
                    user.SelectedProfileUuid = authResult.Data.SelectedProfile?.Id;
                    user.AccessToken = authResult.Data.AccessToken;
                    user.UserData = authResult.Data.User;
                    user.Username = username;
                }
                else
                {
                    user = new YggdrasilUser()
                    {
                        SelectedProfileUuid = authResult.Data.SelectedProfile?.Id,
                        AccessToken = authResult.Data.AccessToken,
                        UserData = authResult.Data.User,
                        Username = username
                    };
                }
                user.Profiles.Clear();
                foreach (var item in authResult.Data.AvailableProfiles)
                {
                    user.Profiles.Add(item.Id, item);
                }

                UserNode userNode = new UserNode()
                {
                    AuthModule = selectedAuthNodeId,
                    User = user
                };

                LoginNode(userNode);
                #endregion
            }
            else
            {
            }

            switch (authResult.State)
            {
                case ResponseState.SUCCESS:

                    break;

                case ResponseState.ERR_INVALID_CRDL:
                    #region 错误的验证信息/禁止访问
                    await MainWindowVM.ShowMessageAsync("验证失败：您的登录账号或密码错误",
                        string.Format("请您确认您输入的账号密码正确。也有可能是因为验证请求频率过高，被服务器暂时禁止访问。具体信息：{0}", authResult.Error.ErrorMessage));
                    #endregion
                    break;

                case ResponseState.ERR_NOTFOUND:
                    #region 页面未找到
                    await MainWindowVM.ShowMessageAsync("验证失败：验证服务器Uri不存在",
                    string.Format("验证服务器URI：{0}返回404，请联系{1}客服或启动器开发者寻求帮助，开发者请检查验证URI是否可用。具体信息：{2}",
                    authenticator.AuthServerUrl, selectedAuthNode.Name, authResult.Error?.ErrorMessage));
                    #endregion
                    break;

                case ResponseState.ERR_METHOD_NOT_ALLOW:
                    #region 错误的验证信息/禁止访问
                    await MainWindowVM.ShowMessageAsync("验证失败：方法不允许",
                        string.Format("可能使用了错误的验证服务器Uri或提交了post之外的内容，也有可能是因为验证请求频率过高，被服务器暂时禁止访问。具体信息：{0}",
                        authResult.Error.ErrorMessage));
                    #endregion
                    break;

                case ResponseState.ERR_OTHER:
                    #region 其他错误
                    await MainWindowVM.ShowMessageAsync("验证失败：其他错误",
                        string.Format("具体信息：{0}", authResult.Error.ErrorMessage));
                    #endregion
                    break;

                case ResponseState.ERR_INSIDE:
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

                case ResponseState.CANCELED:
                    #region 取消
                    await MainWindowVM.ShowMessageAsync("登录已被取消",
                            string.Format("您已经主动取消了登录过程。具体信息：{0}", authResult.Error.ErrorMessage));
                    #endregion
                    break;

                case ResponseState.ERR_TIMEOUT:
                    #region 网络超时
                    await MainWindowVM.ShowMessageAsync("登录过程中网络超时",
                            string.Format("请检查您的网络环境。具体信息：{0}", authResult.Error.ErrorMessage));
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

        private async Task Logout()
        {
            if (LoggedInUser == null)
            {
                await App.MainWindowVM.ShowMessageAsync("没有登录的用户", "这可能是因为内部异常导致的");
                return;
            }
            if (!User.AuthenticationDic.ContainsKey(LoggedInUser.AuthModule))
            {
                await App.MainWindowVM.ShowMessageAsync("不存在目前登录用户的登录模型", "这可能是因为未登录或注销前删除了登录模型");
                return;
            }
            AuthenticationNode node = User.AuthenticationDic[LoggedInUser.AuthModule];

            IAuthenticator authenticator = null;

            switch (node.AuthType)
            {
                case AuthenticationType.OFFLINE:
                    break;
                case AuthenticationType.MOJANG:

                    authenticator = new MojangAuthenticator();
                    break;
                case AuthenticationType.NIDE8:
                    authenticator = new Nide8Authenticator(node.Property["nide8ID"]);
                    break;
                case AuthenticationType.AUTHLIB_INJECTOR:
                    authenticator = new AuthlibInjectorAuthenticator(node.Property["authserver"]);
                    break;
                case AuthenticationType.CUSTOM_SERVER:
                    authenticator = new YggdrasilAuthenticator(node.Property["authserver"]);
                    break;
                case AuthenticationType.MICROSOFT:
                    break;
                default:
                    break;
            }

            if (node.AuthType != AuthenticationType.OFFLINE && authenticator != null)
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();

                string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
                var loader = await MainWindowVM.ShowProgressAsync("正在注销中...", loginMsg, true, null);

                loader.Canceled += (obj, arg) =>
                {
                    cancellationSource.Cancel();
                };

                loader.SetIndeterminate();
                await authenticator.Invalidate(new AccessClientTokenPair(LoggedInUser.User.LaunchAccessToken, App.Config.MainConfig.User.ClientToken), cancellationSource.Token);
                await loader.CloseAsync();
            }

            User.SelectedUserUuid = null;
            if (LoggedInUser.User is YggdrasilUser ygg_usr)
            {
                ygg_usr.AccessToken = null;
            }
            LoggedInUser = null;
        }

        private void LoginNode(UserNode user)
        {
            if (string.IsNullOrWhiteSpace(user.User.UserId))
            {
                throw new ArgumentNullException("User's UserData is null or empty");
            }
            if (!User.UserDatabase.ContainsKey(user.User.UserId))
            {
                User.UserDatabase.Add(user.User.UserId, user);
            }
            else
            {
                User.UserDatabase[user.User.UserId] = user;
            }
            LoggedInUser = user;
            User.SelectedUserUuid = user.User.UserId;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
