using NsisoLauncherCore.Net.MicrosoftLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// OauthLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OauthLoginWindow : Window
    {
        public OAuthFlow OAuthFlower { get; set; }
        public XboxliveAuth XboxliveAuther { get; set; }

        public CancellationToken CancelToken { get; set; } = default;

        public OauthLoginWindow()
        {
            InitializeComponent();

            wb.Navigating += Wb_Navigating;
            OAuthFlower = new OAuthFlow(App.NetHandler.Requester);
            XboxliveAuther = new XboxliveAuth(App.NetHandler.Requester);
            wb.Source = OAuthFlower.GetAuthorizeUri();
        }

        private async void Wb_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Uri uri = e.Uri;
            if (uri.Host == OAuthFlower.RedirectUri.Host && uri.AbsolutePath == OAuthFlower.RedirectUri.AbsolutePath)
            {
                string code = OAuthFlower.RedirectUrlToAuthCode(uri);
                var result = await OAuthFlower.MicrosoftCodeToAccessToken(code, CancelToken);
                //var xbox_result = await XboxliveAuther.Authenticate(result.Access_token, CancelToken);
            }
        }
    }
}
