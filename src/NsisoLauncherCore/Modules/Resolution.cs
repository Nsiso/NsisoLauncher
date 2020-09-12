namespace NsisoLauncherCore.Modules
{
    public class Resolution
    {
        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool FullScreen { get; set; }

        /// <summary>
        /// 高px
        /// </summary>
        public ushort Height { get; set; }

        /// <summary>
        /// 宽px
        /// </summary>
        public ushort Width { get; set; }

        public Resolution()
        {
            Height = 0;
            Width = 0;
            FullScreen = false;
        }
    }
}
