using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Modules
{
    public class VersionV1 : VersionBase
    {
        public string MinecraftArguments { get; set; }

        public override string ToLaunchArgument(ArgumentsParser parser, LaunchSetting setting)
        {
            return parser.ParseV1(this, setting);
        }
    }
}
