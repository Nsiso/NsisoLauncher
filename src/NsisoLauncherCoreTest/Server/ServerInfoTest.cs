using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsisoLauncherCore.Net.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCoreTest.Server
{
    [TestClass]
    public class ServerInfoTest
    {
        [TestMethod]
        public void TestGetServerInfo()
        {
            ServerInfo serverInfo = new ServerInfo("mc.hypixel.net", 25565);
            serverInfo.StartGetServerInfo();
            Assert.IsTrue(serverInfo.State == ServerInfo.StateType.GOOD);
            Console.WriteLine(serverInfo.Description);
        }
    }
}
