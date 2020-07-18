using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     代表一对用于验证的用户名和密码
    /// </summary>
    public class Credentials
    {
        /// <summary>
        ///     用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     密码
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    ///     验证请求类
    /// </summary>
    public class Authenticate : IEndpoint<AuthenticateResponse>
    {
        /// <summary>
        ///     发送认证请求
        /// </summary>
        public Authenticate(Credentials credentials)
        {
            Address = new Uri(Requester.AuthURL + "/authenticate");
            Arguments.Add(credentials.Username);
            Arguments.Add(credentials.Password);
        }

        /// <summary>
        ///     执行认证.
        /// </summary>
        public override async Task<AuthenticateResponse> PerformRequestAsync()
        {
            try
            {
                PostContent = new JObject(
                    new JProperty("agent",
                        new JObject(
                            new JProperty("name", "Minecraft"),
                            new JProperty("version", "1"))),
                    new JProperty("username", Arguments[0]),
                    new JProperty("password", Arguments[1]),
                    new JProperty("clientToken", Requester.ClientToken),
                    new JProperty("requestUser", true)).ToString();

                Response = await Requester.Post(this);

                //only in debug
                //MessageBox.Show(Response.RawMessage);

                if (Response.IsSuccess)
                {
                    var user = JObject.Parse(Response.RawMessage);

                    var availableProfiles = new List<Uuid>();

                    #region 处理可用Profiles

                    foreach (JObject profile in user["availableProfiles"])
                    {
                        var playerName = profile["name"].ToObject<string>();
                        var value = profile["id"].ToObject<string>();
                        var legacy = profile.ContainsKey("legacyProfile")
                            ? profile["legacyProfile"].ToObject<bool>()
                            : false;
                        availableProfiles.Add(new Uuid
                        {
                            PlayerName = playerName,
                            Value = value,
                            Legacy = legacy,
                            Demo = null
                        });
                    }

                    #endregion

                    Uuid selectedProfile = null;

                    #region 处理选中Profrile

                    if (user["selectedProfile"] != null)
                        selectedProfile = new Uuid
                        {
                            PlayerName = user["selectedProfile"]["name"]?.ToObject<string>(),
                            Value = user["selectedProfile"]["id"]?.ToObject<string>(),
                            Legacy = user["selectedProfile"].ToString().Contains("legacyProfile")
                                ? user["selectedProfile"]["legacyProfile"].ToObject<bool>()
                                : false,
                            Demo = null
                        };

                    #endregion

                    var response = new AuthenticateResponse(Response)
                    {
                        AccessToken = user["accessToken"]?.ToObject<string>(),
                        ClientToken = user["clientToken"]?.ToObject<string>(),
                        AvailableProfiles = availableProfiles,
                        SelectedProfile = selectedProfile,
                        User = user["user"]?.ToObject<UserData>()
                    };

                    return response;
                }

                try
                {
                    var error = new AuthenticationResponseError(JObject.Parse(Response.RawMessage));
                    return new AuthenticateResponse(Response) {Error = error};
                }
                catch (Exception)
                {
                    return new AuthenticateResponse(Error.GetError(Response));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    ///     刷新认证请求类
    /// </summary>
    public class Refresh : IEndpoint<TokenResponse>
    {
        /// <summary>
        ///     刷新访问令牌,必须与身份验证相同.
        /// </summary>
        public Refresh(string accessToken)
        {
            Address = new Uri(Requester.AuthURL + "/refresh");
            Arguments.Add(accessToken);
        }

        /// <summary>
        ///     执行刷新令牌.
        /// </summary>
        /// <returns></returns>
        public override async Task<TokenResponse> PerformRequestAsync()
        {
            PostContent = new JObject(
                new JProperty("accessToken", Arguments[0]),
                new JProperty("clientToken", Requester.ClientToken)).ToString();

            Response = await Requester.Post(this);

            if (Response.IsSuccess)
            {
                var refresh = JObject.Parse(Response.RawMessage);

                return new TokenResponse(Response)
                {
                    AccessToken = refresh["accessToken"].ToObject<string>()
                };
            }

            return new TokenResponse(Error.GetError(Response));
        }
    }

    /// <summary>
    ///     验证令牌请求类
    /// </summary>
    public class Validate : IEndpoint<Response>
    {
        /// <summary>
        ///     刷新访问令牌,必须与身份验证相同
        /// </summary>
        public Validate(string accessToken)
        {
            Address = new Uri(Requester.AuthURL + "/validate");
            Arguments.Add(accessToken);
        }

        /// <summary>
        ///     执行验证令牌
        /// </summary>
        public override async Task<Response> PerformRequestAsync()
        {
            PostContent = new JObject(
                new JProperty("accessToken", Arguments[0]),
                new JProperty("clientToken", Requester.ClientToken)).ToString();

            Response = await Requester.Post(this);

            if (Response.Code == HttpStatusCode.NoContent)
                return new Response(Response) {IsSuccess = true};
            return new Response(Error.GetError(Response));
        }
    }

    /// <summary>
    ///     注销请求类
    /// </summary>
    public class Signout : IEndpoint<Response>
    {
        /// <summary>
        ///     刷新访问令牌,必须与身份验证相同
        /// </summary>
        public Signout(Credentials credentials)
        {
            Address = new Uri(Requester.AuthURL + "/signout");
            Arguments.Add(credentials.Username);
            Arguments.Add(credentials.Password);
        }

        /// <summary>
        ///     执行注销
        /// </summary>
        public override async Task<Response> PerformRequestAsync()
        {
            PostContent = new JObject(
                new JProperty("username", Arguments[0]),
                new JProperty("password", Arguments[1])).ToString();

            Response = await Requester.Post(this);

            if (string.IsNullOrWhiteSpace(Response.RawMessage))
                return new Response(Response) {IsSuccess = true};
            return new Response(Error.GetError(Response));
        }
    }

    /// <summary>
    ///     废止令牌请求类
    /// </summary>
    public class Invalidate : IEndpoint<Response>
    {
        /// <summary>
        ///     刷新访问令牌,必须与身份验证相同
        /// </summary>
        public Invalidate(string accessToken)
        {
            Address = new Uri(Requester.AuthURL + "/invalidate");
            Arguments.Add(accessToken);
        }

        /// <summary>
        ///     执行验证令牌
        /// </summary>
        public override async Task<Response> PerformRequestAsync()
        {
            PostContent = new JObject(
                new JProperty("accessToken", Arguments[0]),
                new JProperty("clientToken", Requester.ClientToken)).ToString();

            Response = await Requester.Post(this);

            if (Response.Code == HttpStatusCode.NoContent)
            {
                return new Response(Response) {IsSuccess = true};
                ;
            }

            return new Response(Error.GetError(Response));
        }
    }
}