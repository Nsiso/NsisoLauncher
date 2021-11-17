using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil;
using Nsisnamespace NsisoLauncherCore.Net.Apis.Modules.Yggdrasil.Responses;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Yggdrasil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Auth
{
    public class MojangAuthenticator : YggdrasilAuthenticator
    {
        public MojangAuthenticator(NetRequester requester) : base("https://authserver.mojang.com", requester)
        { }
    }
}
