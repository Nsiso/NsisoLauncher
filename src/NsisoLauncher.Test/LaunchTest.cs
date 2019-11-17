using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsisoLauncherCore;
using NsisoLauncherCore.Util;

namespace NsisoLauncher.Test
{
    [TestClass]
    public class LaunchTest
    {
        LaunchHandler launchHandler;

        [TestMethod]
        public void TestLaunching()
        {
            launchHandler = new LaunchHandler(".minecraft/", Java.GetSuitableJava(), true);
        }
    }
}
