using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public class YggdrasilSkin : ISkin
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public JObject Metadata { get; set; }

        public SkinType Type
        {
            get
            {
                if (Metadata != null &&
                    Metadata.ContainsKey("model") &&
                    Metadata["model"].Type == JTokenType.String &&
                    Metadata["model"].Value<string>() == "slim")
                {
                    return SkinType.SLIM;
                }
                else
                {
                    return SkinType.NORMAL;
                }
            }
        }
    }
}
