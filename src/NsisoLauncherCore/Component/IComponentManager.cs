using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Component
{
    public interface IComponentManager<T> where T : IComponent
    {
        ObservableCollection<T> Items { get; }

        void Add(string path);

        void Remove(T item);

        void Enable(T item);

        void Disable(T item);

        void Refresh();
    }
}
