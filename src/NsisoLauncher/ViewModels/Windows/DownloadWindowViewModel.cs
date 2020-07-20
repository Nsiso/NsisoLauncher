using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using NsisoLauncherCore.Net;

namespace NsisoLauncher.ViewModels.Windows
{
    public class DownloadWindowViewModel : INotifyPropertyChanged
    {
        public DownloadWindowViewModel()
        {
            InitChart();
            SpeedStr = "0Kb/s";
            ProgressMaximum = 1;
            ProgressValue = 0;
            Percentage = 0;
            if (App.NetHandler.Downloader != null)
            {
                Tasks = App.NetHandler.Downloader.ViewDownloadTasks;
                App.NetHandler.Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                App.NetHandler.Downloader.DownloadSpeedChanged += Downloader_DownloadSpeedChanged;
                App.NetHandler.Downloader.DownloadCompleted += Downloader_DownloadCompleted;
            }
        }

        public ObservableCollection<DownloadTask> Tasks { get; }

        public double Percentage { get; set; }

        public string SpeedStr { get; set; }

        public int ProgressMaximum { get; set; }

        public int ProgressValue { get; set; }

        /// <summary>
        ///     窗口对话
        /// </summary>
        public IDialogCoordinator Instance { get; set; }

        public Func<double, string> YFormatter { get; set; } = value =>
        {
            string speedUnit;
            double speedValue;
            if (value > 1048576)
            {
                speedUnit = "MB/s";
                speedValue = Math.Round(value / 1048576);
            }
            else if (value > 1024)
            {
                speedUnit = "KB/s";
                speedValue = Math.Round(value / 1024);
            }
            else
            {
                speedUnit = "B/s";
                speedValue = value;
            }

            return string.Format("{0}{1}", speedValue, speedUnit);
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public void InitChart()
        {
            SpeedValues = new ChartValues<double>();
            for (var i = 0; i < 50; i++) SpeedValues.Add(0);
            ChartSeries = new SeriesCollection
                {new LineSeries {Values = SpeedValues, PointGeometry = null, LineSmoothness = 0, Title = "下载速度"}};
        }

        private void ClearSpeedValues()
        {
            for (var i = 0; i < 50; i++) SpeedValues[i] = 0;
        }

        private async void Downloader_DownloadCompleted(object sender, DownloadCompletedArg e)
        {
            SpeedStr = "0Kb/s";
            ProgressMaximum = 1;
            ProgressValue = 0;
            Percentage = 0;
            ClearSpeedValues();
            if (e.ErrorList == null || e.ErrorList.Count == 0)
                await Instance.ShowMessageAsync(this, App.GetResourceString("String.Downloadwindow.DownloadComplete"),
                    App.GetResourceString("String.Downloadwindow.DownloadComplete2"));
            //undo close window
            else
                await Instance.ShowMessageAsync(this,
                    App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError"),
                    string.Format(App.GetResourceString("String.Downloadwindow.DownloadCompleteWithError2"),
                        e.ErrorList.Count, e.ErrorList.First().Value.Message));
        }

        private void Downloader_DownloadSpeedChanged(object sender, DownloadSpeedChangedArg e)
        {
            SpeedStr = e.SpeedValue + e.SpeedUnit;
            SpeedValues.Add(e.SizePerSec);
            SpeedValues.RemoveAt(0);
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedArg e)
        {
            var total = e.LeftTasksCount + e.DoneTaskCount;
            ProgressMaximum = total;
            ProgressValue = e.DoneTaskCount;
            Percentage = (double) e.DoneTaskCount / (e.DoneTaskCount + e.LeftTasksCount);
        }

        #region Char

        public SeriesCollection ChartSeries { get; set; }

        public ChartValues<double> SpeedValues { get; set; }

        #endregion
    }
}