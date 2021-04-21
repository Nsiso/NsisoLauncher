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
    public class NetRequester : IDisposable
    {
        /// <summary>
        /// NsisoLauncher目前名称.
        /// </summary>
        public string ClientName { get; set; } = "NsisoLauncher";

        /// <summary>
        /// NsisoLauncher目前版本号.
        /// </summary>
        public string ClientVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);

        /// <summary>
        /// 使用的代理服务
        /// </summary>
        public IWebProxy NetProxy
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

        private HttpClient _client;
        /// <summary>
        /// 表示Web请求中使用的http客户端.
        /// </summary>
        public HttpClient Client
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

        private HttpClientHandler _clientHandler;
        /// <summary>
        /// 表示http客户端handler
        /// </summary>
        public HttpClientHandler ClientHandler
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

        public async Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg)
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

        public async Task<HttpResponseMessage> HttpPostAsync(string uri, Dictionary<string, string> arg, CancellationToken cancellation)
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

        public void Dispose()
        {
            if (_clientHandler != null)
            {
                _clientHandler.Dispose();
            }
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}