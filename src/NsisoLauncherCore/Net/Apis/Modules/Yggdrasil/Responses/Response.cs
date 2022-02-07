using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses
{
    /// <summary>
    /// The response class of yggdrasil
    /// </summary>
    public class Response
    {

        /// <summary>
        /// Http code
        /// </summary>
        public HttpStatusCode Code { get; internal set; }

        /// <summary>
        /// Is success
        /// </summary>
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// The raw http response body message.
        /// </summary>
        public string RawMessage { get; internal set; }

        /// <summary>
        /// The error response if failed
        /// </summary>
        public Error Error { get; internal set; }

        internal Response()
        {

        }

        internal Response(Response response) : this()
        {
            this.IsSuccess = response.IsSuccess;
            this.Code = response.Code;
            this.RawMessage = response.RawMessage;
            this.Error = response.Error;
        }
    }

    /// <summary>
    /// The error class when not return http 2xx code
    /// </summary>
    public class Error
    {
        /// <summary>
        /// The tag of error
        /// </summary>
        [JsonProperty("error")]
        public string ErrorTag { get; internal set; }

        /// <summary>
        /// The message of error
        /// </summary>
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// The cause of the error
        /// </summary>
        [JsonProperty("cause")]
        public string ErrorCause { get; internal set; }
    }
}
