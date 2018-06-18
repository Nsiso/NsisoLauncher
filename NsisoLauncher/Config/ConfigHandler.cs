using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NsisoLauncher.Config
{
    public class ConfigHandler
    {
        /// <summary>
        /// 配置文件存放文件夹
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// 首要配置文件路径
        /// </summary>
        public string MainConfigPath { get => Directory.FullName + @"\MainConfig.json"; }

        /// <summary>
        /// 首要配置文件内容
        /// </summary>
        public MainConfig MainConfig { get; set; }

        private object locker = new object();

        public ConfigHandler()
        {
            Directory = new DirectoryInfo("Config");

            if (!Directory.Exists)
            {
                Directory.Create();
            }

            if (!File.Exists(MainConfigPath))
            { NewConfig(); }
            else
            { Read(); }
            
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void Save()
        {
            lock(locker)
            {
                File.WriteAllText(MainConfigPath, JsonConvert.SerializeObject(MainConfig));
            }
        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        public void Read()
        {
            lock (locker)
            {
                MainConfig = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(MainConfigPath));
            }
        }
        
        /// <summary>
        /// 新建配置文件
        /// </summary>
        private void NewConfig()
        {
            MainConfig = new MainConfig()
            {
                User = new User()
                {
                    ClientToken = Guid.NewGuid().ToString("N"),
                    AuthenticationType = AuthenticationType.OFFLINE
                },
                History = new History()
                {

                },
                Environment = new Environment()
                {
                    AutoMemory = true,
                    GamePathType = GameDirEnum.ROOT,
                    DownloadLostAssets = true,
                    DownloadLostDepend = true,
                    GCEnabled = true,
                    GCType = Core.Modules.GCType.G1GC,
                    AutoJava = true
                },
                Download = new Download()
                {
                    DownloadSource = Core.Net.DownloadSource.BMCLAPI,
                    DownloadThreadsSize = 5
                },
                Launcher = new Launcher()
                {
                    Debug = false
                },
                Server = new Server()
                {
                    
                },
                Customize = new Customize()
                {

                }
            };
            Save();
        }
    }
}
