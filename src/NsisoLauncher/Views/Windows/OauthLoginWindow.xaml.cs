using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.MicrosoftLogin;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// OauthLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OauthLoginWindow : Window
    {
        public Visibility WebBrowserVisibility { get; set; }

        public OAuthFlow OAuthFlower { get; set; }
        public XboxliveAuth XboxliveAuther { get; set; }
        public MinecraftServices McServices { get; set; }

        public CancellationToken CancelToken { get; set; } = default;

        public OauthLoginWindow(NetRequester requester)
        {
            InitializeComponent();
            this.DataContext = this;

            OAuthFlower = new OAuthFlow(requester);
            XboxliveAuther = new XboxliveAuth(requester);
            McServices = new MinecraftServices(requester);

        }

        public void ShowLogin()
        {
            wb.Source = OAuthFlower.GetAuthorizeUri();
            this.ShowLoading();
            this.ShowDialog();
        }

        private async void wb_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Uri uri = e.Uri;
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
            var result = await OAuthFlower.MicrosoftCodeToAccessToken(code, CancelToken);
            var xbox_result = await XboxliveAuther.Authenticate(result.Access_token, CancelToken);
            var mc_result = await McServices.Authenticate(xbox_result, CancelToken);
            var owner_result = await McServices.CheckHaveGameOwnership(mc_result, CancelToken);
            Console.WriteLine("{0}\n{1}", mc_result.AccessToken, owner_result);
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
