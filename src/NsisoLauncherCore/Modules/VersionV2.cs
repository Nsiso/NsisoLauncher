using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Modules
{
    public class VersionV2 : VersionBase
    {
        public VersionArguments Arguments { get; set; }

        public override string GetJvmLaunchArguments()
        {
            string current_arg = BuildV2ArgFromJTokenList(Arguments.Jvm);
            if (InheritsFromInstance == null)
            {
                return current_arg;
            }
            else
            {
                string base_arg = InheritsFromInstance.GetJvmLaunchArguments();
                return string.Format("{0} {1}", base_arg, current_arg);
            }
        }

        public override string GetGameLaunchArguments()
        {
            string current_arg = BuildV2ArgFromJTokenList(Arguments.Game);
            if (InheritsFromInstance == null)
            {
                return current_arg;
            }
            else
            {
                string base_arg = InheritsFromInstance.GetGameLaunchArguments();
                return string.Format("{0} {1}", base_arg, current_arg);
            }
        }

        private string BuildV2ArgFromJTokenList(List<JToken> args)
        {
            StringBuilder argBuilder = new StringBuilder();
            if (args == null)
            {
                return null;
            }
            foreach (var arg in args)
            {
                switch (arg.Type)
                {
                    case JTokenType.Object:
                        {
                            JObject argObj = arg.ToObject<JObject>();
                            if (argObj.ContainsKey("rules") && argObj.ContainsKey("value"))
                            {
                                List<Rule> rules = argObj["rules"].ToObject<List<Rule>>();
                                if (RuleChecker.CheckRules(rules))
                                {
                                    if (arg["value"].Type == JTokenType.String)
                                    {
                                        string value = arg["value"].ToString();
                                        argBuilder.AppendFormat("\"{0}\" ", value);
                                    }
                                    else if (arg["value"].Type == JTokenType.Array)
                                    {
                                        foreach (var str in arg["value"])
                                        {
                                            string value = str.ToString();
                                            argBuilder.AppendFormat("\"{0}\" ", value);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case JTokenType.String:
                        argBuilder.AppendFormat("\"{0}\" ", arg.ToString());
                        break;
                    default:
                        break;
                }

            }
            return argBuilder.ToString().Trim();
        }
    }

    public class VersionArguments
    {
        public List<JToken> Game { get; set; }

        public List<JToken> Jvm { get; set; }
    }
}
