using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public enum AuthenticationType
    {
        /// <summary>
        /// 离线验证
        /// </summary>
        OFFLINE,

        /// <summary> 
        /// 官方正版验证
        /// </summary>
        MOJANG,

        /// <summary>
        /// 统一验证
        /// </summary>
        NIDE8,

        /// <summary>
        /// authlib-injector验证
        /// </summary>
        AUTHLIB_INJECTOR,

        /// <summary>
        /// 自定义服务器验证
        /// </summary>
        CUSTOM_SERVER
    }
}
