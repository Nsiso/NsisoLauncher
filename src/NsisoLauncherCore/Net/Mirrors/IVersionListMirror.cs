using System;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IVersionListMirror : IMirror
    {
        Uri VersionListUri { get; set; }

        Uri CoreVersionListUri { get; set; }
    }
}
