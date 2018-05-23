using NsisoLauncher.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace NsisoLauncher.Windows
{
    /// <summary>
    /// DebugWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindow : MetroWindow
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        public void AppendLog(object sender, Log log)
        {
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                this.textBox.Text += string.Format("[{0}]{1}\n", log.LogLevel.ToString(), log.Message);
                textBox.ScrollToEnd();
            }));
            
        }
    }
}
