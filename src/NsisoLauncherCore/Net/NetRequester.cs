using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net
{
    public static class NetRequester
    {
        /// <summary>
        /// NsisoLauncher目前名称.
        /// </summary>
        public static string ClientName { get; set; } = "NsisoLauncher";

        /// <summary>
        /// NsisoLauncher目前版本号.
        /// </summary>
        public static string ClientVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);

        /// <summary>
        /// 使用的代理服务
        /// </summary>
        public static IWebProxy NetProxy
        {
            get
            {
                return ClientHandler.Proxy;
            }
            set
            {
                ClientHandler.Proxy = value;
            }
        }

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
                    _client = new HttpClient(ClientHandler) {/* Timeout = NetRequester.Timeout */};
                    _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(ClientName, ClientVersion));
                    _client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(CultureInfo.CurrentCulture.Name));
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

        public static async Task<string> HttpGetStringAsync(Uri uri)
        {
            try
            {
                return await Client.GetStringAsync(uri);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public static async Task<string> HttpGetStringAsync(string uri)
        {
            try
            {
                return await Client.GetStringAsync(uri);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(Uri uri, HttpCompletionOption option, CancellationToken cancellation)
        {
            try
            {
                return await Client.GetAsync(uri, option, cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(Uri uri)
        {
            try
            {
                return await Client.GetAsync(uri);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(Uri uri, CancellationToken cancellation)
        {
            try
            {
                return await Client.GetAsync(uri, cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(string uri)
        {
            try
            {
                return await Client.GetAsync(uri);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpGetAsync(string uri, CancellationToken cancellation)
        {
            try
            {
                return await Client.GetAsync(uri, cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(Uri uri, HttpContent arg)
        {
            try
            {
                return await Client.PostAsync(uri, arg);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(Uri uri, Dictionary<string, string> arg)
        {
            try
            {
                return await Client.PostAsync(uri, new FormUrlEncodedContent(arg));
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
        {
            try
            {
                return await Client.PostAsync(uri, new FormUrlEncodedContent(arg));
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(Uri uri, HttpContent arg, CancellationToken cancellation)
        {
            try
            {
                return await Client.PostAsync(uri, arg, cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(Uri uri, Dictionary<string, string> arg, CancellationToken cancellation)
        {
            try
            {
                return await Client.PostAsync(uri, new FormUrlEncodedContent(arg), cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg, CancellationToken cancellation)
        {
            try
            {
                return await Client.PostAsync(uri, new FormUrlEncodedContent(arg), cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpSendAsync(HttpRequestMessage httpRequest)
        {
            try
            {
                return await Client.SendAsync(httpRequest);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> HttpSendAsync(HttpRequestMessage httpRequest, CancellationToken cancellation)
        {
            try
            {
                return await Client.SendAsync(httpRequest, cancellation);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
    }
}