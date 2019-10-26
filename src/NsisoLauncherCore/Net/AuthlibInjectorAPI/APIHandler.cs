using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.AuthlibInjectorAPI
{
    public class APIHandler
    {
        public async Task<DownloadTask> GetLatestAICoreDownloadTask(DownloadSource source, string downloadTo)
        {
            string apiBase = string.Empty;
            switch (source)
            {
                case DownloadSource.Mojang:
                    apiBase = "https://authlib-injector.yushi.moe/artifact/latest.json";
                    break;
                case DownloadSource.BMCLAPI:
                    apiBase = "https://bmclapi2.bangbang93.com/mirrors/authlib-injector/artifact/latest.json";
                    break;
                default:
                    apiBase = "https://authlib-injector.yushi.moe/artifact/latest.json";
                    break;
            }
            var jobj = JObject.Parse(await APIRequester.HttpGetStringAsync(apiBase));
            string downloadURL = jobj.Value<string>("download_url");
            string sha256 = jobj["checksums"].Value<string>("sha256");
            return new DownloadTask("AuthlibInjector核心", downloadURL, downloadTo)
            {
                Checker = new Util.Checker.SHA256Checker()
                {
                    CheckSum = sha256,
                    FilePath = downloadTo
                }
            };
        }
    }
}
