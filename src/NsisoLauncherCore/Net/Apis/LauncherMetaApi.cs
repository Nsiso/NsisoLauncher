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
    public static class LauncherMetaApi
    {
        public const string BaseUrl = "https://launchermeta.mojang.com";

        public static async Task<VersionManifest> GetVersionManifest(CancellationToken cancellation = default)
        {
            string url = BaseUrl + "/mc/game/version_manifest.json";

            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VersionManifest>(json_str);
        }

        public static async Task<JavaAll> GetJavaAll(CancellationToken cancellation = default)
        {
            string url = BaseUrl + "/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";

            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JavaAll>(json_str);
        }

        public static async Task<JavaManifest> GetJavaManifest(JavaMeta java, CancellationToken cancellation = default)
        {
            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(java.Manifest.Url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JavaManifest>(json_str);
        }

        public static async Task<NativeJavaMeta> GetNativeJavaMeta(string gamecore, CancellationToken cancellation = default)
        {
            JavaAll javas = await GetJavaAll(cancellation);
            if (javas == null)
            {
                throw new Exception("The java list is null");
            }
            JavaMeta java = null;
            OsType os = SystemTools.GetOsType();
            ArchEnum arch = SystemTools.GetSystemArch();
            string os_info = null;
            switch (os)
            {
                case OsType.Windows:
                    os_info = "windows";
                    switch (arch)
                    {
                        case ArchEnum.x32:
                            os_info += "-x86";
                            java = javas.Windows_x86[gamecore].FirstOrDefault();
                            break;
                        case ArchEnum.x64:
                            os_info += "-x64";
                            java = javas.Windows_x64[gamecore].FirstOrDefault();
                            break;
                        default:
                            os_info += "-x86";
                            java = javas.Windows_x86[gamecore].FirstOrDefault();
                            break;
                    }
                    break;
                case OsType.Linux:
                    os_info = "linux";
                    java = javas.Linux[gamecore].FirstOrDefault();
                    break;
                case OsType.MacOS:
                    os_info = "mac-os";
                    java = javas.MacOS[gamecore].FirstOrDefault();
                    break;
                default:
                    break;
            }

            if (java == null)
            {
                throw new Exception("The selected java is null in get native java meta");
            }

            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(java.Manifest.Url, cancellation);
            jsonRespond.EnsureSuccessStatusCode();
            string json_str = await jsonRespond.Content.ReadAsStringAsync();
            JavaManifest manifest = JsonConvert.DeserializeObject<JavaManifest>(json_str);

            NativeJavaMeta nativeJavaMeta = new NativeJavaMeta()
            {
                Manifest = manifest,
                Tag = gamecore,
                Version = java.Version.Name,
                OsInfo = os_info
            };


            return nativeJavaMeta;
        }
    }
}
