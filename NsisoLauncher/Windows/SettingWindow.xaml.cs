using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncher.Core.Util;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        private Config.MainConfig config;
        private List<AuthTypeItem> authTypes = new List<AuthTypeItem>()
        {
            new AuthTypeItem(){Type = Config.AuthenticationType.OFFLINE, Name = App.GetResourceString("String.MainWindow.Auth.Offline")},
            new AuthTypeItem(){Type = Config.AuthenticationType.MOJANG, Name = App.GetResourceString("String.MainWindow.Auth.Mojang")},
            new AuthTypeItem(){Type = Config.AuthenticationType.NIDE8, Name = App.GetResourceString("String.MainWindow.Auth.Nide8")},
            new AuthTypeItem(){Type = Config.AuthenticationType.CUSTOM_SERVER, Name = App.GetResourceString("String.MainWindow.Auth.Custom")}
        };

        public SettingWindow()
        {
            InitializeComponent();

            Refresh();
        }

        private async void Refresh()
        {
            config = DeepCloneObject(App.config.MainConfig);
            debugCheckBox.DataContext = config.Launcher;
            javaPathComboBox.ItemsSource = App.javaList;
            environmentGrid.DataContext = config.Environment;
            downloadGrid.DataContext = config.Download;
            memorySlider.Maximum = SystemTools.GetTotalMemory();
            customGrid.DataContext = config.Customize;
            AccentColorComboBox.ItemsSource = ThemeManager.Accents;
            appThmeComboBox.ItemsSource = ThemeManager.AppThemes;
            serverGroupBox.DataContext = config.Server;
            authtypeCombobox.ItemsSource = authTypes;
            authtypeCombobox.SelectedItem = authTypes.Where(x => { return x.Type == config.User.AuthenticationType; }).FirstOrDefault();
            userGrid.DataContext = config.User;
            versionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            VersionsComboBox.ItemsSource = await App.handler.GetVersionsAsync();
            if (App.config.MainConfig.Environment.VersionIsolation)
            {
                VersionsComboBox.IsEnabled = true;
            }
            else
            {
                VersionsComboBox.IsEnabled = false;
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(App.handler, new Core.Modules.Version() { ID = "null" });
            }
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
                if (java!=null)
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
                config.Environment.GamePath = dialog.SelectedPath.Trim();
            }
        }
        #region 全局设置部分
        private void memorySlider_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            config.Environment.MaxMemory = Convert.ToInt32(((RangeSlider)sender).UpperValue);
        }

        private void memorySlider_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            config.Environment.MinMemory = Convert.ToInt32(((RangeSlider)sender).LowerValue);
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
            switch (config.Environment.GamePathType)
            {
                case GameDirEnum.ROOT:
                    App.handler.GameRootPath = Path.GetFullPath(".minecraft");
                    break;
                case GameDirEnum.APPDATA:
                    App.handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                    break;
                case GameDirEnum.PROGRAMFILES:
                    App.handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                    break;
                case GameDirEnum.CUSTOM:
                    App.handler.GameRootPath = config.Environment.GamePath + "\\.minecraft";
                    break;
                default:
                    throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
            }
            App.handler.VersionIsolation = config.Environment.VersionIsolation;
            if (!string.IsNullOrWhiteSpace(nide8IdTextBox.Text))
            {
                App.nide8Handler = new Core.Net.Nide8API.APIHandler(nide8IdTextBox.Text);
            }
            #endregion
            config.User.AuthenticationType = ((AuthTypeItem)authtypeCombobox.SelectedItem).Type;
            App.config.MainConfig = config;
            if (App.config.MainConfig.Environment.VersionIsolation)
            {
                await GameHelper.SaveOptionsAsync(
                (List<VersionOption>)versionOptionsGrid.ItemsSource,
                App.handler,
                (Core.Modules.Version)VersionsComboBox.SelectedItem);
            }
            else
            {
                await GameHelper.SaveOptionsAsync(
                (List<VersionOption>)versionOptionsGrid.ItemsSource,
                App.handler,
                new Core.Modules.Version() { ID = "null" });
            }
            
            App.config.Save();
            await this.ShowMessageAsync("保存成功", "所有设置已成功保存在本地");
        }

        //取消按钮点击后
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void VersionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = (System.Windows.Controls.ComboBox)sender;

            if (comboBox.SelectedItem != null)
            {
                versionOptionsGrid.ItemsSource = await GameHelper.GetOptionsAsync(App.handler, (Core.Modules.Version)comboBox.SelectedItem);
            }
            else
            {
                versionOptionsGrid.ItemsSource = null;
            }
        }

        private async void refreshVersionsButton_Click(object sender, RoutedEventArgs e)
        {
            VersionsComboBox.ItemsSource = await App.handler.GetVersionsAsync();
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

        private static T DeepCloneObject<T>(T t) where T : class
        {
            T model = Activator.CreateInstance<T>();                     //实例化一个T类型对象
            PropertyInfo[] propertyInfos = model.GetType().GetProperties();     //获取T对象的所有公共属性
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                //判断值是否为空，如果空赋值为null见else
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    //如果convertsionType为nullable类，声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
                    NullableConverter nullableConverter = new NullableConverter(propertyInfo.PropertyType);
                    //将convertsionType转换为nullable对的基础基元类型
                    propertyInfo.SetValue(model, Convert.ChangeType(propertyInfo.GetValue(t,null), nullableConverter.UnderlyingType), null);
                }
                else
                {
                    propertyInfo.SetValue(model, Convert.ChangeType(propertyInfo.GetValue(t,null), propertyInfo.PropertyType), null);
                }
            }
            return model;
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(config.User.AccessToken))
            {
                string token = config.User.AccessToken;
                config.User.AccessToken = null;
                Core.Net.MojangApi.Endpoints.Invalidate invalidate = new Core.Net.MojangApi.Endpoints.Invalidate(token);
                var loading = await this.ShowProgressAsync("注销正版登陆中", "需要联网进行注销，请稍后...");
                loading.SetIndeterminate();
                var result = await invalidate.PerformRequestAsync();
                await loading.CloseAsync();
                if (result.IsSuccess)
                {
                    await this.ShowMessageAsync("注销成功", "已经安全,成功的取消了记住登录状态");
                }
                else
                {
                    await this.ShowMessageAsync("已取消记住登陆状态但未注销", "虽然取消并删除了本地的记住登陆状态和密匙，但未能成功联网注销密匙，请注意密匙安全");
                }

            }
            else
            {
                await this.ShowMessageAsync("未进行过在线验证", "您未进行过在线验证，无需注销登陆状态");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            config.User = new Config.User()
            {
                AuthenticationType = Config.AuthenticationType.OFFLINE
            };
            this.ShowMessageAsync("重置完成", "点击右下角应用按钮保存");
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            config.User.AccessToken = null;
            config.User.AuthenticationUserData = null;
            config.User.AuthenticationUUID = null;
            config.User.ClientToken = null;
            config.User.UserName = null;
            await this.ShowMessageAsync("设置发布状态成功",
                "点击右下角应用按钮保存，保存后除了关闭启动器请不要执行任何操作（二次设置，启动游戏等），否则将导致数据重新初始化");
        }
    }
}
