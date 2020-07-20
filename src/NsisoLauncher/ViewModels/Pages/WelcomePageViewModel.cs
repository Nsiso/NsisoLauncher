using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Pages;
using System.ComponentModel;

namespace NsisoLauncher.ViewModels.Pages
{
    public class WelcomePageViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel MainWindowVM { get; set; }

        public DelegateCommand LoadedCommand { get; set; }

        public WelcomePageViewModel(MainWindowViewModel mainWindowVM)
        {
            this.MainWindowVM = mainWindowVM;
            LoadedCommand = new DelegateCommand(async (a) =>
            {
                await App.RefreshVersionListAsync();
                MainPage mainPage = new MainPage(mainWindowVM);
                MainWindowVM.NavigationService.Navigate(mainPage);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
