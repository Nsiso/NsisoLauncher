using NsisoLauncher.Config;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.User;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NsisoLauncher.Views.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
        }

        //public void ShowAddAuthModule()
        //{
        //    tabControl.SelectedIndex = 3;
        //    addAuthModuleExpander.IsExpanded = true;
        //    addAuthModuleExpander.Focus();
        //}

        //#region 全局设置部分
        //private void memorySlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        //{
        //    App.Config.MainConfig.Environment.MaxMemory = Convert.ToInt32(((RangeSlider)sender).UpperValue);
        //}

        //private void memorySlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        //{
        //    App.Config.MainConfig.Environment.MinMemory = Convert.ToInt32(((RangeSlider)sender).LowerValue);
        //}
        //#endregion

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }



        //保存按钮点击后
        //private async void saveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    #region 实时修改
        //    switch (App.Config.MainConfig.Environment.GamePathType)
        //    {
        //        case GameDirEnum.ROOT:
        //            App.Handler.GameRootPath = Path.GetFullPath(".minecraft");
        //            break;
        //        case GameDirEnum.APPDATA:
        //            App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
        //            break;
        //        case GameDirEnum.PROGRAMFILES:
        //            App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
        //            break;
        //        case GameDirEnum.CUSTOM:
        //            App.Handler.GameRootPath = App.Config.MainConfig.Environment.GamePath + "\\.minecraft";
        //            break;
        //        default:
        //            throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
        //    }
        //    App.Handler.VersionIsolation = App.Config.MainConfig.Environment.VersionIsolation;
        //    App.NetHandler.Downloader.CheckFileHash = App.Config.MainConfig.Net.CheckDownloadFileHash;
        //    #endregion

        //    if (_isGameSettingChanged)
        //    {
        //        if (App.Config.MainConfig.Environment.VersionIsolation)
        //        {
        //            await GameHelper.SaveOptionsAsync(
        //            (List<VersionOption>)versionOptionsGrid.ItemsSource,
        //            App.Handler,
        //            (NsisoLauncherCore.Modules.Version)VersionsComboBox.SelectedItem);
        //        }
        //        else
        //        {
        //            await GameHelper.SaveOptionsAsync(
        //            (List<VersionOption>)versionOptionsGrid.ItemsSource,
        //            App.Handler,
        //            new NsisoLauncherCore.Modules.Version() { Id = "null" });
        //        }
        //    }

        //    App.Config.Save();
        //    await this.ShowMessageAsync("保存成功", "所有设置已成功保存在本地");
        //}

        private async void forgetUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                await this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            //todo （后）恢复注销用户功能
            if (node.User is YggdrasilUser ygg)
            {
                ygg.AccessToken = null;
            }
            else if(node.User is MicrosoftUser ms)
            {
                ms.MicrosoftToken = null;
                ms.MinecraftToken = null;
            }
            await this.ShowMessageAsync("注销成功", "请保存以生效");
        }

        private async void clearUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                await this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            if (node.User is YggdrasilUser ygg)
            {
                ygg.AccessToken = null;
                ygg.Profiles = null;
                ygg.UserData = null;
                ygg.SelectedProfileUuid = null;
            }
            else if (node.User is MicrosoftUser ms)
            {
                ms.MicrosoftToken = null;
                ms.MinecraftToken = null;
            }
            
            await this.ShowMessageAsync("重置用户成功", "请保存以生效");
        }

        private async void delUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                await this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            string key = (string)userComboBox.SelectedValue;
            App.Config.MainConfig.User.UserDatabase.Remove(key);
            await this.ShowMessageAsync("删除用户成功", "请保存以生效");
        }

        private void clearAllauthButton_Click(object sender, RoutedEventArgs e)
        {
            lockauthCombobox.SelectedItem = null;
        }

        private void VersionOptionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //todo 恢复游戏设置
        }

        private async Task ShowMessageAsync(string title, string msg)
        {
            await App.MainWindowVM.ShowMessageAsync(title, msg);
        }
    }
}
