using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
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
                MainWindowVM.NavigationService.Navigate(new Views.Pages.MainPage(mainWindowVM));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
