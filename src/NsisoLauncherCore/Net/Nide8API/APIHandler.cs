using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.Nide8API
{
    public class APIHandler
    {
        private readonly NetRequester _netRequester;

        public APIHandler(NetRequester requester)
        {
            _netRequester = requester;
        }

        public APIHandler(string id)
        {
            Nide8ID = id;
        }

        public string Nide8ID { get; }

        public async Task<APIModules> GetInfoAsync()
        {
            var jsonRespond =
                await _netRequester.Client.GetAsync(string.Format("https://auth2.nide8.com:233/{0}", Nide8ID));
            string json = null;
            if (jsonRespond.IsSuccessStatusCode) json = await jsonRespond.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            return await Task.Factory.StartNew(() => { return JsonConvert.DeserializeObject<APIModules>(json); });
        }

        public async Task<bool> TestIsNide8DnsOK()
        {
            var query = await Dns.GetHostEntryAsync("auth2.nide8.com").ConfigureAwait(false);
            return query?.AddressList?.Length > 0;
        }
    }
}