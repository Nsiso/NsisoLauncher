using NsisoLauncherCore.Net.MojangApi.Responses;
using System;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.MojangApi.Responses.BlockedServersResponse;


namespace NsisoLauncherCore.Net.MojangApi.Endpoints
{

    /// <summary>
    /// 获取用于检查被阻止的服务器地址的SHA1哈希列表
    /// 客户端使用ISO-8859-1字符集对这个列表检查小写字母的名称。
    /// 他们也会尝试检查子域，用*替换每个级别。
    /// 具体来说，它分裂的基础上。 在域中，一次去除每个部分。
    /// 例如，对于mc.example.com，它会尝试mc.example.com，* .example.com和* .com。 使用IP地址
    /// （通过具有4个分割部分来验证，每个部分是0到255之间的有效整数，包括0和255）
    /// 替换从结尾开始，因此对于192.168.0.1，它将尝试192.168.0.1,192.168.0。*，192.168。*和192。*。
    /// 这个检查是通过netty中的bootstrap类完成的。默认的netty类在com.mojang：netty依赖项中被一个覆盖
    /// 由启动器加载。 这可以影响任何使用netty（1.7+）的版本
    /// </summary>
    public class BlockedServers : IEndpoint<BlockedServersResponse>
    {

        /// <summary>
        /// BlockServer请求类的实例
        /// </summary>
        public BlockedServers()
        {
            this.Address = new Uri($"https://sessionserver.mojang.com/blockedservers");
        }

        /// <summary>
        /// 执行BlockServer服务器请求
        /// </summary>
        /// <returns></returns>
        public async override Task<BlockedServersResponse> PerformRequestAsync()
        {
            this.Response = await Api.Requester.Get(this);

            if (this.Response.IsSuccess)
            {
                return new BlockedServersResponse(this.Response)
                {
                    BlockedServers = BlockedServer.Parse(this.Response.RawMessage)
                };
            }
            else
                return new BlockedServersResponse(Error.GetError(this.Response));
        }
    }

}
