using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public static class APIRequester
    {
        public async static Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetAsync(uri);
            }
        }

        public async static Task<string> HttpGetStringAsync(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }

        public async static Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.PostAsync(uri, new FormUrlEncodedContent(arg));
            }
        }

        public async static Task<string> HttpPostReadAsStringForString(string uri, Dictionary<string, string> arg)
        {
            var result = await HttpPostAsync(uri, arg);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
