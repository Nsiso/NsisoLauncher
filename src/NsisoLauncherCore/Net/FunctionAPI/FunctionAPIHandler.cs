using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        private readonly NetRequester _requester;

        public FunctionAPIHandler(IList<IVersionListMirror> versionListMirrors,
            IList<IFunctionalMirror> functionMirrors, NetRequester requester)
        {
            FunctionalMirrorList = functionMirrors ?? throw new ArgumentNullException("FunctionalMirror is null");
            VersionListMirrorList = versionListMirrors ?? throw new ArgumentNullException("VersionListMirror is null");
            _requester = requester ?? throw new ArgumentNullException("NetRequester is null");
        }

        public IList<IFunctionalMirror> FunctionalMirrorList { get; set; }
        public IList<IVersionListMirror> VersionListMirrorList { get; set; }

        /// <summary>
        ///     联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public async Task<List<JWVersion>> GetVersionList()
        {
            Uri versionListUri;
            var mirror = (IVersionListMirror) await MirrorHelper.ChooseBestMirror(VersionListMirrorList);
            if (mirror == null)
                versionListUri = new Uri(GetDownloadUri.MojangVersionUrl);
            else
                versionListUri = mirror.VersionListUri;
            var jsonRespond = await _requester.Client.GetAsync(versionListUri);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode) json = await jsonRespond.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(json))
            {
                var e = JsonConvert.DeserializeObject<JWVersions>(json);
                return e.Versions;
            }

            return null;
        }

        /// <summary>
        ///     联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public async Task<List<JWJava>> GetJavaList()
        {
            Uri javaListUri;
            var mirror = (IFunctionalMirror) await MirrorHelper.ChooseBestMirror(FunctionalMirrorList);
            if (mirror == null)
                throw new Exception("The Functional Mirror is null");
            javaListUri = mirror.JavaListUri;
            var jsonRespond = await _requester.Client.GetAsync(javaListUri);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode) json = await jsonRespond.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            var e = JsonConvert.DeserializeObject<List<JWJava>>(json);
            return e;
        }

        /// <summary>
        ///     联网获取指定版本所有的FORGE
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Forge列表</returns>
        public async Task<List<JWForge>> GetForgeList(Version version)
        {
            Uri forgeListUri;
            var mirror = (IFunctionalMirror) await MirrorHelper.ChooseBestMirror(FunctionalMirrorList);
            if (mirror == null)
                throw new Exception("The Functional Mirror is null");
            forgeListUri = mirror.ForgeListUri;
            var jsonRespond = await _requester.Client.GetAsync(new Uri(forgeListUri, version.ID));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode) json = await jsonRespond.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            var e = JsonConvert.DeserializeObject<List<JWForge>>(json);
            return e;
        }
    }
}