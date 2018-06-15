using NsisoLauncher.Core.Net.MojangApi.Api;
using NsisoLauncher.Core.Net.MojangApi.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NsisoLauncher.Core.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncher.Config
{
    public class MainConfig
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// 历史数据
        /// </summary>
        public History History { get; set; }
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

        /// <summary>
        /// 玩家选择的角色UUID
        /// </summary>
        public Uuid AuthenticationUUID { get; set; }

        /// <summary>
        /// 验证的用户信息
        /// </summary>
        public UserData AuthenticationUserData { get; set; }
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

    public enum GameDirEnum
    {
        /// <summary>
        /// 启动器根目录
        /// </summary>
        ROOT = 0,

        /// <summary>
        /// 系统AppData
        /// </summary>
        APPDATA = 1,

        /// <summary>
        /// 系统程序文件夹
        /// </summary>
        PROGRAMFILES = 2,

        /// <summary>
        /// 自定义
        /// </summary>
        CUSTOM = 3
    }

    /// <summary>
    /// 历史记录类
    /// </summary>
    public class History
    {
        /// <summary>
        /// 上一次启动版本
        /// </summary>
        public string LastLaunchVersion { get; set; }

        /// <summary>
        /// 上次启动时间
        /// </summary>
        public DateTime LastLaunchTime { get; set; }

        /// <summary>
        /// 上次启动使用的时间(Ms)
        /// </summary>
        public long LastLaunchUsingMs { get; set; }
    }
}
