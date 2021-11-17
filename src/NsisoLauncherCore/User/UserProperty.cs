using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.User
{
    public class UserProperty
    {
        /// <summary>
        /// Property name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Property value
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
