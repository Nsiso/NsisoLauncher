using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Apis;
using System;
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
        }
    }
}
