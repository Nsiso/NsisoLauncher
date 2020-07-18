using System;

namespace NsisoLauncherCore.LaunchException
{
    [Serializable]
    public class LaunchException : Exception
    {
        public LaunchException(string title, string message) : base(message)
        {
            Title = title;
        }

        public LaunchException(Exception ex) : base("在启动时发生意外错误:\n" + ex, ex)
        {
            Title = "启动时发生意外错误";
        }

        /// <summary>
        ///     异常标题
        /// </summary>
        public string Title { get; set; }
    }
}