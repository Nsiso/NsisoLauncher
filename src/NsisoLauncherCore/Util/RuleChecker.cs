using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Util
{
    public static class RuleChecker
    {
        /// <summary>
        /// 判断规则
        /// </summary>
        /// <param name="rules">规则列表</param>
        /// <returns>是否启用</returns>
        public static bool CheckAllowed(List<RuleInfo> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                return true;
            }
            bool allowed = false;
            foreach (var rule in rules)
            {
                if (rule.Os == null)
                {
                    allowed = rule.Action == "allow";
                }
                else if (rule.Os.Name == "windows")
                {
                    allowed = rule.Action == "allow";
                }
            }
            return allowed;
        }
    }
}
