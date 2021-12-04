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
            else
            {
                if (!string.IsNullOrWhiteSpace(authenticateResponse.RawMessage))
                {
                    string raw_trim = response.RawMessage.Trim();
                    if (raw_trim.StartsWith("{") && raw_trim.EndsWith("}"))
                    {
                        authenticateResponse.Error = JsonConvert.DeserializeObject<AuthenticationResponseError>(raw_trim);
                    }
                }

            }
            return authenticateResponse;
        }

        async public Task<Response> Invalidate(AccessClientTokenPair data, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "invalidate"), data, cancellation);
            return response;
        }

        async public Task<TokenResponse> Refresh(RefreshRequest request, CancellationToken cancellation = default)
        {
            Response response = await SendRequest(string.Format("{0}/{1}", ApiAddress, "refresh"), request, cancellation);
            TokenResponse tokenResponse = new TokenResponse(response);
            if (response.IsSuccess)
            {
                tokenResponse.Data = JsonConvert.DeserializeObject<AccessClientTokenPair>(response.RawMessage);
            }
            return tokenResponse;
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
            try
            {
                string request_str = JsonConvert.SerializeObject(request);
                StringContent content = new StringContent(request_str, Encoding.UTF8, "application/json");

                var result = await NetRequester.Client.PostAsync(url, content, cancellation);

                //result.EnsureSuccessStatusCode();

                bool is_success = result.IsSuccessStatusCode;
                string raw = await result.Content.ReadAsStringAsync();
                HttpStatusCode code = result.StatusCode;

                Response response = new Response()
                {
                    RawMessage = raw,
                    Code = code
                };

                ResponseState errState = ResponseState.ERR_OTHER;
                if (is_success)
                {
                    errState = ResponseState.SUCCESS;
                }
                else
                {
                    string raw_trim = raw.Trim();
                    if (raw_trim.StartsWith("{") && raw_trim.EndsWith("}"))
                    {
                        Error error = JsonConvert.DeserializeObject<Error>(raw_trim);
                        response.Error = error;
                    }

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

                    if (response.Error == null)
                    {
                        response.Error = new Error()
                        {
                            ErrorMessage = result.StatusCode.ToString(),
                            ErrorTag = result.StatusCode.ToString()
                        };
                    }
                }
                response.State = errState;
                return response;
            }
            catch (TaskCanceledException ex)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return new Response(ResponseState.CANCELED)
                    {
                        Error = new Error()
                        {
                            ErrorTag = ex.Message,
                            ErrorMessage = ex.Message,
                            Exception = ex
                        }
                    };
                }
                else
                {
                    return new Response(ResponseState.ERR_TIMEOUT)
                    {
                        Error = new Error()
                        {
                            ErrorTag = ex.Message,
                            ErrorMessage = ex.Message,
                            Exception = ex
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response(ResponseState.ERR_INSIDE)
                {
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
