using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Mirrors;
using System;
using System.IO;
using System.Threading;
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

    public class FabricInstallOptions : CommonInstallOptions
    {
        public Net.FunctionAPI.APIModules.JWFabric Fabric { get; set; }
    }

    public class CommonInstallOptions
    {
        public string GameRootPath { get; set; }

        public bool IsClient { get; set; }

        public VersionBase VersionToInstall { get; set; }

        public IDownloadableMirror Mirror { get; set; }

        public Java Java { get; set; }
    }

    public class CommonInstaller : IInstaller
    {
        public string InstallerPath { get; set; }
        public CommonInstallOptions Options { get; set; }

        public CommonInstaller(string installerPath, CommonInstallOptions options)
        {
            if (string.IsNullOrWhiteSpace(installerPath))
            {
                throw new ArgumentException("Installer path can not be null or whitespace.");
            }
            this.InstallerPath = installerPath;
            this.Options = options ?? throw new ArgumentNullException("Install options is null");
        }

        public void BeginInstall(ProgressCallback callback, CancellationToken cancellationToken)
        {
            string installerName = Path.GetFileNameWithoutExtension(InstallerPath);
            string tempPath = string.Format("{0}\\{1}Temp", PathManager.TempDirectory, installerName);
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            Unzip.UnZipFile(InstallerPath, tempPath);
            string mainJson = File.ReadAllText(tempPath + "\\install_profile.json");
            JObject jObject = JObject.Parse(mainJson);

            BeginInstallFromJObject(callback, cancellationToken, jObject, tempPath);

            Directory.Delete(tempPath, true);
            File.Delete(InstallerPath);
        }

        public void BeginInstallFromJObject(ProgressCallback callback, CancellationToken cancellationToken, JObject jObj, string tempPath)
        {
            var jsonObj = jObj.ToObject<CommonJsonObj>();
            var t = jsonObj.Install.Path.Split(':');
            var libPackage = t[0];
            var libName = t[1];
            var libVersion = t[2];
            string libPath = string.Format(@"{0}\libraries\{1}\{2}\{3}\{2}-{3}.jar",
                Options.GameRootPath, libPackage.Replace(".", @"\"), libName, libVersion);

            string libDir = Path.GetDirectoryName(libPath);
            if (!Directory.Exists(libDir))
            {
                Directory.CreateDirectory(libDir);
            }
            File.Copy(tempPath + '\\' + jsonObj.Install.FilePath, libPath, true);


            string newPath = PathManager.GetVersionJsonPath(Options.GameRootPath, jsonObj.Install.Target);
            string newDir = Path.GetDirectoryName(newPath);
            string jarPath = PathManager.GetVersionJarPath(Options.GameRootPath, jsonObj.Install.Target);

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            File.WriteAllText(newPath, jsonObj.VersionInfo.ToString());
            File.Copy(tempPath + '\\' + jsonObj.Install.FilePath, jarPath, true);
        }

        public async Task BeginInstallAsync(ProgressCallback callback, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                BeginInstall(callback, cancellationToken);
            });
        }
    }
}