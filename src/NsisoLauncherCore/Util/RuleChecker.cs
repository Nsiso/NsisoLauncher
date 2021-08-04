using NsisoLauncherCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util
{
    public static class RuleChecker
    {
        public static bool CheckRule(Rule rule)
        {
            // p for postive and n for negative;
            bool p = rule.Action == "allow";
            bool n = !p;

            if (rule.Features != null)
            {
                return n;
            }

            if (rule.OS != null)
            {
                // check os name
                if (!string.IsNullOrWhiteSpace(rule.OS.Name) && rule.OS.Name != "windows")
                {
                    return n;
                }

                //check os arch
                if (!string.IsNullOrWhiteSpace(rule.OS.Arch))
                {
                    switch (SystemTools.GetSystemArch())
                    {
                        case ArchEnum.x32:
                            if (rule.OS.Arch != "x86")
                            {
                                return n;
                            }
                            break;
                        case ArchEnum.x64:
                            if (rule.OS.Arch != "x64")
                            {
                                return n;
                            }
                            break;
                        default:
                            return n;
                    }
                }

                // check os version
                if (!string.IsNullOrWhiteSpace(rule.OS.Version))
                {
                    if (!Regex.Match(Environment.OSVersion.Version.ToString(), rule.OS.Version).Success)
                    {
                        return n;
                    }
                }

            }

            return p;
        }

        public static bool CheckRules(List<Rule> rules)
        {
            if (rules == null)
            {
                return true;
            }
            foreach (var rule in rules)
            {
                if (!CheckRule(rule))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
