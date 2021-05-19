using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public class MicrosoftSkin : ISkin
    {
        public string Id { get; set; }

        public string State { get; set; }

        public string Url { get; set; }

        public string Variant { get; set; }

        public string Alias { get; set; }

        public SkinType Type
        {
            get
            {
                if (Variant == "CLASSIC")
                {
                    return SkinType.NORMAL;
                }
                else
                {
                    return SkinType.SLIM;
                }
            }
        }

        public string Name { get => Alias; }
    }
}
