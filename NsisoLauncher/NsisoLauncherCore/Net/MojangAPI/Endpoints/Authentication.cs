using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// 代表一对用于验证的用户名和密码
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// 验证请求类
    /// </summary>
    public class Authenticate : IEndpoint<AuthenticateResponse>
    {

        /// <summary>
        /// 发送认证请求
        /// </summary>
        public Authenticate(Credentials credentials)
        {
            this.Address = new Uri(Requester.AuthURL + "/authenticate");
            this.Arguments.Add(credentials.Username);
            this.Arguments.Add(credentials.Password);
        }

        /// <summary>
        /// 执行认证.
        /// </summary>
        public async override Task<AuthenticateResponse> PerformRequestAsync()
        {
            try
            {
                this.PostContent = new JObject(
                                    new JProperty("agent",
                                        new JObject(
                                            new JProperty("name", "Minecraft"),
                                            new JProperty("version", "1"))),
                                    new JProperty("username", this.Arguments[0]),
                                    new JProperty("password", this.Arguments[1]),
                                    new JProperty("clientToken", Requester.ClientToken),
                                    new JProperty("requestUser", true)).ToString();

                this.Response = await Requester.Post(this);
                if (this.Response.IsSuccess)
                {
                    JObject user = JObject.Parse(this.Response.RawMessage);
                    List<Uuid> availableProfiles = new List<Uuid>();

                    foreach (JObject profile in user["availableProfiles"])
                    {
                        var playerName = profile["name"].ToObject<string>();
                        var value = profile["id"].ToObject<string>();
                        var legacy = (profile.ContainsKey("legacyProfile") ? profile["legacyProfile"].ToObject<bool>() : false);
                        availableProfiles.Add(new Uuid()
                        {
                            PlayerName = playerName,
                            Value = value,
                            Legacy = legacy,
                            Demo = null
                        });
                    }


                    return new AuthenticateResponse(this.Response)
                    {
                        AccessToken = user["accessToken"].ToObject<string>(),
                        ClientToken = user["clientToken"].ToObject<string>(),
                        AvailableProfiles = availableProfiles,
                        SelectedProfile = new Uuid()
                        {
                            PlayerName = user["selectedProfile"]["name"].ToObject<string>(),
                            Value = user["selectedProfile"]["id"].ToObject<string>(),
                            Legacy = (user["selectedProfile"].ToString().Contains("legacyProfile") ? user["selectedProfile"]["legacyProfile"].ToObject<bool>() : false),
                            Demo = null
                        },
                        User = user["user"].ToObject<UserData>()
                    };
                }
                else
                {
                    try
                    {
                        AuthenticationResponseError error = new AuthenticationResponseError(JObject.Parse(this.Response.RawMessage));
                        return new AuthenticateResponse(this.Response) { Error = error };
                    }
                    catch (Exception)
                    {
                        return new AuthenticateResponse(Error.GetError(this.Response));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    /// 刷新认证请求类
    /// </summary>
    public class Refresh : IEndpoint<TokenResponse>
    {

        /// <summary>
        /// 刷新访问令牌,必须与身份验证相同.
        /// </summary>
        public Refresh(string accessToken)
        {
            this.Address = new Uri(Requester.AuthURL + "/refresh");
            this.Arguments.Add(accessToken);
        }

        /// <summary>
        /// 执行刷新令牌.
        /// </summary>
        /// <returns></returns>
        public async override Task<TokenResponse> PerformRequestAsync()
        {
            this.PostContent = new JObject(
                                    new JProperty("accessToken", this.Arguments[0]),
                                    new JProperty("clientToken", Requester.ClientToken)).ToString();

            this.Response = await Requester.Post(this);

            if (this.Response.IsSuccess)
            {
                JObject refresh = JObject.Parse(this.Response.RawMessage);

                return new TokenResponse(this.Response)
                {
                    AccessToken = refresh["accessToken"].ToObject<string>()
                };
            }
            else
                return new TokenResponse(Error.GetError(this.Response));
        }
    }

    /// <summary>
    /// 验证令牌请求类
    /// </summary>
    public class Validate : IEndpoint<Response>
    {

        /// <summary>
        /// 刷新访问令牌,必须与身份验证相同
        /// </summary>
        public Validate(string accessToken)
        {
            this.Address = new Uri(Requester.AuthURL + "/validate");
            this.Arguments.Add(accessToken);
        }

        /// <summary>
        /// 执行验证令牌
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.PostContent = new JObject(
                                    new JProperty("accessToken", this.Arguments[0]),
                                    new JProperty("clientToken", Requester.ClientToken)).ToString();

            this.Response = await Requester.Post(this);

            if (this.Response.Code == HttpStatusCode.NoContent)
                return new Response(this.Response) { IsSuccess = true };
            else
                return new Response(Error.GetError(this.Response));
        }
    }

    /// <summary>
    /// 注销请求类
    /// </summary>
    public class Signout : IEndpoint<Response>
    {

        /// <summary>
        /// 刷新访问令牌,必须与身份验证相同
        /// </summary>
        public Signout(Credentials credentials)
        {
            this.Address = new Uri(Requester.AuthURL + "/signout");
            this.Arguments.Add(credentials.Username);
            this.Arguments.Add(credentials.Password);
        }

        /// <summary>
        /// 执行注销
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.PostContent = new JObject(
                                    new JProperty("username", this.Arguments[0]),
                                    new JProperty("password", this.Arguments[1])).ToString();

            this.Response = await Requester.Post(this);

            if (string.IsNullOrWhiteSpace(this.Response.RawMessage))
                return new Response(this.Response) { IsSuccess = true };
            else
                return new Response(Error.GetError(this.Response));
        }
    }

    /// <summary>
    /// 废止令牌请求类
    /// </summary>
    public class Invalidate : IEndpoint<Response>
    {

        /// <summary>
        /// 刷新访问令牌,必须与身份验证相同
        /// </summary>
        public Invalidate(string accessToken)
        {
            this.Address = new Uri(Requester.AuthURL + "/invalidate");
            this.Arguments.Add(accessToken);
        }

        /// <summary>
        /// 执行验证令牌
        /// </summary>
        public async override Task<Response> PerformRequestAsync()
        {
            this.PostContent = new JObject(
                                    new JProperty("accessToken", this.Arguments[0]),
                                    new JProperty("clientToken", Requester.ClientToken)).ToString();

            this.Response = await Requester.Post(this);

            if (this.Response.Code == HttpStatusCode.NoContent)
            {
                return new Response(this.Response) { IsSuccess = true }; ;
            }
            else
                return new Response(Error.GetError(this.Response));
        }
    }



}
