using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
            {
                return isLaunching;
            }
            set
            {
                isLaunching = value;
                RaisePropertyChangedEvent("IsLaunching");
            }
        }

        private void RaisePropertyChangedEvent(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
