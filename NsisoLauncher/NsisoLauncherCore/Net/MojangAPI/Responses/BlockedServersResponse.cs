using System.Collections.Generic;
using System.Linq;

namespace NsisoLauncherCore.Net.MojangApi.Responses
{
    /// <summary>
    /// 包含所有被阻止服务器的响应
    /// </summary>
    public class BlockedServersResponse : Response
    {
        internal BlockedServersResponse(Response response) : base(response) { }

        /// <summary>
        /// 当前被阻止的服务器的列表
        /// </summary>
        public List<BlockedServer> BlockedServers { get; internal set; }

        /// <summary>
        /// 阻止服务器响应内容
        /// </summary>
        public class BlockedServer
        {

            /// <summary>
            /// 服务器地址SHA1
            /// </summary>
            public string AddressSHA1 { get; internal set; }

            /// <summary>
            /// 如果SHA1地址已被破解，则为真（请参阅地址）
            /// </summary>
            public bool Cracked { get { return Address != null; } }

            /// <summary>
            /// 地址（如果服务器已被破解）
            /// </summary>
            public string Address { get; internal set; }

            /// <summary>
            /// 获取禁止的服务器列表（包含破解的列表）
            /// </summary>
            public static List<BlockedServer> Parse(string addresses)
            {
                List<BlockedServer> blockServers = new List<BlockedServer>();
                foreach (string address in addresses.Split('\n'))
                {
                    if (KnownCrackedServers.Any(x => x.AddressSHA1 == address))
                        blockServers.Add(KnownCrackedServers.Find(x => x.AddressSHA1 == address));
                    else
                        blockServers.Add(new BlockedServer() { AddressSHA1 = address });
                }
                return blockServers;
            }

            /// <summary>
            /// 已知的破解服务器列表
            /// </summary>
            public static List<BlockedServer> KnownCrackedServers = new List<BlockedServer>()
            {
                new BlockedServer() { Address = "*.minetime.com", AddressSHA1 = "6f2520f8bd70a718c568ab5274c56bdbbfc14ef4"},
                new BlockedServer() { Address = "*.trollingbrandon.club", AddressSHA1 = "7ea72de5f8e70a2ac45f1aa17d43f0ca3cddeedd"},
                new BlockedServer() { Address = "*.skygod.us", AddressSHA1 = "c005ad34245a8f2105658da2d6d6e8545ef0f0de"},
                new BlockedServer() { Address = "*.mineaqua.es", AddressSHA1 = "c645d6c6430db3069abd291ec13afebdb320714b"},
                new BlockedServer() { Address = "*.eulablows.host", AddressSHA1 = "8bf58811e6ebca16a01b842ff0c012db1171d7d6"},
                new BlockedServer() { Address = "*.moredotsmoredots.xyz", AddressSHA1 = "8789800277882d1989d384e7941b6ad3dadab430"},
                new BlockedServer() { Address = "*.brandonlovescock.bid", AddressSHA1 = "e40c3456fb05687b8eeb17213a47b263d566f179"},
                new BlockedServer() { Address = "*.brandonlovescock.club", AddressSHA1 = "278b24ffff7f9f46cf71212a4c0948d07fb3bc35"},
                new BlockedServer() { Address = "*.mineaqua.net", AddressSHA1 = "c78697e385bfa58d6bd2a013f543cdfbdc297c4f"},
                new BlockedServer() { Address = "*.endercraft.com", AddressSHA1 = "b13009db1e2fbe05465716f67c8d58b9c0503520"},
                new BlockedServer() { Address = "*.insanefactions.org", AddressSHA1 = "3e560742576af9413fca72e70f75d7ddc9416020"},
                new BlockedServer() { Address = "*.playmc.mx", AddressSHA1 = "986204c70d368d50ffead9031e86f2b9e70bb6d0"},
                new BlockedServer() { Address = "*.howdoiblacklistsrv.host", AddressSHA1 = "65ca8860fa8141da805106c0389de9d7c17e39bf"},
                new BlockedServer() { Address = "198.27.77.72", AddressSHA1 = "dcc1f876e258ac5ecab28244da7a94ed44d4b43f"},
                new BlockedServer() { Address = "*.brandonlovescock.online", AddressSHA1 = "7dca807cc9484b1eed109c003831faf189b6c8bf"},
                new BlockedServer() { Address = "*.brandonlovescock.press", AddressSHA1 = "c6a2203285fb0a475c1cd6ff72527209cc0ccc6e"},
                new BlockedServer() { Address = "*.insanenetwork.org", AddressSHA1 = "e3985eb936d66c9b07aa72c15358f92965b1194e"},
                new BlockedServer() { Address = "*.phoenixnexus.net", AddressSHA1 = "b140bec2347bfbe6dcae44aa876b9ba5fe66505b"},
                new BlockedServer() { Address = "*.arkhamnetwork.org", AddressSHA1 = "27ae74becc8cd701b19f25d347faa71084f69acd"},
                new BlockedServer() { Address = "brandonisan.unusualperson.com", AddressSHA1 = "48f04e89d20b15de115503f22fedfe2cb2d1ab12"},
                new BlockedServer() { Address = "*.kidslovemy500dollarranks.club", AddressSHA1 = "9f0f30820cebb01f6c81f0fdafefa0142660d688"},
                new BlockedServer() { Address = "*.eccgamers.com", AddressSHA1 = "cc90e7b39112a48064f430d3a08bbd78a226d670"},
                new BlockedServer() { Address = "*.fucktheeula.com", AddressSHA1 = "88f155cf583c930ffed0e3e69ebc3a186ea8cbb7"},
                new BlockedServer() { Address = "*.mojangendorsesbrazzers.webcam", AddressSHA1 = "605e6296b8dba9f0e4b8e43269fe5d053b5f4f1b"},
                new BlockedServer() { Address = "touchmybody.redirectme.net", AddressSHA1 = "5d2e23d164a43fbfc4e6093074567f39b504ab51"},
                new BlockedServer() { Address = "*.mojangsentamonkeyinto.space", AddressSHA1 = "f3df314d1f816a8c2185cd7d4bcd73bbcffc4ed8"},
                new BlockedServer() { Address = "*.diacraft.org", AddressSHA1 = "073ca448ef3d311218d7bd32d6307243ce22e7d0"},
                new BlockedServer() { Address = "*.diacraft.de", AddressSHA1 = "33839f4006d6044a3a6675c593fada6a690bb64d"},
                new BlockedServer() { Address = "*.eulablacklist.club", AddressSHA1 = "e2e12f3b7b85eab81c0ee5d2e9e188df583fe281" },
                new BlockedServer() { Address = "*.slaughterhousepvp.com", AddressSHA1 = "11a2c115510bfa6cb56bbd18a7259a4420498fd5"},
                new BlockedServer() { Address = "*.timelesspvp.net", AddressSHA1 = "75df09492c6c979e2db41116100093bb791b8433"},
                new BlockedServer() { Address = "*.herowars.org", AddressSHA1 = "d42339c120bc10a393a0b1d2c6a2e0ed4dbdd61b"},
                new BlockedServer() { Address = "justgiveinandblockddnsbitches.ddns.net", AddressSHA1 = "4a1b3b860ba0b441fa722bbcba97a614f6af9bb8" },
                new BlockedServer() { Address = "brandonisagainst.health-carereform.com", AddressSHA1 = "b8c876f599dcf5162911bba2d543ccbd23d18ae5" },
                new BlockedServer() { Address = "brandonwatchesporn.onthewifi.com", AddressSHA1 = "9a9ae8e9d0b6f3bf54c266dcd1e4ec034e13f714"},
                new BlockedServer() { Address = "canyouwildcardipsplease.gotdns.ch", AddressSHA1 = "336e718ffbc705e76b4a72884172c6b95216b57c"},
                new BlockedServer() { Address = "letsplaysome.servecounterstrike.com", AddressSHA1 = "27cf97ecf24c92f1fe5c84c5ff654728c3ee37dd"},
                new BlockedServer() { Address = "mojangbrokeintomy.homesecuritymac.com", AddressSHA1 = "32066aa0c7dc9b097eed5b00c5629ad03f250a2d"},
            };
        }
    }
}
