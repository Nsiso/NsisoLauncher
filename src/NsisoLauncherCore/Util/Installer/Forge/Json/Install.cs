using Newtonsoft.Json;
using NsisoLauncherCore.Modules;
using System.Collections.Generic;

namespace NsisoLauncherCore.Util.Installer.Forge.Json
{
    public class Install
    {
        /// <summary>
        /// Profile name to install and direct at this new version
        /// </summary>
        [JsonProperty("profile")]
        public string Profile { get; set; }

        /// <summary>
        /// Version name to install to.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Vanilla version this is based off of.
        /// </summary>
        [JsonProperty("minecraft")]
        public string Minecraft { get; set; }

        /// <summary>
        /// Version json to install into the client
        /// </summary>
        [JsonProperty("json")]
        public string Json { get; set; }

        /// <summary>
        /// Logo to be displayed on the installer GUI.
        /// </summary>
        [JsonProperty("logo")]
        public string Logo { get; set; }

        /// <summary>
        /// Maven artifact path for the 'main' jar to install.
        /// </summary>
        [JsonProperty("path")]
        public Artifact Path { get; set; }

        /// <summary>
        /// Icon to use for the url button
        /// </summary>
        [JsonProperty("urlIcon")]
        public string UrlIcon { get; set; }

        /// <summary>
        /// Welcome message displayed on main install panel.
        /// </summary>
        [JsonProperty("welcome")]
        public string Welcome { get; set; }

        /// <summary>
        /// Data files to be extracted during install, used for processor.
        /// </summary>
        [JsonProperty("data")]
        public Dictionary<string, DataFile> Data { get; set; }

        /// <summary>
        /// Extra libraries needed by processors, that may differ from the installer version's library list. Uses the same format as Mojang for simplicities sake.
        /// </summary>
        [JsonProperty("libraries")]
        public List<Library> Libraries { get; set; }

        /// <summary>
        /// Executable jars to be run after all libraries have been downloaded.
        /// </summary>
        [JsonProperty("processors")]
        public List<Processor> Processors { get; set; }

        public Dictionary<string, string> GetData(bool client)
        {
            Dictionary<string, string> datas = new Dictionary<string, string>();
            if (Data == null)
            {
                return datas;
            }
            else
            {
                foreach (var item in Data)
                {
                    if (client)
                    {
                        datas.Add(item.Key, item.Value.Client);
                    }
                    else
                    {
                        datas.Add(item.Key, item.Value.Server);
                    }
                }
                return datas;
            }
        }

        public class Processor
        {
            [JsonProperty("sides")]
            public List<string> Sides { get; set; }

            /// <summary>
            /// The executable jar to run, The installer will run it in-process, but external tools can run it using java -jar {file}, so MANFEST Main-Class entry must be valid.
            /// </summary>
            [JsonProperty("jar")]
            public Artifact Jar { get; set; }

            /// <summary>
            /// Dependency list of files needed for this jar to run. Aything listed here SHOULD be listed in {@see Install#libraries} so the installer knows to download it.
            /// </summary>
            [JsonProperty("classpath")]
            public List<Artifact> ClassPath { get; set; }

            /*
             * Arguments to pass to the jar, can be in the following formats:
             * [Artifact] : A artifact path in the target maven style repo, where all libraries are downloaded to.
             * {DATA_ENTRY} : A entry in the Install#data map, extract as a file, there are a few extra specified values to allow the same processor to run on both sides:
             *   {MINECRAFT_JAR} - The vanilla minecraft jar we are dealing with, /versions/VERSION/VERSION.jar on the client and /minecraft_server.VERSION.jar for the server
             *   {SIDE} - Either the exact string "client", "server", and "extract" depending on what side we are installing.
             */
            [JsonProperty("args")]
            public string[] Args { get; set; }
        }

        public class DataFile
        {
            /*
              * Can be in the following formats:
              * [value] - An absolute path to an artifact located in the target maven style repo.
              * 'value' - A string literal, remove the 's and use this value
              * value - A file in the installer package, to be extracted to a temp folder, and then have the absolute path in replacements.
              */

            [JsonProperty("client")]
            public string Client { get; set; }

            [JsonProperty("server")]
            public string Server { get; set; }
        }
    }
}
