using System.Windows.Controls;

namespace NsisoLauncher.Views.Pages
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage(ViewModels.Windows.MainWindowViewModel mainWindowVM)
        {
            InitializeComponent();
            ViewModels.Pages.MainPageViewModel vm = new ViewModels.Pages.MainPageViewModel(mainWindowVM);
            this.DataContext = vm;
        }
    }
}
