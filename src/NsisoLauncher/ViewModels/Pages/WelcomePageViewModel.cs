using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

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
