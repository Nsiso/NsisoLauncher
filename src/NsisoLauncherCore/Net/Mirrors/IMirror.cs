using System;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IMirror
    {
        string MirrorName { get; set; }

        Uri BaseUri { get; set; }
    }
}
