using NsisoLauncher.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NsisoLauncher.Core.Util
{
    public class LogHandler
    {
        public bool WriteToFile { get; set; } = false;
        public event EventHandler<Log> OnLog;
        ReaderWriterLockSlim LogLock = new ReaderWriterLockSlim();

        public LogHandler()
        {
        }

        public LogHandler(bool write2file)
        {
            this.WriteToFile = write2file;
        }

        public void AppendLog(object sender, Log log)
        {
            OnLog?.Invoke(sender, log);
            if (WriteToFile)
            {
                Task.Factory.StartNew(() =>
                {
                    LogLock.EnterWriteLock();
                    try
                    {
                        if (log.LogLevel == LogLevel.GAME)
                        {
                            File.AppendAllText("log.txt", string.Format("[GAME]{0}\r\n", log.Message));
                        }
                        else
                        {
                            File.AppendAllText("log.txt", string.Format("[{0}][{1}]{2}\r\n", DateTime.Now.ToString(), log.LogLevel, log.Message));
                        }
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        LogLock.ExitWriteLock();
                    }
                    
                });
            }
        }

        public void AppendDebug(string msg)
        {
           AppendLog(this, new Log() { LogLevel = LogLevel.DEBUG, Message = msg });
        }

        public void AppendInfo(string msg)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.INFO, Message = msg });
        }

        public void AppendWarn(string msg)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.WARN, Message = msg });
        }

        public void AppendError(Exception e)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.ERROR, Message = e.ToString() });
        }

        public void AppendFatal(Exception e)
        {
            AppendLog(this, new Log() { LogLevel = LogLevel.FATAL, Message = e.ToString() });
            new Windows.ErrorWindow(e).Show();
        }
    }
}
