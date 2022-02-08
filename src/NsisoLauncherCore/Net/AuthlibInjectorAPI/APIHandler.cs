using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.AuthlibInjectorAPI
{
    public class APIHandler
    {
        public string ApiUrlBase { get; set; }
        public string LatestUrl => ApiUrlBase + "/artifact/latest.json";

        public APIHandler()
        {
            ApiUrlBase = "https://authlib-injector.yushi.moe";
        }

        public APIHandler(DownloadSource source)
        {
            switch (source)
            {
                case DownloadSource.Mojang:
                    ApiUrlBase = "https://authlib-injector.yushi.moe";
                    break;
                case DownloadSource.BMCLAPI:
                    ApiUrlBase = "https://bmclapi2.bangbang93.com/mirrors/authlib-injector";
                    break;
                default:
                    ApiUrlBase = "https://authlib-injector.yushi.moe";
                    break;
            }
        }

        public async Task<Library> GetCoreLibraryAsync()
        {
            var jsonRespond = await NetRequester.HttpGetAsync(LatestUrl);
            jsonRespond.EnsureSuccessStatusCode();
            string json = await jsonRespond.Content.ReadAsStringAsync();
            LatestArtifact latestArtifact = JsonConvert.DeserializeObject<LatestArtifact>(json);
            Library library = new Library()
            {
                Url = latestArtifact.DownloadUrl,
                Name = new Artifact(string.Format("moe.yushi:authlibinjector:{0}", latestArtifact.Version))
            };
            return library;
        }

        public async Task<DownloadTask> GetLatestAICoreDownloadTask(string downloadTo)
        {
            var jsonRespond = await NetRequester.HttpGetAsync(LatestUrl);
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
