using Newtonsoft.Json;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Apis
{
    public class YggdrasilApi
    {
        public string ApiAddress { get; set; }

        public YggdrasilApi(string authServer)
        {
            this.ApiAddress = authServer;
        }

        public YggdrasilApi()
        {
            this.ApiAddress = "https://authserver.mojang.com";
        }

        async public Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "authenticate"), request, cancellation);
            AuthenticateResponse authenticateResponse = new AuthenticateResponse(response);
            if (response.IsSuccess)
            {
                authenticateResponse.Data = JsonConvert.DeserializeObject<AuthenticateResponseData>(response.RawMessage);
            }
            return authenticateResponse;
        }

        async public Task<Response> Invalidate(AccessClientTokenPair data, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "invalidate"), data, cancellation);
            return response;
        }

        async public Task<AuthenticateResponse> Refresh(RefreshRequest request, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "refresh"), request, cancellation);
            AuthenticateResponse refreshResponse = new AuthenticateResponse(response);
            if (response.IsSuccess)
            {
                refreshResponse.Data = JsonConvert.DeserializeObject<AuthenticateResponseData>(response.RawMessage);
            }
            return refreshResponse;
        }

        async public Task<Response> Signout(UsernamePasswordPair data, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "signout"), data, cancellation);
            return response;
        }

        async public Task<Response> Validate(AccessClientTokenPair data, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "validate"), data, cancellation);
            return response;
        }

        async private Task<Response> SendRequest(string url, object request, CancellationToken cancellation = default)
        {
            string request_str = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(request_str, Encoding.UTF8, "application/json");

            var result = await NetRequester.Client.PostAsync(url, content, cancellation);

            bool is_success = result.IsSuccessStatusCode;
            string raw = await result.Content.ReadAsStringAsync();
            string raw_trim = raw.Trim();
            HttpStatusCode code = result.StatusCode;

            Response response = new Response()
            {
                RawMessage = raw,
                Code = code,
                IsSuccess = is_success,
            };

            if (!is_success)
            {
                if (result.Content.Headers.ContentType.MediaType == "application/json")
                {
                    response.Error = JsonConvert.DeserializeObject<Error>(raw_trim);
                }
                else
                {
                    response.Error = new Error() { ErrorTag = code.ToString(), ErrorMessage = raw_trim };
                }
            }

            return response;
        }
    }
}
