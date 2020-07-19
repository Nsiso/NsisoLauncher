using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static NsisoLauncherCore.Net.FunctionAPI.APIModules;

namespace NsisoLauncherCore.Net.Mirrors
{
    public interface IVersionListMirror : IMirror
    {
        Uri VersionListUri { get; set; }
    }
}
