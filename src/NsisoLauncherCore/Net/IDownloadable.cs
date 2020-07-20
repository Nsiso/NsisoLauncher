namespace NsisoLauncherCore.Net
{
    public interface IDownloadable
    {
        /// <summary>
        /// 获得下载源地址
        /// </summary>
        /// <returns>下载源地址</returns>
        string GetDownloadSourceURL();
    }

    public class StringUrl : IDownloadable
    {
        public string From { get; set; }
        public StringUrl(string from)
        {
            this.From = from;
        }
        public string GetDownloadSourceURL()
        {
            return From;
        }
    }
}
