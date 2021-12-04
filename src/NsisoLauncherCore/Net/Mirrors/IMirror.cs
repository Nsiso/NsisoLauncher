using System;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IMirror
    {
        string MirrorName { get; set; }

        Uri MCDownloadUri { get; set; }
        Uri ForgeDownloadUri { get; set; }
        Uri FabricDownloadUri { get; set; }
    }
}
