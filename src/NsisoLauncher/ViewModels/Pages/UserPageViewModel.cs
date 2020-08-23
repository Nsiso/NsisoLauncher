using NsisoLauncher.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncher.ViewModels.Pages
{
    public class UserPageViewModel : INotifyPropertyChanged
    {
        public Windows.MainWindowViewModel MainWindowVM { get; set; }

        public User User { get; set; }

        public UserPageViewModel()
        {
            if (App.MainWindowVM != null)
            {
                MainWindowVM = App.MainWindowVM;
            }

            User = App.Config?.MainConfig?.User;

            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
