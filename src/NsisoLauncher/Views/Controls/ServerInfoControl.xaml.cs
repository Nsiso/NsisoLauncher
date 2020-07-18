using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using NsisoLauncher.Config;
using NsisoLauncherCore.Net.Server;
using Brushes = System.Windows.Media.Brushes;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    ///     ServerInfoControl.xaml 的交互逻辑
    /// </summary>
    public partial class ServerInfoControl : UserControl
    {
        public ServerInfoControl()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
        }

        public async void SetServerInfo(Server server)
        {
            if (server.ShowServerInfo)
            {
                serverNameTextBlock.Text = server.ServerName;
                serverStateIcon.Foreground = Brushes.White;
                serverStateIcon.Kind = PackIconFontAwesomeKind.SyncAltSolid;
                serverPeopleTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingNum");
                serverVersionTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingVer");
                serverPingTextBlock.Text = App.GetResourceString("String.Mainwindow.ServerGettingPing");
                serverMotdTextBlock.Text = null;
                Visibility = Visibility.Visible;
                serverLoadingBar.Visibility = Visibility.Visible;
                serverLoadingBar.IsIndeterminate = true;


                var serverInfo = new ServerInfo(server.Address, server.Port);
                await serverInfo.StartGetServerInfoAsync();

                App.LogHandler.AppendDebug(serverInfo.JsonResult);
                serverLoadingBar.IsIndeterminate = false;
                serverLoadingBar.Visibility = Visibility.Hidden;

                switch (serverInfo.State)
                {
                    case ServerInfo.StateType.GOOD:
                        serverStateIcon.Kind = PackIconFontAwesomeKind.CheckCircleSolid;
                        serverStateIcon.Foreground = Brushes.Green;
                        serverPeopleTextBlock.Text = string.Format("人数:[{0}/{1}]", serverInfo.CurrentPlayerCount,
                            serverInfo.MaxPlayerCount);
                        serverVersionTextBlock.Text = serverInfo.GameVersion;
                        serverVersionTextBlock.ToolTip = serverInfo.GameVersion;
                        serverPingTextBlock.Text = string.Format("延迟:{0}ms", serverInfo.Ping);
                        serverMotdTextBlock.Text = serverInfo.MOTD;
                        serverMotdTextBlock.ToolTip = serverInfo.MOTD;
                        if (serverInfo.OnlinePlayersName != null)
                            serverPeopleTextBlock.ToolTip = string.Join("\n", serverInfo.OnlinePlayersName);
                        if (serverInfo.IconData != null)
                            using (var ms = new MemoryStream(serverInfo.IconData))
                            {
                                serverIcon.Fill = new ImageBrush(ChangeBitmapToImageSource(new Bitmap(ms)));
                            }

                        break;

                    case ServerInfo.StateType.NO_RESPONSE:
                        serverStateIcon.Kind = PackIconFontAwesomeKind.ExclamationCircleSolid;
                        serverStateIcon.Foreground = Brushes.Red;
                        serverPeopleTextBlock.Text = "获取失败";
                        serverVersionTextBlock.Text = "获取失败";
                        serverPingTextBlock.Text = "获取失败";
                        serverMotdTextBlock.Text = "服务器没有响应，可能网络或服务器发生异常";
                        break;

                    case ServerInfo.StateType.BAD_CONNECT:
                        serverStateIcon.Kind = PackIconFontAwesomeKind.ExclamationCircleSolid;
                        serverStateIcon.Foreground = Brushes.Red;
                        serverPeopleTextBlock.Text = "获取失败";
                        serverVersionTextBlock.Text = "获取失败";
                        serverPingTextBlock.Text = "获取失败";
                        serverMotdTextBlock.Text = "服务器连接失败，服务器可能不存在或网络异常";
                        break;

                    case ServerInfo.StateType.EXCEPTION:
                        serverStateIcon.Kind = PackIconFontAwesomeKind.ExclamationCircleSolid;
                        serverStateIcon.Foreground = Brushes.Red;
                        serverPeopleTextBlock.Text = "获取失败";
                        serverVersionTextBlock.Text = "获取失败";
                        serverPingTextBlock.Text = "获取失败";
                        serverMotdTextBlock.Text = "启动器获取服务器信息时发生内部异常";
                        break;
                }
            }
            else
            {
                Visibility = Visibility.Hidden;
            }
        }

        #region 图形处理

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        ///     从bitmap转换成ImageSource
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        private static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            //Bitmap bitmap = icon.ToBitmap();  
            var hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap)) throw new Win32Exception();
            return wpfBitmap;
        }

        #endregion
    }
}