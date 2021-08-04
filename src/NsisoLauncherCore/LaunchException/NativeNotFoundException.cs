using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.LaunchException
{
    public class NativeNotFoundException : LaunchException
    {
        public Native LostNative { get; private set; }

        public string LostPath { get; private set; }

        public NativeNotFoundException(Native lostNative, string lostPath) : base("缺失Native库文件", string.Format("无法找到指定的Native库文件{0},游戏是否完整?路径:{1}", lostNative.Name.Name, lostPath))
        {
            this.LostNative = lostNative;
            this.LostPath = LostPath;
            this.HelpLink = @"https://github.com/Nsiso/NsisoLauncher5/wiki/%E5%90%AF%E5%8A%A8%E5%BC%82%E5%B8%B8-:-NativeNotFoundException";
        }
    }
}