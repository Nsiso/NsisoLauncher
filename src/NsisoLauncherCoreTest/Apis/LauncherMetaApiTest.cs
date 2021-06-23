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
        private NetRequester _requester;
        private LauncherMetaApi _api;
        public LauncherMetaApiTest()
        {
            _requester = new NetRequester();
            _api = new LauncherMetaApi(_requester);
        }

        [TestMethod]
        public async Task TestGetVersionManifest()
        {
            var manifest = await _api.GetVersionManifest();
            Assert.IsNotNull(manifest);
            Assert.IsNotNull(manifest.Latest);
            Assert.IsNotNull(manifest.Versions);
            Console.WriteLine("Test get version manifest success! There is {0} versions in this manifest.", manifest.Versions.Count);
        }

        [TestMethod]
        public async Task TestGetJavaAll()
        {
            var result = await _api.GetJavaAll();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Windows_x64);
            Assert.IsNotNull(result.Windows_x86);
            Assert.IsNotNull(result.MacOS);
            Assert.IsNotNull(result.Linux);
            Assert.IsNotNull(result.LinuxI386);
        }

        [TestMethod]
        public async Task TestGetJavaManifest()
        {
            var javas = await _api.GetJavaAll();
            Assert.IsNotNull(javas);
            foreach (var item in javas.Gamecore)
            {
                var result = await _api.GetJavaManifest(item.Key);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Files);
                Assert.IsTrue(result.Files.Count != 0);
            }
        }
    }
}
