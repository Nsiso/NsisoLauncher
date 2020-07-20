using System.Windows.Controls;

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
