using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.ViewModels.Dialogs;

namespace NsisoLauncher.Views.Dialogs
{
    /// <summary>
    ///     LaunchingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchingDialog : CustomDialog
    {
        private readonly LaunchingDialogViewModel _launchingDialogVm;

        public LaunchingDialog(LaunchingDialogViewModel vm)
        {
            InitializeComponent();
            _launchingDialogVm = vm;
            DataContext = _launchingDialogVm;
        }
    }
}