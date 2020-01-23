using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NsisoLauncher.ViewModels.Dialogs
{
    public class LaunchingDialogViewModel : INotifyPropertyChanged
    {
        public string State { get; set; } = "初始化...";

        public string LogLine { get; set; } = "LogLine";

        public ICommand CancelLaunchingCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
