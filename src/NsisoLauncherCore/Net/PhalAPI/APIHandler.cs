using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Net.PhalAPI
{
    public class APIHandler
    {
        private const string APIUrl = "http://hn2.api.okayapi.com/";
        private const string App_key = "7B27B7B6A3C10158C28E3DE0B13785CD";

        private readonly NetRequester _netRequester;

        public APIHandler(bool isNoTracking, NetRequester requester)
        {
            NoTracking = isNoTracking;
            _netRequester = requester ?? throw new ArgumentNullException("NetRequester is null");
        }

        public bool NoTracking { get; set; }

        public async Task<NsisoLauncherVersionResponse> GetLatestLauncherVersion()
        {
            try
            {
                var args = new Dictionary<string, string>();
                args.Add("app_key", App_key);
                //表模型
                args.Add("model_name", "VersionList");
                //order
                args.Add("order", "[\"id DESC\"]");
                //查询规则（ID>0）
                args.Add("where", "[[\"id\", \">\", \"0\"]]");
                //仅返回一条（即ID最高的最新版本）
                args.Add("perpage", "1");
                var resultRespond = await _netRequester.HttpPostAsync(APIUrl + "?s=App.Table.FreeQuery", args);
                if (!resultRespond.IsSuccessStatusCode) return null;
                var result = await resultRespond.Content.ReadAsStringAsync();
                var desObj = JsonConvert.DeserializeObject<PhalApiClientResponse>(result);
                JObject listJobj = desObj.Data;
                var list = listJobj.ToObject<NsisoLauncherVersionListResponse>();
                return list.List.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     异步报告日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="log">日志内容</param>
        /// <returns></returns>
        public async Task PostLogAsync(LogLevel level, string log)
        {
            var escapeLog = Uri.EscapeDataString(log);
            var args = new Dictionary<string, string>();
            args.Add("app_key", App_key);
            args.Add("super_type", level.ToString());
            args.Add("super_message", log);
            var result = await _netRequester.HttpPostAsync(APIUrl + "?s=App.Market_SuperLogger.Record", args);
            Console.WriteLine(result);
        }

        /// <summary>
        ///     刷新使用次数计数器（统计）
        /// </summary>
        /// <returns></returns>
        public async Task RefreshUsingTimesCounter()
        {
            if (!NoTracking)
                try
                {
                    var args = new Dictionary<string, string>();
                    args.Add("app_key", App_key);
                    args.Add("type", "forever");
                    args.Add("name", "NsisoLauncherUsingTimes");
                    args.Add("value", "1");
                    await _netRequester.HttpPostAsync(APIUrl + "?s=App.Main_Counter.SmartRefresh", args);
                }
                catch
                {
                }
        }
    }
}