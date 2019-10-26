namespace NsisoLauncherCore.Net.MojangApi.Responses
{
    /// <summary>
    /// 表示一个统计请求响应
    /// </summary>
    public class StatisticsResponse : Response
    {
        internal StatisticsResponse(Response response) : base(response) { }

        /// <summary>
        /// 销售总额
        /// </summary>
        public int Total { get; internal set; }

        /// <summary>
        /// 在过去24小时内售出总额
        /// </summary>
        public int Last24h { get; internal set; }

        /// <summary>
        /// 平均销售量（秒）
        /// </summary>
        public double SaleVelocity { get; internal set; }
    }
}
