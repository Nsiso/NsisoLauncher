using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NsisoLauncherCore.Util
{
    public class CrashHelper
    {
        public Queue<Log> LogQueue { get; set; }

        public CrashHelper()
        {
            LogQueue = new Queue<Log>(3);
        }

        public void AppendLog(Log log)
        {
            if (LogQueue.Count >= 3)
            {
                LogQueue.Dequeue();
            }
            LogQueue.Enqueue(log);
        }

        private string GetCrashInfo(LaunchHandler handler, GameExitArg exitArg)
        {
            if (!exitArg.Instance.HasExited)
            {
                throw new Exception("The game is running.");
            }
            if (exitArg.IsNormalExit())
            {
                throw new ArgumentException("It seems that the game is safe exit.Exit code equal zero");
            }
            Process process = exitArg.Instance.InstanceProcess;
            DateTime launchTime = process.StartTime;
            DateTime exitTime = process.ExitTime;
            string verRootDir = handler.GetVersionWorkspaceDir(exitArg.Version);
            string crashreportDir = verRootDir + "\\crash-reports";
            string latestlogPath = verRootDir + "\\logs\\latest.log";

            if (Directory.Exists(crashreportDir))
            {
                var files = Directory.EnumerateFiles(crashreportDir);
                var logs = files.Where((x) =>
                {
                    string time = Path.GetFileName(x).Substring(6, 19).Replace('-', '/').Replace('_', ' ').Replace('.', ':');
                    if (DateTime.TryParse(time, out DateTime logtime))
                    {
                        return (launchTime < logtime) && (logtime < exitTime);
                    }
                    else
                    {
                        return false;
                    }
                });
                if (logs.Count() != 0)
                {
                    return File.ReadAllText(logs.FirstOrDefault());
                }
                else
                {
                    return null;
                }
            }

            //没有崩溃日志直接查找log
            else if (File.Exists(latestlogPath))
            {
                DateTime lastWrtTime = File.GetLastWriteTime(latestlogPath);
                if ((launchTime < lastWrtTime) && (lastWrtTime < exitTime))
                {
                    string[] allLogArr = File.ReadAllLines(latestlogPath);
                    List<int> valiLogNo = new List<int>();
                    bool keepRead = false;
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < allLogArr.Length; i++)
                    {
                        string current = allLogArr[i];

                        if (keepRead)
                        {
                            builder.AppendLine(allLogArr[i - 1]);
                        }

                        //最简单的检查
                        if (!current.StartsWith("["))
                        {
                            continue;
                        }

                        //寻找第一个固定要素区块
                        var firstLBrac = current.IndexOf('[');
                        if (firstLBrac == -1)
                        {
                            continue;
                        }

                        var firstRBrac = current.IndexOf(']');
                        if (firstRBrac == -1)
                        {
                            continue;
                        }

                        var firstBlock = current.Substring(firstLBrac + 1, firstRBrac - firstLBrac - 1);

                        //寻找第二个固定要素区块
                        var secondLBrac = current.IndexOf('[', firstRBrac);
                        if (secondLBrac == -1)
                        {
                            continue;
                        }

                        var secondRBrac = current.IndexOf(']', secondLBrac);
                        if (secondRBrac == -1)
                        {
                            continue;
                        }

                        var secondBlock = current.Substring(secondLBrac + 1, secondRBrac - secondLBrac - 1);

                        if (DateTime.TryParse(firstBlock, out DateTime time))
                        {
                            if (secondBlock.Contains("ERROR"))
                            {
                                keepRead = true;
                            }
                            else
                            {
                                keepRead = false;
                            }
                        }
                    }
                    return builder.ToString();
                }
                else
                {
                    return null;
                }
            }

            else
            {
                return null;
            }

        }
    }
}
