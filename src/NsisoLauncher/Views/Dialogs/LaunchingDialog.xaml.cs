using MahApps.Metro.Controls.Dialogs;

namespace NsisoLauncher.Views.Dialogs
{
    /// <summary>
    /// LaunchingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchingDialog : CustomDialog
    {
        ViewModels.Dialogs.LaunchingDialogViewModel _launchingDialogVm;
        public LaunchingDialog(ViewModels.Dialogs.LaunchingDialogViewModel vm)
        {
            InitializeComponent();
            _launchingDialogVm = vm;
            this.DataContext = _launchingDialogVm;
        }
    }
}
