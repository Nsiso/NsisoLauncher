using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.AuthlibInjectorAPI
{
    public class APIHandler
    {
        public async Task<DownloadTask> GetLatestAICoreDownloadTask(DownloadSource source, string downloadTo)
        {
            string apiBase;
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
            var jsonRespond = await NetRequester.HttpGetAsync(apiBase);
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            var jobj = JObject.Parse(json);
            string downloadURL = jobj.Value<string>("download_url");
            string sha256 = jobj["checksums"].Value<string>("sha256");
            DownloadTask downloadTask = new DownloadTask("AuthlibInjector核心", new StringUrl(downloadURL), downloadTo);
            downloadTask.DownloadObject.Checker = new Util.Checker.SHA256Checker()
            {
                CheckSum = sha256,
                FilePath = downloadTo
            };
            return downloadTask;
        }
    }
}
