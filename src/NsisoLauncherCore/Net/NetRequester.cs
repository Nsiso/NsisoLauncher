using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public static class NetRequester
    {
        /// <summary>
        /// NsisoLauncher5目前名称.
        /// </summary>
        public readonly static string ClientName = "NsisoLauncher";

        /// <summary>
        /// NsisoLauncher5目前版本号.
        /// </summary>
        public readonly static string ClientVersion = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();

        private static HttpClient _client;
        /// <summary>
        /// 表示Web请求中使用的http客户端.
        /// </summary>
        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(ClientHandler) { Timeout = NetRequester.Timeout };
                    _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(ClientName, ClientVersion));
                }
                return _client;
            }
            private set
            {
                _client = value;
            }
        }

        private static HttpClientHandler _clientHandler;
        /// <summary>
        /// 表示http客户端handler
        /// </summary>
        public static HttpClientHandler ClientHandler
        {
            get
            {
                if (_clientHandler == null)
                {
                    _clientHandler = new HttpClientHandler();
                }
                return _clientHandler;
            }
            set
            {
                _clientHandler = value;
            }
        }

        /// <summary>
        /// 定义http请求的超时时间.
        /// </summary>
        public static TimeSpan Timeout = TimeSpan.FromSeconds(10);


        public async static Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            try
            {
                return await Client.GetAsync(uri);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public async static Task<string> HttpGetStringAsync(string uri)
        {

            try
            {
                return await Client.GetStringAsync(uri);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            try
            {
                return await Client.PostAsync(uri, new FormUrlEncodedContent(arg));
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public async static Task<string> HttpPostReadAsStringForString(string uri, Dictionary<string, string> arg)
        {
            try
            {
                var result = await HttpPostAsync(uri, arg);
                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
