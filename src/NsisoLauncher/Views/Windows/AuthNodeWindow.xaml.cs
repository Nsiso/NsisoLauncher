using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using MahApps.Metro;
using MahApps.Metro.Controls;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Modules;
using System.ComponentModel;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// AuthNodeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AuthNodeWindow : MetroWindow, INotifyPropertyChanged
    {
        public ObservableCollection<AuthenticationNode> Nodes { get; set; }

        public AuthenticationNode SelectedNode { get; set; }

        public MainConfig MainConfig { get; set; }


        public string NodeName { get; set; }

        public AuthenticationType? AuthType { get; set; }

        public string Property { get; set; }

        public string PropertyLabel { get; set; } = "验证模型数据：";


        public bool SaveEnable { get; set; } = true;

        public bool DeleteEnable { get; set; } = false;



        public ICommand SaveCmd { get; set; }

        public ICommand DeleteCmd { get; set; }

        public ICommand ClearSelectedCmd { get; set; }

        public AuthNodeWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            if (App.Config?.MainConfig != null)
            {
                Nodes = new ObservableCollection<AuthenticationNode>(App.Config.MainConfig.User.AuthenticationDic.Values);
            }

            SaveCmd = new DelegateCommand((a) =>
            {
                this.Save();
            });

            DeleteCmd = new DelegateCommand((a) =>
            {
                this.Delete();
            });

            ClearSelectedCmd = new DelegateCommand((a) =>
            {
                this.SelectedNode = null;
            });

            this.PropertyChanged += AuthNodeWindow_PropertyChanged;
        }

        private void AuthNodeWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedNode")
            {
                if (SelectedNode != null)
                {
                    this.NodeName = SelectedNode.Name;
                    this.AuthType = SelectedNode.AuthType;
                    switch (SelectedNode.AuthType)
                    {
                        case AuthenticationType.NIDE8:
                            this.Property = SelectedNode.Property["nide8ID"];
                            break;
                        case AuthenticationType.AUTHLIB_INJECTOR:
                            this.Property = SelectedNode.Property["authserver"];
                            break;
                        case AuthenticationType.CUSTOM_SERVER:
                            this.Property = SelectedNode.Property["authserver"];
                            break;
                        default:
                            this.Property = null;
                            break;
                    }
                    if (SelectedNode.Locked)
                    {
                        SaveEnable = false;
                        DeleteEnable = false;
                    }
                    else
                    {
                        SaveEnable = true;
                        DeleteEnable = true;
                    }
                }
                else
                {
                    this.NodeName = null;
                    this.AuthType = null;
                    this.Property = null;
                    SaveEnable = true;
                    DeleteEnable = false;
                }
            }

            if (e.PropertyName == "AuthType")
            {
                if (AuthType == null)
                {
                    this.PropertyLabel = "验证模型数据：";
                }
                else
                {
                    switch (AuthType)
                    {
                        case AuthenticationType.NIDE8:
                            this.PropertyLabel = "统一通行证ID：";
                            break;
                        case AuthenticationType.AUTHLIB_INJECTOR:
                            this.PropertyLabel = "服务器地址：";
                            break;
                        case AuthenticationType.CUSTOM_SERVER:
                            this.PropertyLabel = "服务器地址：";
                            break;
                        default:
                            this.PropertyLabel = "验证模型数据：";
                            break;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(NodeName))
            {
                this.ShowMessageAsync("没有输入验证节点名称", "请确认您输入的验证节点名称不为空");
                return;
            }
            if (AuthType == null)
            {
                this.ShowMessageAsync("没有选择验证节点类型", "请确认您选择了验证节点的验证类型");
                return;
            }
            if (string.IsNullOrWhiteSpace(Property))
            {
                this.ShowMessageAsync("没有输入验证节点属性", "请确认您输入的验证节点属性不为空");
                return;
            }
            if (SelectedNode == null)
            {
                //add
                string id = Guid.NewGuid().ToString();
                AuthenticationNode newNode = new AuthenticationNode(id)
                {
                    AuthType = AuthType.Value,
                    Name = NodeName
                };
                switch (AuthType)
                {
                    case AuthenticationType.NIDE8:
                        newNode.Property["nide8ID"] = Property;
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        newNode.Property["authserver"] = Property;
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        newNode.Property["authserver"] = Property;
                        break;
                    default:
                        break;
                }
                App.Config.MainConfig.User.AuthenticationDic.Add(id, newNode);
                Nodes.Add(newNode);
                this.SelectedNode = newNode;
            }
            else
            {
                //change
                AuthenticationNode selected = SelectedNode;
                selected.Name = NodeName;
                selected.AuthType = AuthType.Value;
                switch (AuthType)
                {
                    case AuthenticationType.NIDE8:
                        selected.Property["nide8ID"] = Property;
                        break;
                    case AuthenticationType.AUTHLIB_INJECTOR:
                        selected.Property["authserver"] = Property;
                        break;
                    case AuthenticationType.CUSTOM_SERVER:
                        selected.Property["authserver"] = Property;
                        break;
                    default:
                        break;
                }
            }
        }

        private void Delete()
        {
            if (SelectedNode == null)
            {
                this.ShowMessageAsync("没有选中任何节点，无法执行删除操作", "请确认您选中了验证节点");
            }
            else
            {
                App.Config.MainConfig.User.AuthenticationDic.Remove(SelectedNode.Id);
                Nodes.Remove(SelectedNode);
            }
        }
    }
}
