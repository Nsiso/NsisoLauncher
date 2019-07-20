using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Net.MojangApi.Api;
using System;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{

    /// <summary>
    /// Profile响应类
    /// </summary>
    public class ProfileResponse : Response
    {
        internal ProfileResponse(Response response) : base(response) { }

        /// <summary>
        /// 玩家UUID.
        /// </summary>
        public Uuid Uuid { get; internal set; }

        /// <summary>
        /// 玩家的profile属性
        /// </summary>
        public ProfileProperties Properties { get; internal set; }

        /// <summary>
        /// 代表profile所有属性的类
        /// </summary>
        public class ProfileProperties
        {

            /// <summary>
            /// 使用base64字符串实例化属性
            /// </summary>
            public ProfileProperties(string base64)
            {
                if (base64 == null)
                    throw new ArgumentNullException("Base64", "Base 64 string must not be null");

                string json = Requester.Encoding.GetString(Convert.FromBase64String(base64));
                JObject data = JObject.Parse(json);

                this.Date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(data["timestamp"].ToObject<double>());
                this.ProfileUuid = data["profileId"].ToObject<string>();
                this.ProfileName = data["profileName"].ToObject<string>();
                this.SignatureRequired = (!json.Contains("signatureRequired") ? false :
                                          data["signatureRequired"].ToObject<bool>());

                JObject textures = data["textures"].ToObject<JObject>();

                if (textures["SKIN"] != null && textures["SKIN"]["url"] != null)
                    this.SkinUri = new Uri(textures["SKIN"]["url"].ToObject<string>());
                if (textures["CAPE"] != null && textures["CAPE"]["url"] != null)
                    this.CapeUri = new Uri(textures["CAPE"]["url"].ToObject<string>());
            }

            /// <summary>
            /// 时间戳值
            /// </summary>
            public DateTime Date { get; internal set; }

            /// <summary>
            /// Profile UUID
            /// </summary>
            public string ProfileUuid { get; internal set; }

            /// <summary>
            /// Profile 名称
            /// </summary>
            public string ProfileName { get; internal set; }

            /// <summary>
            /// 不知道这是关于什么
            /// </summary>
            public bool SignatureRequired { get; internal set; }

            // The two elements below are part of an array.
            // If the API change, there likely be no trouble
            // with the parsing *but* future values will have
            // to be added manually

            //下面的两个元素是数组的一部分
            //如果API更改，则可能没有问题
            //解析*但是*未来的值将会有
            //手动添加

            /// <summary>
            /// 皮肤URI.如果用户没有皮肤可能无法工作
            /// </summary>
            public Uri SkinUri { get; internal set; }

            /// <summary>
            /// Cape URI
            /// </summary>
            public Uri CapeUri { get; internal set; }

        }
    }

    /// <summary>
    /// 表示访问配置文件API时发生错误
    /// </summary>
    public class ProfileResponseError : Error
    {
        internal ProfileResponseError(JObject json)
        {
            this.ErrorTag = json["error"].ToObject<string>();
            this.ErrorMessage = json["errorMessage"].ToObject<string>();
        }
    }
}
