using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncherCore.Net.MojangApi.Api;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

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
