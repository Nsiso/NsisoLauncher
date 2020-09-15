using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public class Proxy
    {
        public string ProxyHost { get; set; }

        public int ProxyPort { get; set; }

        public string ProxyUsername { get; set; }

        public string ProxyPassword { get; set; }
    }
}
