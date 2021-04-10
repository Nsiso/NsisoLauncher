using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Config;
using NsisoLauncherCore.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// AuthModuleControl.xaml 的交互逻辑
    /// </summary>
    public partial class AuthModuleControl : UserControl
    {
        private KeyValuePair<string, AuthenticationNode> authModule;

        private AuthenticationType authenticationType = AuthenticationType.OFFLINE;

        public AuthModuleControl()
        {
            InitializeComponent();
            saveButton.IsEnabled = false;
            delButton.IsEnabled = false;
        }

        public void SelectionChangedAccept(KeyValuePair<string, AuthenticationNode> node)
        {
            authModule = node;
            if (authModule.Value != null)
            {
                authmoduleNameTextbox.Text = authModule.Value.Name;
                switch (authModule.Value.AuthType)
                {
                    case AuthenticationType.NIDE8:
                        nide8Radio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["nide8ID"];
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        aiRadio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["authserver"];
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        customRadio.IsChecked = true;
                        authDataTextbox.Text = authModule.Value.Property["authserver"];
                        break;
                    default:
                        return;
                }
                addButton.IsEnabled = false;
                saveButton.IsEnabled = true;
                delButton.IsEnabled = true;
                authmoduleNameTextbox.IsEnabled = false;
            }
        }

        private void Nide8_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = "统一通行证ID：";
            authenticationType = AuthenticationType.NIDE8;
        }

        private void AI_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = "验证地址：";
            authenticationType = AuthenticationType.AUTHLIB_INJECTOR;
        }

        private void Custom_Checked(object sender, RoutedEventArgs e)
        {
            authmoduleLable.Content = "代理服务器地址：";
            authenticationType = AuthenticationType.CUSTOM_SERVER;
        }

        public void ClearAll()
        {
            authmoduleNameTextbox.IsEnabled = true;
            nide8Radio.IsChecked = false;
            aiRadio.IsChecked = false;
            customRadio.IsChecked = false;
            authmoduleNameTextbox.Text = string.Empty;
            authDataTextbox.Text = string.Empty;

            addButton.IsEnabled = true;
            saveButton.IsEnabled = false;
            delButton.IsEnabled = false;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsEmpty())
            {
                string authName = authmoduleNameTextbox.Text;
                string authData = authDataTextbox.Text;
                AuthenticationNode node = new AuthenticationNode() { AuthType = authenticationType, Name = authName };
                switch (authenticationType)
                {
                    case AuthenticationType.NIDE8:
                        node.Property.Add("nide8ID", authData);
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        node.Property.Add("authserver", authData);
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        node.Property.Add("authserver", authData);
                        break;
                    default:
                        node.Property.Add("authserver", authData);
                        break;
                }
                if (App.Config.MainConfig.User.AuthenticationDic.Any(x => x.Key == authName))
                {
                    await App.MainWindowVM.ShowMessageAsync("添加的验证模型名称已存在", "您可以尝试更换可用的验证模型名称");
                    return;
                }
                var item = new KeyValuePair<string, AuthenticationNode>(authName, node);
                App.Config.MainConfig.User.AuthenticationDic.Add(authName, node);
                await App.MainWindowVM.ShowMessageAsync("添加成功", "记得点击应用按钮保存噢");
            }
        }

        private bool CheckIsEmpty()
        {
            if (authenticationType == AuthenticationType.OFFLINE)
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("未选择验证模型", "请选择您要使用的验证模型");
                return true;
            }
            if (string.IsNullOrWhiteSpace(authmoduleNameTextbox.Text))
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("未填写验证模型名称", "请填写您验证模型的名称");
                return true;
            }
            if (string.IsNullOrWhiteSpace(authDataTextbox.Text))
            {
                ((MetroWindow)Window.GetWindow(this)).ShowMessageAsync("未填写模型数据", "请填写您验证模型的数据");
                return true;
            }
            return false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsEmpty())
            {
                string authData = authDataTextbox.Text;
                authModule.Value.Property.Clear();
                authModule.Value.AuthType = authenticationType;
                switch (authenticationType)
                {
                    case AuthenticationType.NIDE8:
                        authModule.Value.Property.Add("nide8ID", authData);
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        authModule.Value.Property.Add("authserver", authData);
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        authModule.Value.Property.Add("authserver", authData);
                        break;
                    default:
                        authModule.Value.Property.Add("authserver", authData);
                        break;
                }
                await App.MainWindowVM.ShowMessageAsync("保存成功", "记得点击应用按钮保存噢");
            }
        }

        private async void DelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Config.MainConfig.User.AuthenticationDic.Remove(authModule.Key);
            await App.MainWindowVM.ShowMessageAsync("删除成功", "记得点击应用按钮保存噢");
        }
    }
}
