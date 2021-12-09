using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NsisoLauncherCoreTest.Apis
{
    [TestClass]
    public class LauncherMetaApiTest
    {
        [TestMethod]
        public async Task TestGetVersionManifest()
        {
            var manifest = await LauncherMetaApi.GetVersionManifest();
            Assert.IsNotNull(manifest);
            Assert.IsNotNull(manifest.Latest);
            Assert.IsNotNull(manifest.Versions);
            Console.WriteLine("Test get version manifest success! There is {0} versions in this manifest.", manifest.Versions.Count);
        }

        [TestMethod]
        public async Task TestGetJavaAll()
        {
            var result = await LauncherMetaApi.GetJavaAll();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Windows_x64);
            Assert.IsNotNull(result.Windows_x86);
            Assert.IsNotNull(result.MacOS);
            Assert.IsNotNull(result.Linux);
            Assert.IsNotNull(result.LinuxI386);
        }

        [TestMethod]
        public async Task TestGetNativeJavaMeta()
        {
            var javas = await LauncherMetaApi.GetJavaAll();
            Assert.IsNotNull(javas);
            foreach (var item in javas.Gamecore)
            {
                var result = await LauncherMetaApi.GetNativeJavaMeta(item.Key);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Manifest);
                Assert.IsNotNull(result.Manifest.Files);
                Assert.IsTrue(result.Manifest.Files.Count != 0);
            }
        }
    }
}
