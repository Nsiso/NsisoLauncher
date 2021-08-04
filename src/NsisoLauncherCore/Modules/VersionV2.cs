using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Modules
{
    public class VersionV2 : VersionBase
    {
        public VersionArguments Arguments { get; set; }

        public override string ToLaunchArgument(ArgumentsParser parser, LaunchSetting setting)
        {
            return parser.ParseV2(this, setting);
        }
    }

    public class VersionArguments
    {
        public List<JToken> Game { get; set; }

        public List<JToken> Jvm { get; set; }
    }
}
