using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public static class APIRequester
    {
        public async static Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetAsync(uri);
                }
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }

        public async static Task<string> HttpGetStringAsync(string uri)
        {
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync(uri);
                }
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public async static Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.PostAsync(uri, new FormUrlEncodedContent(arg));
                }
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }

        public async static Task<string> HttpPostReadAsStringForString(string uri, Dictionary<string, string> arg)
        {
            try
            {
                var result = await HttpPostAsync(uri, arg);
                return await result.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
    }
}
