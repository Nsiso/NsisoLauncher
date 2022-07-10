using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;

namespace NsisoLauncherCore.Net.Nide8API
{
    public class APIHandler
    {
        public string Nide8BaseUrl { get; set; } = "https://auth.mc-user.com:233";

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
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync("auth.mc-user.com", QueryType.A);
            return !result.HasError && result.Answers.Count != 0;
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
