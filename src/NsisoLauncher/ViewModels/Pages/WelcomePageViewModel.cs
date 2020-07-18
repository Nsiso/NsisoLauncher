using System.ComponentModel;
using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Pages;

namespace NsisoLauncher.ViewModels.Pages
{
    public class WelcomePageViewModel : INotifyPropertyChanged
    {
        public WelcomePageViewModel(MainWindowViewModel mainWindowVM)
        {
            MainWindowVM = mainWindowVM;
            LoadedCommand = new DelegateCommand(async a =>
            {
                await App.RefreshVersionListAsync();
                var mainPage = new MainPage(mainWindowVM);
                MainWindowVM.NavigationService.Navigate(mainPage);
            });
        }

        public MainWindowViewModel MainWindowVM { get; set; }

        public DelegateCommand LoadedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}