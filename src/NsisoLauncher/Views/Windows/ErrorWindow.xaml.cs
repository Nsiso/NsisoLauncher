using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Util;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NsisoLauncher.Views.Windows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : MetroWindow
    {
        Exception exception;
        string report;

        string[] funny = {
            "你所不知道的事实：参与这个启动器的开发者只有一个人，而且整个开发工作室也只有这一个人",
            "崩溃了？不要急，看见你的键盘了吗？看见了吗？现在把它拿起来。拿起来的对吧？，现在去砸显示器",
            "还记得世界末日这个脑洞大开的电影吗？这个崩溃不会造成那样的后果的，放心",
            "其实这个启动器里面最大的彩蛋就是这个错误窗口的槽点栏。每次都会随机生成。就比如说这条",
            "这个启动器发生意外的原因是：开发者正在陪他的老婆，而且对这个启动器的修复不闻不问（伪",
            "如果你打算拿这个窗口左下角的联系方式来骚扰我的话，那么再见。当然除非你是妹子，那我不会介意",
            "其实开发者在开发新版本的时候还是比较懒的，有些内容还是旧版本里的，比如说这个错误窗口！",
            "新版本的异常处理貌似不错！崩溃是不会直接崩溃了，但问题是这个异常什么时候搞定？",
            "好心人，拜托上传个错误报告吧，会有美女找你的"
        };

        public ErrorWindow(Exception ex)
        {
            try
            {
                InitializeComponent();
                exception = ex;
                Random random = new Random();
                FunnyBlock.Text = funny[random.Next(funny.Count())];
                moreInfoCheckBox.IsChecked = true;

                report = GetReport();

                this.textBox.Text = report;
                if (Environment.GetCommandLineArgs().Contains("-reboot"))
                {
                    MessageBox.Show("很抱歉启动器在重启之后再次发生错误\n您可以在左下角联系开发者加快解决这个问题，我们由衷的表示感谢");
                }
            }
            catch (Exception)
            {
            }
        }

        //作者邮箱点击后
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:siso@nsiso.com");
        }

        //作者qq点击后
        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://shang.qq.com/wpa/qunwpa?idkey=3b39ff435aeca097dbe8bcef6f32d26367bdb630357570693b5315e6c13f7c9f");
        }

        //作者github点击后
        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Nsiso");
        }

        private async void RebootButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var progress = await this.ShowProgressAsync("正在处理中", "请稍后...");
                progress.SetIndeterminate();
                bool moreInfo = (bool)moreInfoCheckBox.IsChecked;
                if (moreInfo)
                { report += ("/r/n" + await GetEnvironmentInfoAsync()); }
                await App.NsisoAPIHandler.PostLogAsync(NsisoLauncherCore.Modules.LogLevel.FATAL, report);
            }
            catch (Exception ex)
            {
                App.LogHandler.AppendError(ex);
            }
            finally
            {
                App.Reboot(false);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(textBox.Text);
            this.ShowMessageAsync("复制成功", "你现在可以点击窗口左下角作者联系方式，并把这该死的错误抛给他");
        }

        private string GetEnvironmentInfo()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("\r\n==========环境信息==========");
            builder.Append("\r\nCPU信息:" + SystemTools.GetProcessorInfo());
            builder.Append("\r\n内存信息: 总大小:" + SystemTools.GetTotalMemory().ToString() + "MB/可用大小:" + SystemTools.GetRunmemory().ToString() + "MB");
            builder.Append("\r\n显卡信息:" + SystemTools.GetVideoCardInfo());
            builder.Append("\r\n操作系统:" + Environment.OSVersion.Platform);
            builder.Append("\r\n版本号:" + Environment.OSVersion.VersionString);
            builder.Append("\r\n系统位数:" + SystemTools.GetSystemArch());
            builder.Append("\r\n程序运行命令行:" + Environment.CommandLine);
            builder.Append("\r\n程序工作目录:" + Environment.CurrentDirectory);
            return builder.ToString();
        }

        private Task<string> GetEnvironmentInfoAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return GetEnvironmentInfo();
            });
        }

        private string GetReport()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine("========NsisoLauncher Error Report========");
            strBuilder.AppendFormat("Date:{0}    Launcher Version:{1}", DateTime.Now, App.ResourceAssembly.GetName().Version).AppendLine();
            strBuilder.AppendLine("Exception Detail:");
            strBuilder.AppendLine(exception?.ToString());
            return strBuilder.ToString();
        }
    }
}
