using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    /// <summary>
    /// 验证节点设置
    /// </summary>
    public class AuthenticationNode : INotifyPropertyChanged
    {
        public AuthenticationNode(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public AuthenticationType AuthType { get; set; }

        /// <summary>
        /// authserver:验证服务器地址
        /// nide8ID:NIDE8的验证ID
        /// </summary>
        public Dictionary<string, string> Property { get; set; } = new Dictionary<string, string>();

        public bool Locked { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
