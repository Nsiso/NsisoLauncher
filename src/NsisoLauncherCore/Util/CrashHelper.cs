using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NsisoLauncherCore.Util
{
    public class CrashHelper
    {
        public string GetCrashInfo(LaunchHandler handler, GameExitArg exitArg)
        {
            if (!exitArg.Process.HasExited) throw new Exception("The game is running.");
            if (exitArg.IsNormalExit())
                throw new ArgumentException("It seems that the game is safe exit.Exit code equal zero");
            var process = exitArg.Process;
            var launchTime = process.StartTime;
            var exitTime = process.ExitTime;
            var verRootDir = handler.GetGameVersionRootDir(exitArg.Version);
            var crashreportDir = verRootDir + "\\crash-reports";
            var latestlogPath = verRootDir + "\\logs\\latest.log";

            if (Directory.Exists(crashreportDir))
            {
                var files = Directory.EnumerateFiles(crashreportDir);
                var logs = files.Where(x =>
                {
                    var time = Path.GetFileName(x).Substring(6, 19).Replace('-', '/').Replace('_', ' ')
                        .Replace('.', ':');
                    if (DateTime.TryParse(time, out var logtime))
                        return launchTime < logtime && logtime < exitTime;
                    return false;
                });
                if (logs.Count() != 0)
                    return File.ReadAllText(logs.FirstOrDefault());
                return null;
            }

            //没有崩溃日志直接查找log

            if (File.Exists(latestlogPath))
            {
                var lastWrtTime = File.GetLastWriteTime(latestlogPath);
                if (launchTime < lastWrtTime && lastWrtTime < exitTime)
                {
                    var allLogArr = File.ReadAllLines(latestlogPath);
                    var valiLogNo = new List<int>();
                    var keepRead = false;
                    var builder = new StringBuilder();
                    for (var i = 0; i < allLogArr.Length; i++)
                    {
                        var current = allLogArr[i];

                        if (keepRead) builder.AppendLine(allLogArr[i - 1]);

                        //最简单的检查
                        if (!current.StartsWith("[")) continue;

                        //寻找第一个固定要素区块
                        var firstLBrac = current.IndexOf('[');
                        if (firstLBrac == -1) continue;
                        var firstRBrac = current.IndexOf(']');
                        if (firstRBrac == -1) continue;
                        var firstBlock = current.Substring(firstLBrac + 1, firstRBrac - firstLBrac - 1);

                        //寻找第二个固定要素区块
                        var secondLBrac = current.IndexOf('[', firstRBrac);
                        if (secondLBrac == -1) continue;
                        var secondRBrac = current.IndexOf(']', secondLBrac);
                        if (secondRBrac == -1) continue;
                        var secondBlock = current.Substring(secondLBrac + 1, secondRBrac - secondLBrac - 1);

                        if (DateTime.TryParse(firstBlock, out var time))
                        {
                            if (secondBlock.Contains("ERROR"))
                                keepRead = true;
                            else
                                keepRead = false;
                        }
                    }

                    return builder.ToString();
                }

                return null;
            }

            return null;
        }
    }
}