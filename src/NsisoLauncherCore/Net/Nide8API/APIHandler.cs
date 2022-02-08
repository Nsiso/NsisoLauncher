using Heijden.DNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Nide8API
{
    public class APIHandler
    {
        public string Nide8BaseUrl { get; set; } = "https://auth2.nide8.com:233";

        public string Nide8Id { get; private set; }

        public string Nide8PublicId => "00000000000000000000000000000000";

        public APIHandler(string id)
        {
            this.Nide8Id = id;
        }

        public async Task<APIModules> GetInfoAsync()
        {
            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(string.Format("{0}/{1}", Nide8BaseUrl, Nide8Id));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return await Task.Factory.StartNew(() =>
            {
                return JsonConvert.DeserializeObject<APIModules>(json);
            });
        }

        public async Task<bool> TestIsNide8DnsOK()
        {
            Response response = null;
            await Task.Factory.StartNew(() =>
            {
                Resolver resolver = new Resolver();
                response = resolver.Query("auth2.nide8.com", Heijden.DNS.QType.A);
            });
            if (string.IsNullOrWhiteSpace(response.Error) && response.RecordsA.Length != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> GetJarVersion()
        {
            HttpResponseMessage jsonRespond = await NetRequester.HttpGetAsync(string.Format("{0}/{1}", Nide8BaseUrl, Nide8PublicId));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode)
            {
                json = await jsonRespond.Content.ReadAsStringAsync();
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return await Task.Factory.StartNew(() =>
            {
                JObject jobj = JObject.Parse(json);
                return jobj["jarVersion"].Value<string>();
            });
        }
    }
}
