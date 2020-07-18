using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;
using static NsisoLauncherCore.Net.MojangApi.Responses.ChallengesResponse;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     验证问题请求类
    /// </summary>
    public class Challenges : IEndpoint<ChallengesResponse>
    {
        /// <summary>
        ///     实例化允许用户所要回答的验证问题的endpoint。
        /// </summary>
        /// <param name="token">有效的用户令牌</param>
        public Challenges(string accessToken)
        {
            Address = new Uri("https://api.mojang.com/user/security/challenges");
            Arguments.Add(accessToken);
        }

        /// <summary>
        ///     执行请求并返回响应属性
        /// </summary>
        public override async Task<ChallengesResponse> PerformRequestAsync()
        {
            Response = await Requester.Get(this, true);

            if (Response.IsSuccess)
            {
                var jchallenges = JArray.Parse(Response.RawMessage);
                var challenges = new List<Challenge>();
                foreach (var token in jchallenges)
                    challenges.Add(Parse(token));

                return new ChallengesResponse(Response)
                {
                    Challenges = challenges
                };
            }

            return new ChallengesResponse(Error.GetError(Response));
        }
    }
}