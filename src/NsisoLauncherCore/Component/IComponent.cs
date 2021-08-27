using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Component
{
    public interface IComponent : INotifyPropertyChanged
    {
        string Path { get; }

        string Name { get; }

        ComponentState State { get; }
    }

    public enum ComponentState
    {
        DISABLE = 0,
        ENABLE = 1
    }
}
