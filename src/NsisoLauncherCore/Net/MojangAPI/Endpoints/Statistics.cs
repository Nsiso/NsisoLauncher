using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using NsisoLauncherCore.Net.MojangApi.Responses;

namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{
    /// <summary>
    ///     统计请求类
    /// </summary>
    public class Statistics : IEndpoint<StatisticsResponse>
    {
        /// <summary>
        ///     可用统计条目
        /// </summary>
        public enum Item
        {
            /// <summary>
            ///     已售出的Minecraft帐户金额
            /// </summary>
            MinecraftAccountsSold,

            /// <summary>
            ///     已兑换的Minecraft预付卡数额
            /// </summary>
            MinecraftPrepaidCardsRedeemed,

            /// <summary>
            ///     销售的Cobalt账户数量
            /// </summary>
            CobaltAccountsSold,

            /// <summary>
            ///     销售的Scrolls账户数量
            /// </summary>
            ScrollsAccountsSold
        }

        /// <summary>
        ///     可用统计项目的列表
        /// </summary>
        public static Dictionary<Item, string> StatisticItems = new Dictionary<Item, string>
        {
            {Item.MinecraftAccountsSold, "item_sold_minecraft"},
            {Item.MinecraftPrepaidCardsRedeemed, "prepaid_card_redeemed_minecraft"},
            {Item.CobaltAccountsSold, "item_sold_cobalt"},
            {Item.ScrollsAccountsSold, "item_sold_scrolls"}
        };

        /// <summary>
        ///     询问Mojang有关该项目的统计数据
        /// </summary>
        public Statistics(List<Item> items) : this(items.ToArray())
        {
        }

        /// <summary>
        ///     询问Mojang有关该项目的统计数据
        /// </summary>
        public Statistics(params Item[] items)
        {
            Address = new Uri("https://api.mojang.com/orders/statistics");
            foreach (var item in items)
                Arguments.Add(StatisticItems[item]);
        }

        /// <summary>
        ///     执行统计请求
        /// </summary>
        /// <returns></returns>
        public override async Task<StatisticsResponse> PerformRequestAsync()
        {
            PostContent = "{ \"metricKeys\": [" +
                          string.Join(",", Arguments.ConvertAll(x => $"\"{x.ToString()}\"").ToArray()) + "]}";
            Response = await Requester.Post(this);

            if (Response.IsSuccess)
            {
                var stats = JObject.Parse(Response.RawMessage);

                return new StatisticsResponse(Response)
                {
                    Total = stats["total"].ToObject<int>(),
                    Last24h = stats["last24h"].ToObject<int>(),
                    SaleVelocity = stats["saleVelocityPerSeconds"].ToObject<double>()
                };
            }

            if (Response.Code == HttpStatusCode.BadRequest)
                return new StatisticsResponse(new Response(Response)
                {
                    Error =
                    {
                        ErrorMessage = "One of the usernames is empty.",
                        ErrorTag = "IllegalArgumentException"
                    }
                });
            return new StatisticsResponse(Error.GetError(Response));
        }
    }
}