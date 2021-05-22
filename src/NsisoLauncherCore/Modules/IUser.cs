using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static NsisoLauncherCore.Modules.UserData;

namespace NsisoLauncherCore.Modules
{
    public interface IUser
    {
        /// <summary>
        /// 启动所使用的验证密匙
        /// </summary>
        [JsonIgnore]
        string LaunchAccessToken { get; }

        /// <summary>
        /// 用户Id
        /// </summary>
        [JsonIgnore]
        string UserId { get; }

        /// <summary>
        /// 启动所使用的uuid
        /// </summary>
        [JsonIgnore]
        string LaunchUuid { get; }

        /// <summary>
        /// 启动所使用的玩家名字
        /// </summary>
        [JsonIgnore]
        string LaunchPlayerName { get; }

        /// <summary>
        /// 用户类型
        /// </summary>
        [JsonIgnore]
        string UserType { get; }

        /// <summary>
        /// 用户属性
        /// </summary>
        [JsonIgnore]
        List<Property> Properties { get; }
    }
}
