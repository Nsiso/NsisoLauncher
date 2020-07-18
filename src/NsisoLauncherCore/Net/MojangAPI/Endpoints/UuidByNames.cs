using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     UuidByName请求类
    /// </summary>
    public class UuidByNames : IEndpoint<UuidByNamesResponse>
    {
        /// <summary>
        ///     获取与给定用户名相对应的Uuid列表
        /// </summary>
        /// <param name="usernames"></param>
        public UuidByNames(List<string> usernames) : this(usernames.ToArray())
        {
        }

        /// <summary>
        ///     获取与给定用户名相对应的Uuid列表
        /// </summary>
        /// <param name="usernames"></param>
        public UuidByNames(params string[] usernames)
        {
            if (usernames.Length > 100)
                throw new ArgumentException("Only up to 100 usernames per request are allowed.");

            Address = new Uri("https://api.mojang.com/profiles/minecraft");
            Arguments = usernames.ToList();
        }

        /// <summary>
        ///     执行UuidByNames请求
        /// </summary>
        /// <returns></returns>
        public override async Task<UuidByNamesResponse> PerformRequestAsync()
        {
            PostContent = "[" + string.Join(",", Arguments.ConvertAll(x => $"\"{x.ToString()}\"").ToArray()) + "]";
            Response = await Requester.Post(this);

            if (Response.IsSuccess)
            {
                var uuids = JArray.Parse(Response.RawMessage);
                var uuidList = new List<Uuid>();

                foreach (JObject uuid in uuids)
                    uuidList.Add(uuid.ToObject<Uuid>());

                return new UuidByNamesResponse(Response)
                {
                    UuidList = uuidList
                };
            }

            if (Response.Code == HttpStatusCode.BadRequest)
                return new UuidByNamesResponse(new Response(Response)
                {
                    Error =
                    {
                        ErrorMessage = "One of the usernames is empty.",
                        ErrorTag = "IllegalArgumentException"
                    }
                });
            return new UuidByNamesResponse(Error.GetError(Response));
        }
    }
}