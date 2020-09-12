namespace NsisoLauncherCore.Net
{
    /// <summary>
    /// 下载设置
    /// </summary>
    public class DownloadSetting
    {
        public DownloadProtocolType ProtocolType { get; set; }

        public int RetryTimes { get; set; }

        public bool CheckFileHash { get; set; }
    }
}
