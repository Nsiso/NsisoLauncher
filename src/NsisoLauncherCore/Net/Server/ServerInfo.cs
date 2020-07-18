using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using Newtonsoft.Json.Linq;

namespace NsisoLauncherCore.Net.Server
{
    public class ServerInfo
    {
        public enum StateType
        {
            GOOD,
            NO_RESPONSE,
            BAD_CONNECT,
            EXCEPTION
        }

        public ServerInfo(string ip, ushort port)
        {
            ServerAddress = ip;
            ServerPort = port;
        }

        public ServerInfo(Modules.Server info)
        {
            ServerAddress = info.Address;
            ServerPort = info.Port;
        }

        /// <summary>
        ///     服务器IP地址
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     服务器端口
        /// </summary>
        public ushort ServerPort { get; set; }

        /// <summary>
        ///     服务器名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        ///     获取服务器MOTD
        /// </summary>
        public string MOTD { get; private set; }

        /// <summary>
        ///     获取服务器的最大玩家数量
        /// </summary>
        public int MaxPlayerCount { get; private set; }

        /// <summary>
        ///     获取服务器的在线人数
        /// </summary>
        public int CurrentPlayerCount { get; private set; }

        /// <summary>
        ///     获取服务器版本号
        /// </summary>
        public int ProtocolVersion { get; private set; }

        /// <summary>
        ///     获取服务器游戏版本
        /// </summary>
        public string GameVersion { get; private set; }

        /// <summary>
        ///     获取服务器详细的服务器信息JsonResult
        /// </summary>
        public string JsonResult { get; private set; }

        /// <summary>
        ///     获取服务器Forge信息（如果可用）
        /// </summary>
        public ForgeInfo ForgeInfo { get; private set; }

        /// <summary>
        ///     获取服务器在线玩家的名称（如果可用）
        /// </summary>
        public List<string> OnlinePlayersName { get; private set; }

        /// <summary>
        ///     获取此次连接服务器的延迟(ms)
        /// </summary>
        public long Ping { get; private set; }

        /// <summary>
        ///     Icon DATA
        /// </summary>
        public byte[] IconData { get; set; }

        /// <summary>
        ///     连接状态
        /// </summary>
        public StateType State { get; set; }

        /// <summary>
        ///     获取与特定格式代码相关联的颜色代码
        /// </summary>
        public static Dictionary<char, string> MinecraftColors => new Dictionary<char, string>
        {
            {'0', "#000000"}, {'1', "#0000AA"}, {'2', "#00AA00"}, {'3', "#00AAAA"}, {'4', "#AA0000"}, {'5', "#AA00AA"},
            {'6', "#FFAA00"}, {'7', "#AAAAAA"}, {'8', "#555555"}, {'9', "#5555FF"}, {'a', "#55FF55"}, {'b', "#55FFFF"},
            {'c', "#FF5555"}, {'d', "#FF55FF"}, {'e', "#FFFF55"}, {'f', "#FFFFFF"}
        };

        public async void StartGetServerInfo()
        {
            TcpClient tcp = null;
            try
            {
                // Some code source form:
                // Minecraft Client v1.9.0 for Minecraft 1.4.6 to 1.9.0 - By ORelio under CDDL-1.0
                // wiki.vg
                try
                {
                    tcp = new TcpClient(ServerAddress, ServerPort);
                }
                catch (SocketException)
                {
                    var client = new LookupClient();
                    var result = await client.QueryAsync("_minecraft._tcp." + ServerAddress, QueryType.SRV)
                        .ConfigureAwait(false);
                    var record = result?.Answers?.OfType<SrvRecord>().FirstOrDefault();
                    if (record != null)
                    {
                        tcp = new TcpClient(record.Target, record.Port);
                        ServerAddress = record.Target;
                        ServerPort = record.Port;
                    }
                    else
                    {
                        State = StateType.BAD_CONNECT;
                        return;
                    }
                }

                try
                {
                    tcp.ReceiveBufferSize = 1024 * 1024;

                    var packet_id = ProtocolHandler.getVarInt(0);
                    var protocol_version = ProtocolHandler.getVarInt(-1);
                    var server_adress_val = Encoding.UTF8.GetBytes(ServerAddress);
                    var server_adress_len = ProtocolHandler.getVarInt(server_adress_val.Length);
                    var server_port = BitConverter.GetBytes(ServerPort);
                    Array.Reverse(server_port);
                    var next_state = ProtocolHandler.getVarInt(1);
                    var packet2 = ProtocolHandler.concatBytes(packet_id, protocol_version, server_adress_len,
                        server_adress_val, server_port, next_state);
                    var tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(packet2.Length), packet2);

                    var status_request = ProtocolHandler.getVarInt(0);
                    var request_packet = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(status_request.Length),
                        status_request);

                    tcp.Client.Send(tosend, SocketFlags.None);

                    tcp.Client.Send(request_packet, SocketFlags.None);
                    var handler = new ProtocolHandler(tcp);
                    var packetLength = handler.readNextVarIntRAW();
                    if (packetLength > 0)
                    {
                        var packetData = new List<byte>(handler.readDataRAW(packetLength));
                        if (ProtocolHandler.readNextVarInt(packetData) == 0x00) //Read Packet ID
                        {
                            var result = ProtocolHandler.readNextString(packetData); //Get the Json data
                            JsonResult = result;
                            SetInfoFromJsonText(result);
                        }
                    }

                    var ping_id = ProtocolHandler.getVarInt(1);
                    var ping_content = BitConverter.GetBytes((long)233);
                    var ping_packet = ProtocolHandler.concatBytes(ping_id, ping_content);
                    var ping_tosend =
                        ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(ping_packet.Length), ping_packet);

                    try
                    {
                        tcp.ReceiveTimeout = 1000;

                        var pingWatcher = new Stopwatch();

                        pingWatcher.Start();
                        tcp.Client.Send(ping_tosend, SocketFlags.None);

                        var pingLenghth = handler.readNextVarIntRAW();
                        pingWatcher.Stop();
                        if (pingLenghth > 0)
                        {
                            var packetData = new List<byte>(handler.readDataRAW(pingLenghth));
                            if (ProtocolHandler.readNextVarInt(packetData) == 0x01) //Read Packet ID
                            {
                                long content = ProtocolHandler.readNextByte(packetData); //Get the Json data
                                if (content == 233) Ping = pingWatcher.ElapsedMilliseconds;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Ping = 0;
                    }
                }
                catch (SocketException)
                {
                    State = StateType.NO_RESPONSE;
                }

                tcp.Close();
            }
            catch (SocketException)
            {
                State = StateType.BAD_CONNECT;
            }
            catch (Exception)
            {
                State = StateType.EXCEPTION;
            }
            finally
            {
                tcp?.Dispose();
            }
        }

        public async Task StartGetServerInfoAsync()
        {
            await Task.Factory.StartNew(() => { StartGetServerInfo(); });
        }

        private void SetInfoFromJsonText(string JsonText)
        {
            JsonText = ClearColor(JsonText);
            if (!string.IsNullOrEmpty(JsonText) && JsonText.StartsWith("{") && JsonText.EndsWith("}"))
            {
                var jsonData = JObject.Parse(JsonText);

                if (jsonData.ContainsKey("version"))
                {
                    var versionData = (JObject)jsonData["version"];
                    GameVersion = versionData["name"].ToString();
                    ProtocolVersion = int.Parse(versionData["protocol"].ToString());
                }

                if (jsonData.ContainsKey("players"))
                {
                    var playerData = (JObject)jsonData["players"];
                    MaxPlayerCount = int.Parse(playerData["max"].ToString());
                    CurrentPlayerCount = int.Parse(playerData["online"].ToString());
                    if (playerData.ContainsKey("sample"))
                    {
                        OnlinePlayersName = new List<string>();
                        foreach (JObject name in playerData["sample"])
                            if (name.ContainsKey("name"))
                            {
                                var playername = name["name"].ToString();
                                OnlinePlayersName.Add(playername);
                            }
                    }
                }

                if (jsonData.ContainsKey("description"))
                {
                    var descriptionData = jsonData["description"];
                    if (descriptionData.Type == JTokenType.String)
                    {
                        MOTD = descriptionData.ToString();
                    }
                    else if (descriptionData.Type == JTokenType.Object)
                    {
                        var descriptionDataObj = (JObject)descriptionData;
                        if (descriptionDataObj.ContainsKey("extra"))
                            foreach (var item in descriptionDataObj["extra"])
                            {
                                var text = item["text"].ToString();
                                if (!string.IsNullOrWhiteSpace(text)) MOTD += text;
                            }
                        else if (descriptionDataObj.ContainsKey("text")) MOTD = descriptionDataObj["text"].ToString();
                    }
                    //if (descriptionData.ContainsKey("extra"))
                    //{
                    //    //Json.JSONData extraData = jsonData.DataArray;
                    //    foreach (JToken item in descriptionData["extra"])
                    //    {
                    //        string text = item["text"].ToString();
                    //        if (!string.IsNullOrWhiteSpace(text))
                    //        {
                    //            this.MOTD += text;
                    //        }
                    //    }
                    //}
                    //else if (descriptionData.ContainsKey("text"))
                    //{
                    //    this.MOTD = descriptionData["text"].ToString();
                    //}
                    //else
                    //{

                    //}
                }

                // Check for forge on the server.
                if (jsonData.ContainsKey("modinfo") && jsonData["modinfo"].Type == JTokenType.Object)
                {
                    var modData = (JObject)jsonData["modinfo"];
                    if (modData.ContainsKey("type") && modData["type"].ToString() == "FML")
                    {
                        ForgeInfo = new ForgeInfo(modData);
                        if (!ForgeInfo.Mods.Any()) ForgeInfo = null;
                    }
                }

                if (jsonData.ContainsKey("favicon"))
                    try
                    {
                        var datastring = jsonData["favicon"].ToString();
                        var arr = Convert.FromBase64String(datastring.Replace("data:image/png;base64,", ""));
                        IconData = arr;
                        //using (MemoryStream ms = new MemoryStream(arr))
                        //{
                        //    this.Icon = new Bitmap(ms);
                        //}
                    }
                    catch
                    {
                        IconData = null;
                    }

                State = StateType.GOOD;
            }
        }

        private string ClearColor(string str)
        {
            str = str.Replace(@"\n", "");
            while (str.Contains('§')) str = str.Remove(str.IndexOf('§'), 2);
            return str;
        }

        //[DllImport("gdi32.dll", SetLastError = true)]
        //private static extern bool DeleteObject(IntPtr hObject);

        ///// <summary>  
        ///// 从bitmap转换成ImageSource  
        ///// </summary>  
        ///// <param name="icon"></param>  
        ///// <returns></returns>  
        //public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        //{
        //    //Bitmap bitmap = icon.ToBitmap();  
        //    IntPtr hBitmap = bitmap.GetHbitmap();

        //    ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //        hBitmap,
        //        IntPtr.Zero,
        //        Int32Rect.Empty,
        //        BitmapSizeOptions.FromEmptyOptions());
        //    if (!DeleteObject(hBitmap))
        //    {
        //        throw new System.ComponentModel.Win32Exception();
        //    }
        //    return wpfBitmap;

        //}
    }
}