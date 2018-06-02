using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NsisoLauncher.Config
{
    public class MainConfig
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public User User { get; set; }
    }

    public class User
    {
        /// <summary>
        /// 用户名/账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 验证令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 用户端Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// 验证服务器
        /// </summary>
        public string AuthServer { get; set; }

        /// <summary>
        /// 验证类型
        /// </summary>
        public AuthenticationType AuthenticationType { get; set; }
    }

    public enum AuthenticationType
    {
        /// <summary>
        /// 离线验证
        /// </summary>
        OFFLINE,

        /// <summary>
        /// 在线验证
        /// </summary>
        ONLINE
    }
}
