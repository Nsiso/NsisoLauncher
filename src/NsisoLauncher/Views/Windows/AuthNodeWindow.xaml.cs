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
using NsisoLauncherCore.Modules;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// AuthNodeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AuthNodeWindow : MetroWindow
    {
        public ObservableCollection<AuthenticationNode> Nodes { get; set; }

        public AuthenticationNode SelectedNode { get; set; }

        public MainConfig MainConfig { get; set; }

        public string Name { get; set; }

        public AuthenticationType Type { get; set; }

        public string Property { get; set; }



        public ICommand SaveCmd { get; set; }

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
                if (SelectedNode == null)
                {
                    //add

                }
                else
                {
                    //change
                }
            });
        }
    }
}
