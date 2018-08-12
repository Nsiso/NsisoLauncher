using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NsisoLauncher.Core
{
    public class ArgumentsParser
    {
        private LaunchHandler handler;

        public ArgumentsParser(LaunchHandler handler)
        {
            this.handler = handler;
        }

        public string Parse(Modules.LaunchSetting setting)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            #region 处理JVM启动头参数
            StringBuilder jvmHead = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(setting.JavaAgent))
            {
                jvmHead.Append("-javaagent:");
                jvmHead.Append(setting.JavaAgent.Trim());
                jvmHead.Append(" ");
            }
            if (setting.GCEnabled)
            {
                switch (setting.GCType)
                {
                    case Modules.GCType.G1GC:
                        jvmHead.Append("-XX:+UseG1GC");
                        break;
                    case Modules.GCType.SerialGC:
                        jvmHead.Append("-XX:+UseSerialGC");
                        break;
                    case Modules.GCType.ParallelGC:
                        jvmHead.Append("-XX:+UseParallelGC");
                        break;
                    case Modules.GCType.CMSGC:
                        jvmHead.Append("-XX:+UseConcMarkSweepGC");
                        break;
                    default:
                        break;
                }
                jvmHead.Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(setting.GCArgument))
            {
                jvmHead.Append(setting.GCArgument).Append(' ');
            }
            if (setting.MinMemory != 0)
            {
                jvmHead.Append(string.Format("-Xms{0}m ", setting.MinMemory));
            }
            if (setting.MaxMemory != 0)
            {
                jvmHead.Append(string.Format("-Xmx{0}m ", setting.MaxMemory));
            }
            if (!string.IsNullOrWhiteSpace(setting.AdvencedJvmArguments))
            {
                jvmHead.Append(setting.AdvencedJvmArguments).Append(' ');
            }
            jvmHead.Append("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true ");
            #endregion

            #region 处理游戏参数
            string assetsPath = string.Format("\"{0}\\assets\"", handler.GameRootPath);
            string legacy = setting.AuthenticateUUID.Legacy ? "Legacy" : "Mojang";
            string gameDir = string.Format("\"{0}\"", handler.GetGameVersionRootDir(setting.Version));
            Dictionary<string, string> gameArgDic = new Dictionary<string, string>()
            {
                {"${auth_player_name}",string.Format("\"{0}\"", setting.AuthenticateUUID.PlayerName) },
                {"${auth_session}",setting.AuthenticateAccessToken },
                {"${version_name}",string.Format("\"{0}\"", setting.Version.ID) },
                {"${game_directory}",gameDir },
                {"${game_assets}",assetsPath },
                {"${assets_root}",assetsPath },
                {"${assets_index_name}",setting.Version.Assets },
                {"${auth_uuid}",setting.AuthenticateUUID.Value },
                {"${auth_access_token}",setting.AuthenticateAccessToken },
                {"${user_properties}",ToList(setting.AuthenticationUserData?.Properties) },
                {"${user_type}",legacy },
                {"${version_type}", string.IsNullOrWhiteSpace(setting.VersionType) ? "NsisoLauncher":setting.VersionType }
            };
            StringBuilder otherGamearg = new StringBuilder();
            if (setting.WindowSize != null)
            {
                if (setting.WindowSize.FullScreen)
                {
                    otherGamearg.Append(" --fullscreen");
                }
                else
                {
                    if (setting.WindowSize.Width > 0)
                    {
                        otherGamearg.AppendFormat(" --width {0}", setting.WindowSize.Width);
                    }
                    if (setting.WindowSize.Height > 0)
                    {
                        otherGamearg.AppendFormat(" --height {0}", setting.WindowSize.Height);
                    }
                }
            }
            if (setting.LaunchToServer != null)
            {
                if (!string.IsNullOrWhiteSpace(setting.LaunchToServer.Address))
                {
                    otherGamearg.AppendFormat(" --server {0}", setting.LaunchToServer.Address);
                }
                if (setting.LaunchToServer.Port > 0)
                {
                    otherGamearg.AppendFormat(" --port {0}", setting.LaunchToServer.Port);
                }
            }
            if (!string.IsNullOrWhiteSpace(setting.AdvencedGameArguments))
            {
                otherGamearg.Append(' ' + setting.AdvencedGameArguments);
            }
            string gameArg = ReplaceByDic(setting.Version.MinecraftArguments, gameArgDic) + otherGamearg.ToString();

            #endregion

            #region 处理游戏JVM参数
            Dictionary<string, string> jvmArgDic = new Dictionary<string, string>()
            {
                {"${natives_directory}",string.Format("\"{0}{1}\"",handler.GetGameVersionRootDir(setting.Version), @"\$natives") },
                {"${launcher_name}","NsisoLauncher" },
                {"${launcher_version}", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                {"${classpath}", GetClassPaths(setting.Version.Libraries,setting.Version) },
            };

            string jvmArg = ReplaceByDic(setting.Version.JvmArguments, jvmArgDic);
            #endregion

            stopwatch.Stop();

            string allArg = jvmHead.ToString() + jvmArg + ' ' + setting.Version.MainClass + ' ' + gameArg;
            App.logHandler.AppendDebug(string.Format("完成启动参数转换,用时:{0}ms", stopwatch.ElapsedMilliseconds));
            App.logHandler.AppendDebug(string.Format("启动参数:{0}", allArg));

            return allArg.Trim();
        }

        private static string ToList(List<Net.MojangApi.Responses.AuthenticateResponse.UserData.Property> properties)
        {
            if (properties == null)
            {
                return "{}";
            }
            var sb = new StringBuilder();
            sb.Append('{');
            foreach (var item in properties)
            {
                sb.AppendFormat("\"{0}\":[\"{1}\"]", item.Name, item.Value);
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string GetClassPaths(List<Modules.Library> libs, Modules.Version ver)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append('\"');

            foreach (var item in libs)
            {
                stringBuilder.AppendFormat("{0};", handler.GetLibraryPath(item));
            }
            stringBuilder.Append(handler.GetJarPath(ver));

            stringBuilder.Append('\"');
            return stringBuilder.ToString();
        }

        private static string ReplaceByDic(string str, Dictionary<string,string> dic)
        {
            return dic.Aggregate(str, (a, b) => a.Replace(b.Key, b.Value));
        }
    }
}