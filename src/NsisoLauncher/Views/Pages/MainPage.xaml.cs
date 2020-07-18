using System.Windows.Controls;
using NsisoLauncher.ViewModels.Pages;
using NsisoLauncher.ViewModels.Windows;

namespace NsisoLauncher.Views.Pages
{
    /// <summary>
    ///     MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage(MainWindowViewModel mainWindowVM)
        {
            InitializeComponent();
            var vm = new MainPageViewModel(mainWindowVM);
            DataContext = vm;
        }
    }
}