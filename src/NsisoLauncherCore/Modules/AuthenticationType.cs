using System;

namespace NsisoLauncherCore.Modules
{
    [Flags]
    public enum AuthenticationType
    {
        /// <summary>
        /// 离线验证
        /// </summary>
        OFFLINE,

        /// <summary> 
        /// MOJANG登录
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
        CUSTOM_SERVER,

        /// <summary>
        /// 微软登录
        /// </summary>
        MICROSOFT,
    }
}
