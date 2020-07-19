using Heijden.DNS;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Nide8API
{
    public class APIHandler
    {
        private NetRequester _netRequester;
        public APIHandler(NetRequester requester)
        {
            _netRequester = requester;
        }
        public string Nide8ID { get; private set; }
        public APIHandler(string id)
        {
            Nide8ID = id;
        }

        public async Task<APIModules> GetInfoAsync()
        {
            HttpResponseMessage jsonRespond = await _netRequester.Client.GetAsync(string.Format("https://auth2.nide8.com:233/{0}", Nide8ID));
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
    }
}
