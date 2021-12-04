using System;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IFunctionalMirror : IMirror
    {
        Uri JavaListUri { get; set; }

        Uri ForgeListUri { get; set; }

        Uri LiteloaderListUri { get; set; }

        Uri OptifineListUri { get; set; }

        Uri FabricListUri { get; set; }
    }
}
