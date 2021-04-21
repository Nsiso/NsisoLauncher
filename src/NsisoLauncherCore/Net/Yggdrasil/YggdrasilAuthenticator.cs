using NsisoLauncherCore.Modules.Yggdrasil;
using NsisoLauncherCore.Modules.Yggdrasil.Requests;
using NsisoLauncherCore.Modules.Yggdrasil.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;

namespace NsisoLauncherCore.Net.Yggdrasil
{
    public class YggdrasilAuthenticator : IAuthenticator
    {
        public Uri AuthServerUrl { get; set; }

        private NetRequester requester;

        public YggdrasilAuthenticator(Uri authServer, NetRequester requester)
        {
            this.requester = requester;
            this.AuthServerUrl = authServer;
        }

        async public Task<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            Response response = await SendRequest(new Uri(AuthServerUrl, "/authenticate"), request);
            AuthenticateResponse authenticateResponse = new AuthenticateResponse(response);
            if (response.IsSuccess)
            {
                authenticateResponse.Data = JsonConvert.DeserializeObject<AuthenticateResponseData>(response.RawMessage);
            }
            else
            {
                authenticateResponse.Error = JsonConvert.DeserializeObject<AuthenticationResponseError>(response.RawMessage);
            }
            return authenticateResponse;
        }

        async public Task<Response> Invalidate(AccessClientTokenPair data)
        {
            Response response = await SendRequest(new Uri(AuthServerUrl, "/invalidate"), data);
            return response;
        }

        async public Task<TokenResponse> Refresh(RefreshRequest request)
        {
            Response response = await SendRequest(new Uri(AuthServerUrl, "/refresh"), request);
            TokenResponse tokenResponse = new TokenResponse(response);
            if (response.IsSuccess)
            {
                tokenResponse.Data = JsonConvert.DeserializeObject<AccessClientTokenPair>(response.RawMessage);
            }
            return tokenResponse;
        }

        async public Task<Response> Signout(UsernamePasswordPair data)
        {
            Response response = await SendRequest(new Uri(AuthServerUrl, "/signout"), data);
            return response;
        }

        async public Task<Response> Validate(AccessClientTokenPair data)
        {
            Response response = await SendRequest(new Uri(AuthServerUrl, "/validate"), data);
            return response;
        }

        async private Task<Response> SendRequest(Uri url, object request)
        {
            try
            {
                string request_str = JsonConvert.SerializeObject(request);
                StringContent content = new StringContent(request_str, Encoding.UTF8, "application/json");

                var result = await requester.Client.PostAsync(url, content);

                bool is_success = result.IsSuccessStatusCode;
                string raw = await result.Content.ReadAsStringAsync();
                HttpStatusCode code = result.StatusCode;

                Response response = new Response()
                {
                    RawMessage = raw,
                    Code = code,
                    IsSuccess = is_success
                };

                ResponseState errState = ResponseState.ERR_OTHER;
                if (is_success)
                {
                    errState = ResponseState.SUCCESS;
                }
                else
                {
                    Error error = JsonConvert.DeserializeObject<Error>(raw);
                    response.Error = error;

                    switch (result.StatusCode)
                    {
                        case HttpStatusCode.MethodNotAllowed:
                            errState = ResponseState.ERR_METHOD_NOT_ALLOW;
                            break;
                        case HttpStatusCode.NotFound:
                            errState = ResponseState.ERR_NOTFOUND;
                            break;
                        case HttpStatusCode.Forbidden:
                            errState = ResponseState.ERR_INVALID_CRDL;
                            break;
                        default:
                            errState = ResponseState.ERR_OTHER;
                            break;
                    }
                }
                response.State = errState;
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ResponseState.ERR_INSIDE)
                {
                    IsSuccess = false,
                    Error = new Error()
                    {
                        ErrorTag = ex.Message,
                        ErrorMessage = ex.Message,
                        Exception = ex
                    }
                };
            }
        }
    }
}
