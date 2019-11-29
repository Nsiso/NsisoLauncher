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

        public LaunchTest()
        {
            CreateLaunchHandler();
        }

        public void CreateLaunchHandler()
        {
            launchHandler = new LaunchHandler(".minecraft/", Java.GetSuitableJava(), true);
        }

        [TestMethod]
        public void TestGetVersions()
        {
            VersionReader versionReader = new VersionReader(launchHandler);
            var vers = versionReader.GetVersions();
            Assert.IsFalse(vers.Count == 0);
        }
    }
}
