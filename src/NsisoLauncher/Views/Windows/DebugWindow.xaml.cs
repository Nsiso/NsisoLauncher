using System;
using System.Windows;
using MahApps.Metro.Controls;
using NsisoLauncherCore.Modules;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    ///     DebugWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindow : MetroWindow
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        public void AppendLog(object sender, Log log)
        {
            Dispatcher.Invoke(delegate
            {
                textBox.AppendText(string.Format("[{0}][{1}]{2}\n", log.LogLevel.ToString(), DateTime.Now.ToString(),
                    log.Message));
                textBox.ScrollToEnd();
            });
        }

        public void AppendGameLog(object sender, string gamelog)
        {
            Dispatcher.Invoke(delegate
            {
                textBox.AppendText(string.Format("[GameLog]{0}\n", gamelog));
                textBox.ScrollToEnd();
            });
        }

        private void findKeyWord_Click(object sender, RoutedEventArgs e)
        {
            var str = keyWordTextBox.Text;
            var index = textBox.Text.LastIndexOf(str);
            if (index != -1)
            {
                textBox.Focus();
                textBox.Select(index, str.Length);
            }
        }
    }
}