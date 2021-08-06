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

        public override string GetJvmLaunchArguments()
        {
            return "-Djava.library.path=${natives_directory} -cp ${classpath}";
        }

        public override string GetGameLaunchArguments()
        {
            return MinecraftArguments;
        }
    }
}
