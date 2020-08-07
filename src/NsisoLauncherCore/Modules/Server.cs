using System;

namespace NsisoLauncherCore.Modules
{
    public class Server
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort Port { get; set; }

        public Server()
        {

        }

        public Server(string addr)
        {
            string[] arr = addr.Split(new char[] { ':' }, 2);
            if (arr.Length != 2)
            {
                throw new ArgumentException("The addr string is wrong");
            }
            this.Address = arr[0];
            this.Port = Convert.ToUInt16(arr[1]);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Address, Port);
        }
    }
}
