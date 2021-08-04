using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NsisoLauncherCore.Util;
using System.Threading.Tasks;

namespace NsisoLauncherCore
{
    public class LaunchInstance
    {
        public LaunchSetting InstanceSetting { get; private set; }

        public VersionBase InstanceVersion { get; private set; }

        public Process InstanceProcess { get; private set; }

        public bool HasExited { get => InstanceProcess.HasExited; }

        public bool IsBeenKilled { get; private set; } = false;

        public int SaveLogSize { get; set; } = 25;

        public Queue<string> LatestLogQueue { get; private set; }


        public event EventHandler<Log> Log;
        public event EventHandler<GameExitArg> Exit;

        public LaunchInstance(VersionBase version,LaunchSetting setting, ProcessStartInfo processStartInfo)
        {
            this.InstanceSetting = setting;
            this.InstanceVersion = version;
            this.InstanceProcess = new Process();

            this.InstanceProcess.StartInfo = processStartInfo;

            //设置输出流
            this.InstanceProcess.StartInfo.RedirectStandardError = true;
            this.InstanceProcess.StartInfo.RedirectStandardOutput = true;

            //不使用shell execut
            this.InstanceProcess.StartInfo.UseShellExecute = false;

            //输出流事件订阅
            InstanceProcess.OutputDataReceived += Process_OutputDataReceived;
            InstanceProcess.ErrorDataReceived += Process_ErrorDataReceived;
            this.Log += LaunchInstance_Log;

            LatestLogQueue = new Queue<string>(3);
        }

        private void LaunchInstance_Log(object sender, Log e)
        {
            if (LatestLogQueue.Count >= SaveLogSize)
            {
                LatestLogQueue.Dequeue();
            }
            if (e.Message != null)
            {
                LatestLogQueue.Enqueue(e.Message);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Log?.Invoke(this, new Log(LogLevel.GAME, e.Data));
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Log?.Invoke(this, new Log(LogLevel.GAME, e.Data));
        }

        public void Start()
        {
            InstanceProcess.Start();
            InstanceProcess.BeginErrorReadLine();
            InstanceProcess.BeginOutputReadLine();

            Task.Factory.StartNew(() =>
            {
                InstanceProcess.WaitForExit();
                Exit?.Invoke(this, new GameExitArg()
                {
                    Instance = this,
                    Version = InstanceVersion,
                    ExitCode = InstanceProcess.ExitCode,
                    Duration = (InstanceProcess.StartTime - InstanceProcess.ExitTime)
                });

            });
        }

        public async Task WaitForInputIdleAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                InstanceProcess.WaitForInputIdle();
            });
        }

        public void WaitForInputIdle()
        {
            InstanceProcess.WaitForInputIdle();
        }


        public void Kill()
        {
            InstanceProcess.Kill();
            this.IsBeenKilled = true;
        }

        public void SetWindowTitle(string title)
        {
            GameHelper.SetGameTitle(InstanceProcess, title);
        }
    }
}
