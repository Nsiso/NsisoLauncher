using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public class AuthenticationNode
    {
        /// <summary>
        /// The name of this node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is need password.
        /// </summary>
        public bool NeedPassword { get; set; }

        /// <summary>
        /// The base url of auth api.
        /// </summary>
        public string ApiBase { get; set; }

        /// <summary>
        /// The depens of auth.
        /// </summary>
        public List<string> Depends { get; set; }
    }
}
