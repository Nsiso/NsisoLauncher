using NsisoLauncherCore.Net.Server;
using System.Windows;
using System.Windows.Controls;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// ServerInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class ServerInfoControl : UserControl
    {
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(ServerInfoControl), new PropertyMetadata(true));



        public ServerInfo ServerInf
        {
            get { return (ServerInfo)GetValue(ServerInfProperty); }
            set { SetValue(ServerInfProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerInf.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerInfProperty =
            DependencyProperty.Register("ServerInf", typeof(ServerInfo), typeof(ServerInfoControl), new PropertyMetadata(null));







        public ServerInfoControl()
        {
            InitializeComponent();

            //this.Visibility = Visibility.Hidden;
        }

        //public async void SetServerInfo(Server server)
        //{
        //    if (server.ShowServerInfo)
        //    {
        //        serverNameTextBlock.Text = server.ServerName;
        //        serverStateIcon.Foreground = System.Windows.Media.Brushes.White;
        //        serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SyncAltSolid;
        //        serverPeopleTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingNum");
        //        serverVersionTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingVer");
        //        serverPingTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingPing");
        //        serverMotdTextBlock.Text = null;
        //        this.Visibility = Visibility.Visible;
        //        serverLoadingBar.Visibility = Visibility.Visible;
        //        serverLoadingBar.IsIndeterminate = true;


        //        ServerInfo serverInfo = new ServerInfo(server.Address, server.Port);
        //        await serverInfo.StartGetServerInfoAsync();

        //        App.LogHandler.AppendDebug(serverInfo.JsonResult);
        //        serverLoadingBar.IsIndeterminate = false;
        //        serverLoadingBar.Visibility = Visibility.Hidden;

        //        switch (serverInfo.State)
        //        {
        //            case ServerInfo.StateType.GOOD:
        //                this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.CircleRegular;
        //                this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Green;
        //                this.serverPeopleTextBlock.Text = string.Format("人数:[{0}/{1}]", serverInfo.CurrentPlayerCount, serverInfo.MaxPlayerCount);
        //                this.serverVersionTextBlock.Text = serverInfo.GameVersion;
        //                this.serverVersionTextBlock.ToolTip = serverInfo.GameVersion;
        //                this.serverPingTextBlock.Text = string.Format("延迟:{0}ms", serverInfo.Ping);
        //                this.serverMotdTextBlock.Text = serverInfo.MOTD;
        //                this.serverMotdTextBlock.ToolTip = serverInfo.MOTD;
        //                if (serverInfo.OnlinePlayersName != null)
        //                {
        //                    this.serverPeopleTextBlock.ToolTip = string.Join("\n", serverInfo.OnlinePlayersName);
        //                }
        //                if (serverInfo.IconData != null)
        //                {
        //                    using (MemoryStream ms = new MemoryStream(serverInfo.IconData))
        //                    {
        //                        this.serverIcon.Fill = new ImageBrush(ChangeBitmapToImageSource(new Bitmap(ms)));
        //                    }

        //                }
        //                break;

        //            case ServerInfo.StateType.NO_RESPONSE:
        //                this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
        //                this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Red;
        //                this.serverPeopleTextBlock.Text = "获取失败";
        //                this.serverVersionTextBlock.Text = "获取失败";
        //                this.serverPingTextBlock.Text = "获取失败";
        //                this.serverMotdTextBlock.Text = "服务器没有响应，可能网络或服务器发生异常";
        //                break;

        //            case ServerInfo.StateType.BAD_CONNECT:
        //                this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
        //                this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Red;
        //                this.serverPeopleTextBlock.Text = "获取失败";
        //                this.serverVersionTextBlock.Text = "获取失败";
        //                this.serverPingTextBlock.Text = "获取失败";
        //                this.serverMotdTextBlock.Text = "服务器连接失败，服务器可能不存在或网络异常";
        //                break;

        //            case ServerInfo.StateType.EXCEPTION:
        //                this.serverStateIcon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.ExclamationCircleSolid;
        //                this.serverStateIcon.Foreground = System.Windows.Media.Brushes.Red;
        //                this.serverPeopleTextBlock.Text = "获取失败";
        //                this.serverVersionTextBlock.Text = "获取失败";
        //                this.serverPingTextBlock.Text = "获取失败";
        //                this.serverMotdTextBlock.Text = "启动器获取服务器信息时发生内部异常";
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        this.Visibility = Visibility.Hidden;
        //    }
        //}

        //#region 图形处理
        //[DllImport("gdi32.dll", SetLastError = true)]
        //private static extern bool DeleteObject(IntPtr hObject);

        ///// <summary>  
        ///// 从bitmap转换成ImageSource  
        ///// </summary>  
        ///// <param name="icon"></param>  
        ///// <returns></returns>  
        //private static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        //{
        //    //Bitmap bitmap = icon.ToBitmap();  
        //    IntPtr hBitmap = bitmap.GetHbitmap();

        //    ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //        hBitmap,
        //        IntPtr.Zero,
        //        Int32Rect.Empty,
        //        BitmapSizeOptions.FromEmptyOptions());
        //    if (!DeleteObject(hBitmap))
        //    {
        //        throw new System.ComponentModel.Win32Exception();
        //    }
        //    return wpfBitmap;

        //}
        //#endregion
    }
}
