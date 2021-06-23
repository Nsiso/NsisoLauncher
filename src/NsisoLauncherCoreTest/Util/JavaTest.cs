using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NsisoLauncherCore.Util;
using NsisoLauncherCore;

namespace NsisoLauncherCoreTest.Util
{
    [TestClass]
    public class JavaTest
    {
        [TestMethod]
        public void TestGetRuntimeJavaList()
        {
            var list = Java.GetRuntimeRootJavaList(PathManager.RuntimeDirectory);
        }
    }
}
