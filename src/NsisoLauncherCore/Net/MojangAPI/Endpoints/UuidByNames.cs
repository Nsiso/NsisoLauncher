using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    /// UuidByName请求类
    /// </summary>
    public class UuidByNames : IEndpoint<UuidByNamesResponse>
    {
        /// <summary>
        /// 获取与给定用户名相对应的Uuid列表
        /// </summary>
        /// <param name="usernames"></param>
        public UuidByNames(List<string> usernames) : this(usernames.ToArray())
        {
        }

        /// <summary>
        /// 获取与给定用户名相对应的Uuid列表
        /// </summary>
        /// <param name="usernames"></param>
        public UuidByNames(params string[] usernames)
        {
            if (usernames.Length > 100)
                throw new ArgumentException("Only up to 100 usernames per request are allowed.");

            this.Address = new Uri($"https://api.mojang.com/profiles/minecraft");
            this.Arguments = usernames.ToList<string>();
        }

        /// <summary>
        /// 执行UuidByNames请求
        /// </summary>
        /// <returns></returns>
        public async override Task<UuidByNamesResponse> PerformRequestAsync()
        {
            this.PostContent = "[" + string.Join(",", this.Arguments.ConvertAll(x => $"\"{x.ToString()}\"").ToArray()) + "]";
            this.Response = await Requester.Post(this);

            if (this.Response.IsSuccess)
            {
                JArray uuids = JArray.Parse(this.Response.RawMessage);
                List<Uuid> uuidList = new List<Uuid>() { };

                foreach (JObject uuid in uuids)
                    uuidList.Add(uuid.ToObject<Uuid>());

                return new UuidByNamesResponse(this.Response)
                {
                    UuidList = uuidList,
                };
            }
            else
            {
                if (this.Response.Code == HttpStatusCode.BadRequest)
                {
                    return new UuidByNamesResponse(new Response(this.Response)
                    {
                        Error =
                        {
                            ErrorMessage = "One of the usernames is empty.",
                            ErrorTag = "IllegalArgumentException"
                        }
                    });
                }
                else
                    return new UuidByNamesResponse(Error.GetError(this.Response));
            }
        }
    }

}
