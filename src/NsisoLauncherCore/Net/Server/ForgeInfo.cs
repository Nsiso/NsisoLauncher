using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NsisoLauncherCore.Net.Server
{
    /// <summary>
    ///     Contains information about a modded server install.
    /// </summary>
    public class ForgeInfo
    {
        public List<ForgeMod> Mods;

        /// <summary>
        ///     Create a new ForgeInfo from the given data.
        /// </summary>
        /// <param name="data">The modinfo JSON tag.</param>
        internal ForgeInfo(JToken data)
        {
            // Example ModInfo (with spacing):

            // "modinfo": {
            //     "type": "FML",
            //     "modList": [{
            //         "modid": "mcp",
            //         "version": "9.05"
            //     }, {
            //         "modid": "FML",
            //         "version": "8.0.99.99"
            //     }, {
            //         "modid": "Forge",
            //         "version": "11.14.3.1512"
            //     }, {
            //         "modid": "rpcraft",
            //         "version": "Beta 1.3 - 1.8.0"
            //     }]
            // }

            Mods = new List<ForgeMod>();
            foreach (var mod in data["modList"])
            {
                var modid = mod["modid"].ToString();
                var version = mod["version"].ToString();

                Mods.Add(new ForgeMod(modid, version));
            }
        }

        /// <summary>
        ///     Represents an individual forge mod.
        /// </summary>
        public class ForgeMod
        {
            public readonly string ModID;
            public readonly string Version;

            public ForgeMod(string ModID, string Version)
            {
                this.ModID = ModID;
                this.Version = Version;
            }

            public override string ToString()
            {
                return ModID + " [" + Version + ']';
            }
        }
    }
}