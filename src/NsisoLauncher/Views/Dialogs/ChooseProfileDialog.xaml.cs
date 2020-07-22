using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Endpoints;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NsisoLauncher.Views.Dialogs
{
    /// <summary>
    /// ChooseProfileDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseProfileDialog : CustomDialog, INotifyPropertyChanged
    {
        /// <summary>
        /// 窗口对话
        /// </summary>
        private MainWindowViewModel instance;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Uuid> Profiles { get; set; }
        public Uuid SelectedProfile { get; set; }
        public ChooseProfileDialog(MainWindowViewModel vm, List<Uuid> profiles)
        {
            instance = vm;
            Profiles = profiles;
            InitializeComponent();
            comboBox.DataContext = this;
        }

        private async void okButton_Click(object sender, RoutedEventArgs e)
        {
            await instance.HideMetroDialogAsync(this);
        }
    }
}
