using NsisoLauncherPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncher.Plugin
{
    class PluginManager
    {
        public List<IPlugin> Plugins { get; set; }

        void AddPlugin(string pluginPath)
        {
            Assembly pluginAssembly = LoadPluginAssembly(pluginPath);
            IPlugin plugin = LoadPluginFromAssembly(pluginAssembly);
            Plugins.Add(plugin);
        }

        private Assembly LoadPluginAssembly(string pluginPath)
        {
            if (File.Exists(pluginPath))
            {
                throw new FileNotFoundException("The plugin was not found.");
            }
            Console.WriteLine($"Loading assembly from: {pluginPath}");
            PluginLoadContext loadContext = new(pluginPath);
            string pluginName = Path.GetFileNameWithoutExtension(pluginPath);
            return loadContext.LoadFromAssemblyName(new AssemblyName(pluginName));
        }

        private IPlugin LoadPluginFromAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    IPlugin result = Activator.CreateInstance(type) as IPlugin;
                    return result;
                }
            }
            return null;
        }
    }
}
