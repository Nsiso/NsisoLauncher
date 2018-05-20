using NsisoLauncher.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NsisoLauncher.Core.LaunchException
{
    public class NativeNotFoundException : Exception
    {
        public Native LostNative { get; private set; }

        public string LostPath { get; private set; }

        public NativeNotFoundException(Native lostNative, string lostPath) : base(string.Format("缺失Native库文件{0},路径不存在:{1}", lostNative.Name, lostPath))
        {
            this.LostNative = lostNative;
            this.LostPath = LostPath;
            this.HelpLink = @"https://github.com/Nsiso/NsisoLauncher/wiki/%E5%90%AF%E5%8A%A8%E5%BC%82%E5%B8%B8-:-NativeNotFoundException";
        }
    }
}
