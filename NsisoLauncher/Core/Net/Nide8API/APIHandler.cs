using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NsisoLauncher.Core.Net.Nide8API
{
    public class APIHandler
    {
        public string BaseURL { get; private set; }
        public string ServerID { get; private set; }
        public APIHandler(string id)
        {
            ServerID = id;
            BaseURL = string.Format("https://auth2.nide8.com:233/{0}/", ServerID);
        }

        public Task<APIModules> GetInfoAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return JsonConvert.DeserializeObject<APIModules>(FunctionAPI.FunctionAPIHandler.HttpGet(BaseURL));
            });
        }

        public Task UpdateBaseURL()
        {
            return Task.Factory.StartNew(() =>
            {
                APIModules module = JsonConvert.DeserializeObject<APIModules>(FunctionAPI.FunctionAPIHandler.HttpGet(BaseURL));
                if (!string.IsNullOrWhiteSpace(module.APIRoot))
                {
                    this.BaseURL = module.APIRoot;
                }
            });
        }
    }
}
