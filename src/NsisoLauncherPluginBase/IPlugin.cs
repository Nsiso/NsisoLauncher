using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherPluginBase
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }

        Version Version { get; }
    }
}
