namespace NsisoLauncherCore.LaunchException
{
    public class NullJavaException : LaunchException
    {
        public NullJavaException() : base("未设置有效JAVA，无法进行所选操作", "当前启动核心中未设置有效JAVA（即空指针），请设置非空有效JAVA")
        {
        }
    }
}
