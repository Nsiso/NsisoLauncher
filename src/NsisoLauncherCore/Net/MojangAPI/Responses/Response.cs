using System;
using System.Net;

namespace NsisoLauncherCore.Net.MojangApi
{

    /// <summary>
    /// 默认的响应类,可以继承
    /// </summary>
    public class Response
    {

        /// <summary>
        /// 响应的状态代码
        /// </summary>
        public HttpStatusCode Code { get; internal set; }

        /// <summary>
        ///定义请求是否成功
        /// </summary>
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// 响应的原始消息内容
        /// </summary>
        public string RawMessage { get; internal set; }

        /// <summary>
        /// 错误(如果请求失败)
        /// </summary>
        public Error Error { get; internal set; }

        internal Response() { }
        internal Response(Response response) : this()
        {
            this.Code = response.Code;
            this.IsSuccess = response.IsSuccess;
            this.RawMessage = response.RawMessage;
            this.Error = response.Error;
        }
    }

    /// <summary>
    /// 默认错误类
    /// </summary>
    public class Error
    {
        /// <summary>
        /// 给定错误的标记
        /// </summary>
        public string ErrorTag { get; internal set; }

        /// <summary>
        /// 错误的详细信息
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// 异常（如发生代码错误）
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// 获取一个错误回复
        /// </summary>
        public static Response GetError(Response response)
        {
            // This has to be fill
            switch (response.Code)
            {
                case HttpStatusCode.NoContent:
                    {
                        return new Response(response)
                        {
                            IsSuccess = false,
                            Error = new Error()
                            {
                                ErrorMessage = "Response has no content",
                                ErrorTag = "NoContent"
                            }
                        };
                    }

                case HttpStatusCode.UnsupportedMediaType:
                    {
                        return new Response(response)
                        {
                            IsSuccess = false,
                            Error = new Error()
                            {
                                ErrorMessage = "Post contents must not be well formatted",
                                ErrorTag = "UnsupportedMediaType"
                            }
                        };
                    }

                default:
                    {
                        return new Response(response)
                        {
                            IsSuccess = false,
                            Error = new Error()
                            {
                                ErrorMessage = response.Code.ToString(),
                                ErrorTag = response.Code.ToString()
                            }
                        };
                    }
            }
        }
    }
}
