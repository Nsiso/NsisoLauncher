using Heijden.DNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Server
{
    public class ServerInfo : INotifyPropertyChanged
    {
        #region own property
        /// <summary>
        /// Server's address, support srv.
        /// </summary>
        [JsonIgnore]
        public string ServerAddress { get; set; }

        /// <summary>
        /// Server's runing port
        /// </summary>
        [JsonIgnore]
        public ushort ServerPort { get; set; }

        /// <summary>
        /// The server's name, it's used to display server name in ui.
        /// </summary>
        [JsonIgnore]
        public string ServerName { get; set; }
        #endregion

        #region Handshake json response property

        #endregion
        /// <summary>
        /// The server version info such name or protocol
        /// </summary>
        [JsonProperty("version")]
        public ServerVersionInfo Version { get; set; }

        /// <summary>
        /// The server player info such max or current player count and sample.
        /// </summary>
        [JsonProperty("players")]
        public ServerPlayerInfo Players { get; set; }

        /// <summary>
        /// Server's description (aka motd)
        /// </summary>
        [JsonProperty("description")]
        public Chat Description { get; private set; }

        /// <summary>
        /// server's favicon. is a png image that is base64 encoded
        /// </summary>
        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        /// <summary>
        /// Server's mod info including mod type and mod list (if is avaliable)
        /// </summary>
        [JsonProperty("modinfo")]
        public ModInfo ModInfo { get; private set; }

        #region The gen info
        [JsonIgnore]
        public string Motd { get => Description?.ToString(); }

        [JsonIgnore]
        public byte[] FaviconByteArray { get{ return Convert.FromBase64String(Favicon.Replace("data:image/png;base64,", "")); } }

        /// <summary>
        /// The ping delay time.(ms)
        /// </summary>
        [JsonIgnore]
        public long Ping { get; private set; }

        /// <summary>
        /// The handshake state
        /// </summary>
        [JsonIgnore]
        public StateType State { get; set; }

        /// <summary>
        /// The handshake message
        /// </summary>
        [JsonIgnore]
        public string Message { get; set; }
        #endregion

        /// <summary>
        /// 获取与特定格式代码相关联的颜色代码
        /// </summary>
        [JsonIgnore]
        public static Dictionary<char, string> MinecraftColors { get; private set; } = new Dictionary<char, string>()
        {
                    { '0', "#000000" },
                    { '1', "#0000AA" },
                    { '2', "#00AA00" },
                    { '3', "#00AAAA" },
                    { '4', "#AA0000" },
                    { '5', "#AA00AA" },
                    { '6', "#FFAA00" },
                    { '7', "#AAAAAA" },
                    { '8', "#555555" },
                    { '9', "#5555FF" },
                    { 'a', "#55FF55" },
                    { 'b', "#55FFFF" },
                    { 'c', "#FF5555" },
                    { 'd', "#FF55FF" },
                    { 'e', "#FFFF55" },
                    { 'f', "#FFFFFF" }
        };

        public enum StateType
        {
            GOOD,
            NO_RESPONSE,
            BAD_CONNECT,
            EXCEPTION
        }

        public ServerInfo(string ip, ushort port)
        {
            this.ServerAddress = ip;
            this.ServerPort = port;
            this.Favicon = "data:image/png;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
            this.PropertyChanged += ServerInfo_PropertyChanged;
        }

        private void ServerInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Motd"));
            }
            if (e.PropertyName == "Favicon")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FaviconByteArray"));
            }
        }

        public ServerInfo(Modules.Server info) : this(info.Address, info.Port)
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        public void StartGetServerInfo()
        {
            try
            {
                TcpClient tcp = null;

                try
                {
                    tcp = new TcpClient(this.ServerAddress, this.ServerPort);
                }
                catch (SocketException ex)
                {
                    RecordSRV result = (RecordSRV)new Resolver().Query("_minecraft._tcp." + this.ServerAddress, QType.SRV).Answers?.FirstOrDefault()?.RECORD;
                    if (result != null)
                    {
                        tcp = new TcpClient(result.TARGET, result.PORT);
                        this.ServerAddress = result.TARGET;
                        this.ServerPort = result.PORT;
                    }
                    else
                    {
                        this.State = StateType.BAD_CONNECT;
                        this.Message = ex.Message;
                        return;
                    }
                }

                tcp.ReceiveBufferSize = 1024 * 1024;

                try
                {
                    #region Handshake
                    /*
                    *  packid  filed name          filed type      notes
                    *  
                    *          Protocol Version    VarInt          See protocol version numbers. The version that the client plans on using to connect to the server (which is not important for the ping). If the client is pinging to determine what version to use, by convention -1 should be set.
                    *  
                    *          Server Address      String      	Hostname or IP, e.g. localhost or 127.0.0.1, that was used to connect. The Notchian server does not use this information. Note that SRV records are a complete redirect, e.g. if _minecraft._tcp.example.com points to mc.example.org, users connecting to example.com will provide mc.example.org as server address in addition to connecting to it.
                    *  0x00      
                    *          Server Port         Unsigned Short  Default is 25565. The Notchian server does not use this information.
                    *  
                    *          Next state          VarInt          Should be 1 for status, but could also be 2 for login.
                    *  
                    */

                    // Handshake:
                    // Packet ID: 0x00
                    byte[] packet_id = ProtocolHandler.getVarInt(0);

                    byte[] protocol_version = ProtocolHandler.getVarInt(-1);
                    byte[] server_adress_val = Encoding.UTF8.GetBytes(this.ServerAddress);
                    byte[] server_adress_len = ProtocolHandler.getVarInt(server_adress_val.Length);
                    byte[] server_port = BitConverter.GetBytes(this.ServerPort);
                    Array.Reverse(server_port);
                    byte[] next_state = ProtocolHandler.getVarInt(1);

                    byte[] concat_pack = ProtocolHandler.concatBytes(packet_id, protocol_version, server_adress_len, server_adress_val, server_port, next_state);
                    byte[] tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(concat_pack.Length), concat_pack);

                    tcp.Client.Send(tosend, SocketFlags.None);
                    #endregion

                    // Request
                    byte[] status_request = ProtocolHandler.getVarInt(0);
                    byte[] request_packet = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(status_request.Length), status_request);

                    tcp.Client.Send(request_packet, SocketFlags.None);

                    // Response
                    ProtocolHandler handler = new ProtocolHandler(tcp);
                    int packetLength = handler.readNextVarIntRAW();
                    if (packetLength > 0)
                    {
                        List<byte> packetData = new List<byte>(handler.readDataRAW(packetLength));
                        if (ProtocolHandler.readNextVarInt(packetData) == 0x00) //Read Packet ID
                        {
                            string result = ProtocolHandler.readNextString(packetData); //Get the Json data
                            JsonConvert.PopulateObject(result, this);
                        }
                    }

                    #region Ping
                    byte[] ping_id = ProtocolHandler.getVarInt(1);
                    byte[] ping_content = BitConverter.GetBytes((long)233);
                    byte[] ping_packet = ProtocolHandler.concatBytes(ping_id, ping_content);
                    byte[] ping_tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(ping_packet.Length), ping_packet);

                    try
                    {
                        tcp.ReceiveTimeout = 1000;

                        Stopwatch pingWatcher = new Stopwatch();

                        pingWatcher.Start();
                        tcp.Client.Send(ping_tosend, SocketFlags.None);

                        int pingLenghth = handler.readNextVarIntRAW();
                        pingWatcher.Stop();
                        if (pingLenghth > 0)
                        {
                            List<byte> packetData = new List<byte>(handler.readDataRAW(pingLenghth));
                            if (ProtocolHandler.readNextVarInt(packetData) == 0x01) //Read Packet ID
                            {
                                long content = ProtocolHandler.readNextByte(packetData); //Get the Json data
                                if (content == 233)
                                {
                                    this.Ping = pingWatcher.ElapsedMilliseconds;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this.Ping = 0;
                    }
                    #endregion
                }
                catch (SocketException)
                {
                    this.State = StateType.NO_RESPONSE;
                }
                tcp.Close();
            }
            catch (SocketException ex)
            {
                this.State = StateType.BAD_CONNECT;
                this.Message = ex.Message;
            }
            catch (Exception ex)
            {
                this.State = StateType.EXCEPTION;
                this.Message = ex.Message;
            }
        }

        public Task StartGetServerInfoAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                StartGetServerInfo();
            });
        }

        private string ClearColor(string str)
        {
            str = str.Replace(@"\n", "\n");
            while (str.Contains('§'))
            {
                str = str.Remove(str.IndexOf('§'), 2);
            }
            return str.Trim();
        }
    }

    public class ServerVersionInfo
    {
        public string Name { get; set; }

        public int Protocol { get; set; }
    }

    public class ServerPlayerInfo
    {
        public int Max { get; set; }

        public int Online { get; set; }

        public List<Player> Sample { get; set; }
    }

    public class Player
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }
}
