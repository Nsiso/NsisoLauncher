using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MicrosoftLogin;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.MicrosoftLogin.Modules;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// OauthLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OauthLoginWindow : MetroWindow, INotifyPropertyChanged
    {
        /// <summary>
        /// The uri of the web view
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// The progress of the auth
        /// </summary>
        public string Progress { get; set; } = "初始化";

        public Visibility WebBrowserVisibility { get; set; }

        public OAuthFlow OAuthFlower { get; set; }
        public XboxliveAuth XboxliveAuther { get; set; }
        public MinecraftServices McServices { get; set; }

        public CancellationToken CancelToken { get; set; } = default;

        public MicrosoftUser LoggedInUser { get; set; }

        public OauthLoginWindow(NetRequester requester)
        {
            InitializeComponent();
            this.DataContext = this;

            OAuthFlower = new OAuthFlow(requester);
            XboxliveAuther = new XboxliveAuth(requester);
            McServices = new MinecraftServices(requester);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task Login()
        {
            //wb.Source = OAuthFlower.GetAuthorizeUri();
            //this.ShowLoading();
            //this.ShowDialog();
            await OAuthFlower.Login(CancelToken);
            this.ShowDialog();
        }

        public async Task<MinecraftToken> RefreshMinecraftToken(MicrosoftToken token)
        {
            this.Progress = "MicrosoftCodeToAccessToken";
            var result = await OAuthFlower.RefreshMicrosoftAccessToken(token, CancelToken);

            this.Progress = "XboxliveAuther.Authenticate";
            var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);

            this.Progress = "McServices.Authenticate";
            var mc_result = await McServices.Authenticate(xbox_result, CancelToken);

            return mc_result;
           
        }

        private async void wb_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Uri uri = e.Uri;
            this.Uri = e.Uri.ToString();
            await NavigatingUriHandle(uri);
        }

        private void wb_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Uri uri = e.Uri;
            if (uri == OAuthFlower.GetAuthorizeUri())
            {
                HideLoading();
            }
        }

        private async Task NavigatingUriHandle(Uri uri)
        {
            if (uri.Host == OAuthFlower.RedirectUri.Host && uri.AbsolutePath == OAuthFlower.RedirectUri.AbsolutePath)
            {
                ShowLoading();
                string code = OAuthFlower.RedirectUrlToAuthCode(uri);
                await Authenticate(code);
            }
        }

        private async Task Authenticate(string code)
        {
            try
            {
                this.Progress = "MicrosoftCodeToAccessToken";
                var result = await OAuthFlower.MicrosoftCodeToAccessToken(code, CancelToken);

                this.Progress = "XboxliveAuther.Authenticate";
                var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);

                this.Progress = "McServices.Authenticate";
                var mc_result = await McServices.Authenticate(xbox_result, CancelToken);

                this.Progress = "McServices.CheckHaveGameOwnership";
                var owner_result = await McServices.CheckHaveGameOwnership(mc_result, CancelToken);

                if (owner_result)
                {
                    this.Progress = "McServices.GetProfile";
                    MicrosoftUser microsoftUser = await McServices.GetProfile(result, mc_result, CancelToken);
                    this.LoggedInUser = microsoftUser;
                    this.Close();
                }
                else
                {
                    await this.ShowMessageAsync("您的微软账号没有拥有Minecraft正版", "请确认您微软账号中购买了Minecraft正版，并拥有完整游戏权限");
                }

                Console.WriteLine("{0}\n{1}", mc_result.AccessToken, owner_result);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("登录发生异常", ex.ToString());
            }
        }

        private async Task<MinecraftToken> Refresh(MicrosoftToken token)
        {
            this.Progress = "MicrosoftCodeToAccessToken";
            var result = await OAuthFlower.RefreshMicrosoftAccessToken(token, CancelToken);

            this.Progress = "XboxliveAuther.Authenticate";
            var xbox_result = await XboxliveAuther.Authenticate(result, CancelToken);

            this.Progress = "McServices.Authenticate";
            var mc_result = await McServices.Authenticate(xbox_result, CancelToken);

            return mc_result;
        }

        private void ShowLoading()
        {
            wb.Visibility = Visibility.Hidden;
            this.progress.IsActive = true;
            this.loadingPanel.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            wb.Visibility = Visibility.Visible;
            this.progress.IsActive = false;
            this.loadingPanel.Visibility = Visibility.Collapsed;
        }
    }
}
