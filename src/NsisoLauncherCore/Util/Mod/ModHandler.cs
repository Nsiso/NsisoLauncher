using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Mod
{
    public class ModHandler
    {
        LaunchHandler _launchHandler;

        public ModHandler(LaunchHandler handler)
        {
            _launchHandler = handler;
        }

        public List<ModInfo> GetMods(Modules.Version version)
        {
            if (_launchHandler == null)
            {
                throw new ArgumentNullException("The launch handler is null, can't find mods");
            }

            if (version == null)
            {
                throw new ArgumentNullException("The version is null, can't find mods");
            }

            string modsDir = _launchHandler.GetVersionModsDir(version);
            if (!Directory.Exists(modsDir))
            {
                return null;
            }
            string[] modsPath = Directory.GetFiles(modsDir, "*.jar");
            List<ModInfo> mods = new List<ModInfo>();

            foreach (var item in modsPath)
            {
                mods.Add(new ModInfo() { ModPath = item });
            }
            return mods;
        }

        public Task<List<ModInfo>> GetModsAsync(Modules.Version version)
        {
            return Task.Run(() =>
            {
                return GetMods(version);
            });
        }
    }
}
