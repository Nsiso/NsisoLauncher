using System.ComponentModel;

namespace NsisoLauncherCore.Util
{
    /// <summary>
    /// 启动信号类，负责观测的游戏启动中各数据变动
    /// </summary>
    public class LaunchSignal : INotifyPropertyChanged
    {
        private bool isLaunching = false;

        public bool IsLaunching
        {
            get
            { return isLaunching; }
            set
            {
                isLaunching = value;
                RaisePropertyChangedEvent("IsLaunching");
            }
        }

        private LaunchInstance launchingInstance = null;

        public LaunchInstance LaunchingInstance
        {
            get
            { return launchingInstance; }
            set
            {
                launchingInstance = value;
                RaisePropertyChangedEvent("LaunchingInstance");
                if (launchingInstance != null)
                {
                    launchingInstance.Log += (obj, str) => { LatestLog = str.Message; };
                }
            }
        }

        private string latestLog;

        public string LatestLog
        {
            get { return latestLog; }
            set
            {
                latestLog = value;
                RaisePropertyChangedEvent("LatestLog");
            }
        }


        private void RaisePropertyChangedEvent(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
