using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Net.Tools;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.FunctionAPI
{
    public class FunctionAPIHandler
    {
        public IList<IFunctionalMirror> FunctionalMirrorList { get; set; }
        public IList<IVersionListMirror> VersionListMirrorList { get; set; }

        private NetRequester _requester;

        public FunctionAPIHandler(IList<IVersionListMirror> versionListMirrors, IList<IFunctionalMirror> functionMirrors, NetRequester requester)
        {
            FunctionalMirrorList = functionMirrors ?? throw new ArgumentNullException("FunctionalMirror is null");
            VersionListMirrorList = versionListMirrors ?? throw new ArgumentNullException("VersionListMirror is null");
            _requester = requester ?? throw new ArgumentNullException("NetRequester is null");
        }

        /// <summary>
        /// 联网获取版本列表
        /// </summary>
        /// <returns>版本列表</returns>
        public async Task<VersionManifest> GetVersionManifest()
        {
            Uri versionListUri;
            IVersionListMirror mirror = (IVersionListMirror)await MirrorHelper.ChooseBestMirror(VersionListMirrorList);
            versionListUri = mirror == null ? new Uri(GetDownloadUri.MojangVersionUrl) : mirror.VersionListUri;
            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(versionListUri);
            jsonRespond.EnsureSuccessStatusCode();
            string json = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VersionManifest>(json);
        }

        /// <summary>
        /// 联网获取JAVA列表
        /// </summary>
        /// <returns>JAVA列表</returns>
        public async Task<List<JWJava>> GetJavaList()
        {
            Uri javaListUri;
            IFunctionalMirror mirror = (IFunctionalMirror)await MirrorHelper.ChooseBestMirror(FunctionalMirrorList);
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                javaListUri = mirror.JavaListUri;
            }
            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(javaListUri);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var e = JsonConvert.DeserializeObject<List<JWJava>>(json);
            return e;
        }

        /// <summary>
        /// 联网获取指定版本所有的FORGE
        /// </summary>
        /// <param name="version">要搜索的版本</param>
        /// <returns>Forge列表</returns>
        public async Task<List<JWForge>> GetForgeList(VersionBase version)
        {
            Uri forgeListUri;
            IFunctionalMirror mirror = (IFunctionalMirror)await MirrorHelper.ChooseBestMirror(FunctionalMirrorList);
            if (mirror == null)
            {
                throw new Exception("The Functional Mirror is null");
            }
            else
            {
                forgeListUri = mirror.ForgeListUri;
            }
            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(string.Format("{0}/{1}", forgeListUri, version.Id));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var e = JsonConvert.DeserializeObject<List<JWForge>>(json);
            return e;
        }
    }
}
