using System.Windows.Controls;
using NsisoLauncher.ViewModels.Pages;
using NsisoLauncher.ViewModels.Windows;

namespace NsisoLauncher.Views.Pages
{
    /// <summary>
    ///     WelcomePage.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage : Page
    {
        public WelcomePage(MainWindowViewModel mainWindowVM)
        {
            InitializeComponent();
            var vm = new WelcomePageViewModel(mainWindowVM);
            DataContext = vm;
        }
    }
}