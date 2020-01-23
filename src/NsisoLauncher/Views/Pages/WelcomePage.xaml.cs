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

namespace NsisoLauncher.Views.Pages
{
    /// <summary>
    /// WelcomePage.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage : Page
    {
        public WelcomePage(ViewModels.Windows.MainWindowViewModel mainWindowVM)
        {
            InitializeComponent();
            ViewModels.Pages.WelcomePageViewModel vm = new ViewModels.Pages.WelcomePageViewModel(mainWindowVM);
            this.DataContext = vm;
        }
    }
}
