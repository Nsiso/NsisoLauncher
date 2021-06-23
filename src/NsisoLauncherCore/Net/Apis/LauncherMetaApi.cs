using Newtonsoft.Json;
using NsisoLauncherCore.Net.Apis.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Apis
{
    public class LauncherMetaApi
    {
        public string BaseUrl { get => "https://launchermeta.mojang.com"; }

        private NetRequester _requester;

        public LauncherMetaApi(NetRequester requester)
        {
            if (requester == null)
            {
                throw new ArgumentNullException("net requester is null");
            }
            _requester = requester;
        }

        public async Task<VersionManifest> GetVersionManifest(CancellationToken cancellation = default)
        {
            string url = BaseUrl + "/mc/game/version_manifest.json";

            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VersionManifest>(json_str);
        }

        public async Task<JavaAll> GetJavaAll(CancellationToken cancellation = default)
        {
            string url = BaseUrl + "/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";

            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JavaAll>(json_str);
        }

        public async Task<JavaManifest> GetJavaManifest(JavaMeta java, CancellationToken cancellation = default)
        {
            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(java.Manifest.Url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JavaManifest>(json_str);
        }

        public async Task<JavaManifest> GetJavaManifest(string gamecore, CancellationToken cancellation = default)
        {
            JavaAll javas = await GetJavaAll(cancellation);
            JavaMeta java = null;
            OsType os = SystemTools.GetOsType();
            ArchEnum arch = SystemTools.GetSystemArch();
            switch (os)
            {
                case OsType.Windows:
                    switch (arch)
                    {
                        case ArchEnum.x32:
                            java = javas.Windows_x86[gamecore].FirstOrDefault();
                            break;
                        case ArchEnum.x64:
                            java = javas.Windows_x64[gamecore].FirstOrDefault();
                            break;
                        default:
                            java = javas.Windows_x86[gamecore].FirstOrDefault();
                            break;
                    }
                    break;
                case OsType.Linux:
                    java = javas.Linux[gamecore].FirstOrDefault();
                    break;
                case OsType.MacOS:
                    java = javas.MacOS[gamecore].FirstOrDefault();
                    break;
                default:
                    break;
            }
            HttpResponseMessage jsonRespond = await _requester.Client.GetAsync(java.Manifest.Url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JavaManifest>(json_str);
        }
    }
}
