using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IMirror
    {
        string MirrorName { get; set; }

        Uri BaseUri { get; set; }
    }
}
