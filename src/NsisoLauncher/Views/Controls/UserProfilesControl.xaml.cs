using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// UserProfilesControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserProfilesControl : UserControl
    {
        public IUser User
        {
            get { return (IUser)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for User.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(IUser), typeof(UserProfilesControl), new PropertyMetadata(null));

        public UserProfilesControl()
        {
            InitializeComponent();
        }
    }

    public class YggdrasilUserProfilesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            IUser user = (IUser)value;
            if (user is YggdrasilUser ygg)
            {
                return ygg.Profiles;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class YggdrasilUserSelectedProfileUuidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            IUser user = (IUser)value;
            if (user is YggdrasilUser ygg)
            {
                return ygg.SelectedProfileUuid;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
