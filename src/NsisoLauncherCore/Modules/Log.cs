using System;

namespace NsisoLauncherCore.Modules
{
    public enum LogLevel
    {
        GAME,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }

    public class Log : EventArgs
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// LOG信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; set; }

        public Log(LogLevel level, string msg)
        {
            LogLevel = level;
            Time = DateTime.Now;
            Message = msg;
        }
        public Log(LogLevel level, string msg, Exception exc)
        {
            LogLevel = level;
            Time = DateTime.Now;
            Message = msg;
            Exception = exc;
        }

        public override string ToString()
        {
            return this.ToString(false);
        }
        public string ToString(bool without_time)
        {
            if (without_time)
            {
                return string.Format("[{0}]{1}", this.LogLevel.ToString(), this.Message);
            }
            else
            {
                return string.Format("[{0}][{1}]{2}", this.LogLevel.ToString(), this.Time.ToString(), this.Message);
            }
        }

    }
}
