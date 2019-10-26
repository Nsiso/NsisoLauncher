using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Installer
{
    public class CommonJsonObj
    {
        [JsonProperty("install")]
        public InstallInfo Install { get; set; }

        [JsonProperty("versionInfo")]
        public JObject VersionInfo { get; set; }

        public class InstallInfo
        {
            [JsonProperty("target")]
            public string Target { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("filePath")]
            public string FilePath { get; set; }
        }
    }

    public class CommonInstallOptions : IInstallOptions
    {
        public string GameRootPath { get; set; }
    }

    public class CommonInstaller : IInstaller
    {
        public string InstallerPath { get; set; }
        public IInstallOptions Options { get; set; }

        public CommonInstaller(string installerPath, CommonInstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(installerPath))
            {
                throw new ArgumentException("Installer path can not be null or whitespace.");
            }
            this.InstallerPath = installerPath;
            if (string.IsNullOrWhiteSpace(options.GameRootPath))
            {
                throw new ArgumentException("Game root path in options can not be null or whitespace.");
            }
            this.Options = options;
        }

        public void BeginInstall()
        {
            string gameRootPath = ((CommonInstallOptions)Options).GameRootPath;
            string tempPath = PathManager.TempDirectory + "\\CommonInstallerTemp";
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            Unzip.UnZipFile(InstallerPath, tempPath);
            string mainJson = File.ReadAllText(tempPath + "\\install_profile.json");
            var jsonObj = JsonConvert.DeserializeObject<CommonJsonObj>(mainJson);

            var t = jsonObj.Install.Path.Split(':');
            var libPackage = t[0];
            var libName = t[1];
            var libVersion = t[2];
            string libPath = string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.jar",
                gameRootPath, libPackage.Replace(".", @"\"), libName, libVersion);

            string libDir = Path.GetDirectoryName(libPath);
            if (!Directory.Exists(libDir))
            {
                Directory.CreateDirectory(libDir);
            }
            File.Copy(tempPath + '\\' + jsonObj.Install.FilePath, libPath, true);


            string newPath = PathManager.GetJsonPath(gameRootPath, jsonObj.Install.Target);
            string newDir = Path.GetDirectoryName(newPath);
            string jarPath = PathManager.GetJarPath(gameRootPath, jsonObj.Install.Target);

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            File.WriteAllText(newPath, jsonObj.VersionInfo.ToString());
            File.Copy(tempPath + '\\' + jsonObj.Install.FilePath, jarPath, true);

            Directory.Delete(tempPath, true);
            File.Delete(InstallerPath);
        }

        public async Task BeginInstallAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                BeginInstall();
            });
        }
    }
}
