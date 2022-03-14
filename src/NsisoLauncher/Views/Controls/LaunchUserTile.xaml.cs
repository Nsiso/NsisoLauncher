using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using NsisoLauncherCore.User;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// LaunchUserTile.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchUserTile : Tile
    {
        public IUser SelectedUser
        {
            get { return (IUser)GetValue(SelectedUserProperty); }
            set { SetValue(SelectedUserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedUser.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedUserProperty =
            DependencyProperty.Register("SelectedUser", typeof(IUser), typeof(LaunchUserTile), new PropertyMetadata(null));



        public LaunchUserTile()
        {
            InitializeComponent();
        }
    }
}
