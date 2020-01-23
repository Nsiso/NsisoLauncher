using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        private bool _isGameSettingChanged = false;

        public SettingWindow()
        {
            InitializeComponent();
            FirstBinding();
            Refresh();
        }

        private void FirstBinding()
        {
            AccentColorComboBox.ItemsSource = ThemeManager.Accents;
            appThmeComboBox.ItemsSource = ThemeManager.AppThemes;
            authModuleCombobox.ItemsSource = App.Config.MainConfig.User.AuthenticationDic;
            versionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            App.Config.MainConfig.Environment.PropertyChanged += Environment_PropertyChanged;
            App.Config.MainConfig.Download.PropertyChanged += Download_PropertyChanged;
        }

        private void Download_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CheckDownloadFileHash")
            {
                App.Downloader.CheckFileHash = App.Config.MainConfig.Download.CheckDownloadFileHash;
            }
        }

        private void Environment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GamePathType")
            {
                switch (App.Config.MainConfig.Environment.GamePathType)
                {
                    case GameDirEnum.ROOT:
                        App.Handler.GameRootPath = Path.GetFullPath(".minecraft");
                        break;
                    case GameDirEnum.APPDATA:
                        App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                        break;
                    case GameDirEnum.PROGRAMFILES:
                        App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                        break;
                    case GameDirEnum.CUSTOM:
                        App.Handler.GameRootPath = App.Config.MainConfig.Environment.GamePath + "\\.minecraft";
                        break;
                    default:
                        throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
                }
            }
            if (e.PropertyName == "VersionIsolation")
            {
                App.Handler.VersionIsolation = App.Config.MainConfig.Environment.VersionIsolation;
            }
        }

        private async void Refresh()
        {
            //绑定content设置
            this.DataContext = App.Config.MainConfig;

            #region 元素初始化
            javaPathComboBox.ItemsSource = App.JavaList;
            memorySlider.Maximum = SystemTools.GetTotalMemory();

            VersionsComboBox.ItemsSource = await App.Handler.GetVersionsAsync();

            #endregion

            if (App.Config.MainConfig.Environment.VersionIsolation)
            {
                VersionsComboBox.IsEnabled = true;
            }
            else
            {
                VersionsComboBox.IsEnabled = false;
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(App.Handler, new NsisoLauncherCore.Modules.Version() { ID = "null" });
            }

            //debug
        }

        public void ShowAddAuthModule()
        {
            tabControl.SelectedIndex = 3;
            addAuthModuleExpander.IsExpanded = true;
            addAuthModuleExpander.Focus();
        }

        private async void chooseJavaButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "选择Java",
                Filter = "Java应用程序(无窗口)|javaw.exe|Java应用程序(含窗口)|java.exe",
            };
            if (dialog.ShowDialog() == true)
            {
                Java java = await Java.GetJavaInfoAsync(dialog.FileName);
                if (java != null)
                {
                    this.javaPathComboBox.Text = java.Path;
                    this.javaInfoLabel.Content = string.Format("Java版本：{0}，位数：{1}", java.Version, java.Arch);
                }
                else
                {
                    this.javaPathComboBox.Text = dialog.FileName;
                    await this.ShowMessageAsync("选择的Java无法正确获取信息", "请确认您选择的是正确的Java应用");
                }
            }
        }

        private void gamedirChooseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                Description = "选择游戏运行根目录",
                ShowNewFolderButton = true
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            else
            {
                gamedirPathTextBox.Text = dialog.SelectedPath.Trim();
                App.Config.MainConfig.Environment.GamePath = dialog.SelectedPath.Trim();
            }
        }
        #region 全局设置部分
        private void memorySlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            App.Config.MainConfig.Environment.MaxMemory = Convert.ToInt32(((RangeSlider)sender).UpperValue);
        }

        private void memorySlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            App.Config.MainConfig.Environment.MinMemory = Convert.ToInt32(((RangeSlider)sender).LowerValue);
        }
        #endregion

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }



        //保存按钮点击后
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            #region 实时修改
            switch (App.Config.MainConfig.Environment.GamePathType)
            {
                case GameDirEnum.ROOT:
                    App.Handler.GameRootPath = Path.GetFullPath(".minecraft");
                    break;
                case GameDirEnum.APPDATA:
                    App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                    break;
                case GameDirEnum.PROGRAMFILES:
                    App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                    break;
                case GameDirEnum.CUSTOM:
                    App.Handler.GameRootPath = App.Config.MainConfig.Environment.GamePath + "\\.minecraft";
                    break;
                default:
                    throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
            }
            App.Handler.VersionIsolation = App.Config.MainConfig.Environment.VersionIsolation;
            App.Downloader.CheckFileHash = App.Config.MainConfig.Download.CheckDownloadFileHash;
            #endregion

            if (_isGameSettingChanged)
            {
                if (App.Config.MainConfig.Environment.VersionIsolation)
                {
                    await GameHelper.SaveOptionsAsync(
                    (List<VersionOption>)versionOptionsGrid.ItemsSource,
                    App.Handler,
                    (NsisoLauncherCore.Modules.Version)VersionsComboBox.SelectedItem);
                }
                else
                {
                    await GameHelper.SaveOptionsAsync(
                    (List<VersionOption>)versionOptionsGrid.ItemsSource,
                    App.Handler,
                    new NsisoLauncherCore.Modules.Version() { ID = "null" });
                }
            }

            App.Config.Save();
            await this.ShowMessageAsync("保存成功", "所有设置已成功保存在本地");
        }

        private async void VersionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = (System.Windows.Controls.ComboBox)sender;

            if (comboBox.SelectedItem != null)
            {
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(App.Handler, (NsisoLauncherCore.Modules.Version)comboBox.SelectedItem);
            }
            else
            {
                versionOptionsGrid.ItemsSource = null;
            }
        }

        private async void refreshVersionsButton_Click(object sender, RoutedEventArgs e)
        {
            VersionsComboBox.ItemsSource = await App.Handler.GetVersionsAsync();
        }

        private void AccentColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accent item = (Accent)((System.Windows.Controls.ComboBox)sender).SelectedItem;
            if (item != null)
            {
                var AppStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
                var theme = AppStyle.Item1;
                ThemeManager.ChangeAppStyle(System.Windows.Application.Current, ThemeManager.GetAccent(item.Name), theme);
            }
        }

        private void appThmeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme item = (AppTheme)((System.Windows.Controls.ComboBox)sender).SelectedItem;
            if (item != null)
            {
                var AppStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
                var accent = AppStyle.Item2;
                ThemeManager.ChangeAppStyle(System.Windows.Application.Current, accent, item);
            }
        }

        private void javaPathComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Java java = (Java)(((System.Windows.Controls.ComboBox)sender).SelectedItem);
            if (java != null)
            {
                this.javaInfoLabel.Content = string.Format("Java版本：{0}，位数：{1}", java.Version, java.Arch);
            }
            else
            {
                this.javaInfoLabel.Content = null;
            }
        }

        private /*async*/ void forgetUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            //todo （后）恢复注销用户功能
            node.AccessToken = null;
            this.ShowMessageAsync("注销成功", "请保存以生效");
        }

        private void clearUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            KeyValuePair<string, UserNode> selectedItem = (KeyValuePair<string, UserNode>)userComboBox.SelectedItem;
            UserNode node = selectedItem.Value;
            node.AccessToken = null;
            node.Profiles = null;
            node.UserData = null;
            node.SelectProfileUUID = null;
            this.ShowMessageAsync("重置用户成功", "请保存以生效");
        }

        private void delUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userComboBox.SelectedItem == null)
            {
                this.ShowMessageAsync("您未选择要进行操作的用户", "请先选择您要进行操作的用户");
                return;
            }

            string key = (string)userComboBox.SelectedValue;
            App.Config.MainConfig.User.UserDatabase.Remove(key);
            this.ShowMessageAsync("删除用户成功", "请保存以生效");
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            // 激活的是当前默认的浏览器
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
        #region 自定义验证模型

        private void AuthModuleCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedItem = authModuleCombobox.SelectedItem;
            if (selectedItem == null)
            {
                authmoduleControl.ClearAll();
            }
            else
            {
                authmoduleControl.SelectionChangedAccept((KeyValuePair<string, AuthenticationNode>)selectedItem);
            }
        }

        public async void AddAuthModule(string name, AuthenticationNode authmodule)
        {
            if (App.Config.MainConfig.User.AuthenticationDic.Any(x => x.Key == name))
            {
                await this.ShowMessageAsync("添加的验证模型名称已存在", "您可以尝试更换可用的验证模型名称");
                return;
            }
            var item = new KeyValuePair<string, AuthenticationNode>(name, authmodule);
            App.Config.MainConfig.User.AuthenticationDic.Add(name, authmodule);
            await this.ShowMessageAsync("添加成功", "记得点击应用按钮保存噢");
            authModuleCombobox.SelectedItem = item;
        }

        public async void SaveAuthModule(KeyValuePair<string, AuthenticationNode> node)
        {
            await this.ShowMessageAsync("保存成功", "记得点击应用按钮保存噢");
        }

        public async void DeleteAuthModule(KeyValuePair<string, AuthenticationNode> node)
        {
            App.Config.MainConfig.User.AuthenticationDic.Remove(node.Key);
            await this.ShowMessageAsync("删除成功", "记得点击应用按钮保存噢");
        }

        private void ClearAuthselectButton_Click(object sender, RoutedEventArgs e)
        {
            authModuleCombobox.SelectedItem = null;
        }
        #endregion

        private void clearAllauthButton_Click(object sender, RoutedEventArgs e)
        {
            lockauthCombobox.SelectedItem = null;
        }

        private void VersionOptionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _isGameSettingChanged = true;
        }
    }
}
