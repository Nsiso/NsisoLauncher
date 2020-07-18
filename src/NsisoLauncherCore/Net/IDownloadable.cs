namespace NsisoLauncherCore.Net
{
    public interface IDownloadable
    {
        /// <summary>
        ///     获得下载源地址
        /// </summary>
        /// <returns>下载源地址</returns>
        string GetDownloadSourceURL();
    }

    public class StringUrl : IDownloadable
    {
        public StringUrl(string from)
        {
            From = from;
        }

        public string From { get; set; }

        public string GetDownloadSourceURL()
        {
            return From;
        }
    }
}