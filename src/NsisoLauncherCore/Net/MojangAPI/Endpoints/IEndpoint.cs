using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangApi
{
    /// <summary>
    /// Endpoint父类
    /// </summary>
    /// <typeparam name="T">该endpoint的响应类型.</typeparam>
    public abstract class IEndpoint<T>
    {
        /// <summary>
        /// Endpoint的地址
        /// </summary>
        public Uri Address { get; set; }

        /// <summary>
        /// 执行请求后的响应.
        /// </summary>
        public Response Response { get; set; }

        /// <summary>
        /// 要发送的参数.
        /// </summary>
        public List<string> Arguments
        {
            get
            {
                if (_arguments == null)
                    _arguments = new List<string>() { };
                return _arguments;
            }
            set { _arguments = value; }
        }
        private List<string> _arguments;

        /// <summary>
        /// 发布请求的内容，必须在执行之前进行设置.
        /// </summary>
        public string PostContent { get; set; }

        /// <summary>
        /// 执行请求
        /// </summary>
        /// <returns></returns>
        public abstract Task<T> PerformRequestAsync();
    }
}
