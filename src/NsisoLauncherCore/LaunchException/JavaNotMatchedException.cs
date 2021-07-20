using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.LaunchException
{
    public class JavaNotMatchedException : LaunchException
    {
        public JavaVersion RequiredVersion { get; private set; }

        public Java CurrentJava { get; private set; }

        public JavaNotMatchedException(JavaVersion require, Java current) : base("Java version not matched",
            string.Format("This launching minecraft version is required uisng java tag:{0} and version:{1}, but now using java {2}", require.Component, require.MajorVersion, current.MajorVersion))
        {
            RequiredVersion = require;
            CurrentJava = current;
        }
    }
}
