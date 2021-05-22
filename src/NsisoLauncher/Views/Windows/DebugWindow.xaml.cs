using MahApps.Metro.Controls;
using NsisoLauncherCore.Modules;
using System;
using System.Windows;

namespace NsisoLauncher.Views.Windows
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
                if (log.LogLevel == LogLevel.GAME)
                {
                    this.textBox.AppendText(string.Format("{0}\n", log.ToString(true)));
                }
                else
                {
                    this.textBox.AppendText(string.Format("{0}\n", log));
                }
                textBox.ScrollToEnd();
            }));

        }

        private void findKeyWord_Click(object sender, RoutedEventArgs e)
        {
            string str = keyWordTextBox.Text;
            int index = textBox.Text.LastIndexOf(str);
            if (index != -1)
            {
                textBox.Focus();
                textBox.Select(index, str.Length);
            }
        }
    }
}
