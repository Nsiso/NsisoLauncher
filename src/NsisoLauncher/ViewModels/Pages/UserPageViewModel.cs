using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Auth;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Modules.Yggdrasil;
using NsisoLauncherCore.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.Net.Yggdrasil;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using User = NsisoLauncher.Config.User;

namespace NsisoLauncher.ViewModels.Pages
{
    public class UserPageViewModel : INotifyPropertyChanged
    {
        public Windows.MainWindowViewModel MainWindowVM { get; set; }

        public ObservableDictionary<string, AuthenticationNode> AuthenticationDic { get; set; }

        /// <summary>
        /// 选中的验证模型节点
        /// </summary>
        public AuthenticationNode SelectedAuthenticationNode { get; set; }

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
        public Uri SkinUrl { get; set; } = new Uri("/NsisoLauncher;component/Resource/PlayerSkins/steve.png", UriKind.Relative);
        public Brush StateColor { get; set; } = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        public string State { get; set; }
        public string AuthName { get; set; }
        public Dictionary<string, PlayerProfile> UUIDList { get; set; }


        public ICommand LoginCmd { get; set; }

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
                    await MojangLogin();
                    break;
                case AuthenticationType.NIDE8:
                    await App.MainWindowVM.ShowMessageAsync("Nide8登录暂未开发", "敬请期待");
                    break;
                case AuthenticationType.AUTHLIB_INJECTOR:
                    await App.MainWindowVM.ShowMessageAsync("AAuthlib登录暂未开发", "敬请期待");
                    break;
                case AuthenticationType.CUSTOM_SERVER:
                    await App.MainWindowVM.ShowMessageAsync("自定义登录暂未开发", "敬请期待");
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
            IEnumerable<UserNode> matchUsers = User.UserDatabase.Values.Where(x => ((x.Username == real_username) && (x.AuthModule == "offline")));
            if (matchUsers?.Count() == 0)
            {
                //不存在用户新建用户
                string uuidValue = Guid.NewGuid().ToString();
                string userId = Guid.NewGuid().ToString();
                UserNode userNode = new UserNode()
                {
                    Username = real_username,
                    AccessToken = Guid.NewGuid().ToString(),
                    AuthModule = "offline",
                    Profiles = new Dictionary<string, PlayerProfile>() { { uuidValue, new PlayerProfile() { PlayerName = username, Value = uuidValue } } },
                    SelectedProfileUuid = uuidValue,
                    UserData = new UserData() { ID = userId, Username = username }
                };
                LoginNode(userNode);
            }
            else
            {
                UserNode firstMatchUsrNode = matchUsers.FirstOrDefault();
                if (firstMatchUsrNode != null)
                {
                    var dialogResult = await App.MainWindowVM.ShowMessageAsync("输入的用户名已存在", "是否使用原有的用户登录？");
                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        LoginNode(firstMatchUsrNode);
                    }
                }
            }
        }

        private void MicrosoftLogin()
        {
            Views.Windows.OauthLoginWindow loginWindow = new Views.Windows.OauthLoginWindow(App.NetHandler.Requester);
            loginWindow.ShowLogin();
        }

        private async Task MojangLogin()
        {
            string username = InputUsername;
            string password = InputPassword;
            bool rememberPassword = SelectedRememberPassword;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                await MainWindowVM.ShowMessageAsync("您输入的账号或密码为空", "请检查您是否正确填写登录信息");
                return;
            }

            var mYggAuthenticator = new MojangAuthenticator(App.NetHandler.Requester);
            string currentLoginType = string.Format("正在进行Mojang正版登录中...");
            string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
            var loader = await MainWindowVM.ShowProgressAsync(currentLoginType, loginMsg, true, null);

            loader.SetIndeterminate();
            var authResult = await mYggAuthenticator.Authenticate(
                new AuthenticateRequest(username, password, App.Config.MainConfig.User.ClientToken));
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

                UserNode userNode = new UserNode()
                {
                    SelectedProfileUuid = authResult.Data.SelectedProfile?.Value,
                    AccessToken = authResult.Data.AccessToken,
                    UserData = authResult.Data.User,
                    AuthModule = "mojang",
                    Username = username
                };
                foreach (var item in authResult.Data.AvailableProfiles)
                {
                    userNode.Profiles.Add(item.Value, item);
                }
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
                    await MainWindowVM.ShowMessageAsync("验证失败：Mojang验证Uri不存在",
                    string.Format("Mojang的验证URI返回404，请联系Mojang客服或启动器开发者寻求帮助，开发者请检查验证URI是否可用。具体信息：{0}",
                    authResult.Error.ErrorMessage));
                    #endregion
                    break;

                case ResponseState.ERR_METHOD_NOT_ALLOW:
                    #region 错误的验证信息/禁止访问
                    await MainWindowVM.ShowMessageAsync("验证失败：方法不允许",
                        string.Format("可能使用了错误的验证Uri或提交了post之外的内容，也有可能是因为验证请求频率过高，被服务器暂时禁止访问。具体信息：{0}",
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
            if (!User.AuthenticationDic.ContainsKey(LoggedInUser.AuthModule))
            {
                await App.MainWindowVM.ShowMessageAsync("不存在目前登录用户的登录模型", "这可能是因为注销前删除了登录模型");
            }
            AuthenticationNode node = User.AuthenticationDic[LoggedInUser.AuthModule];

            IAuthenticator authenticator = null;

            switch (node.AuthType)
            {
                case AuthenticationType.OFFLINE:
                    break;
                case AuthenticationType.MOJANG:

                    authenticator = new MojangAuthenticator(App.NetHandler.Requester);
                    break;
                case AuthenticationType.NIDE8:
                    authenticator = new Nide8Authenticator(App.NetHandler.Requester, node.Property["nide8ID"]);
                    break;
                case AuthenticationType.AUTHLIB_INJECTOR:
                    authenticator = new YggdrasilAuthenticator(new Uri(node.Property["authserver"]), App.NetHandler.Requester);
                    break;
                case AuthenticationType.CUSTOM_SERVER:
                    authenticator = new YggdrasilAuthenticator(new Uri(node.Property["authserver"]), App.NetHandler.Requester);
                    break;
                case AuthenticationType.MICROSOFT:
                    break;
                default:
                    break;
            }

            if (node.AuthType != AuthenticationType.OFFLINE)
            {
                string loginMsg = "这需要联网进行操作，可能需要一分钟的时间";
                var loader = await MainWindowVM.ShowProgressAsync("正在注销中...", loginMsg, true, null);

                loader.SetIndeterminate();
                await authenticator.Invalidate(new AccessClientTokenPair(LoggedInUser.AccessToken, App.Config.MainConfig.User.ClientToken));
                await loader.CloseAsync();
            }

            User.SelectedUserUuid = null;
            LoggedInUser = null;
        }

        private void LoginNode(UserNode user)
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
