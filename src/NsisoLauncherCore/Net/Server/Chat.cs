using Newtonsoft.Json;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Server
{
    [JsonConverter(typeof(ServerDescriptionJsonConverter))]
    public class Chat
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        // This code is useless in this app

        //[JsonProperty("bold")]
        //public bool Bold { get; set; }

        //[JsonProperty("italic")]
        //public bool Italic { get; set; }

        //[JsonProperty("underlined")]
        //public bool Underlined { get; set; }

        //[JsonProperty("strikethrough")]
        //public bool Strikethrough { get; set; }

        //[JsonProperty("obfuscated")]
        //public bool Obfuscated { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("extra")]
        public List<Chat> Extra { get; set; }

        public string ToPlainTextString()
        {
            StringBuilder stringBuilder = new StringBuilder(Text);
            if (Extra != null)
            {
                foreach (var item in Extra)
                {
                    stringBuilder.Append(item.ToPlainTextString());
                }
            }
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return CleanFormat(ToPlainTextString());
        }

        private string CleanFormat(string str)
        {
            str = str.Replace(@"\n", "\n");

            int index;
            do
            {
                index = str.IndexOf('§');
                if (index >= 0)
                {
                    str = str.Remove(index, 2);
                }
            } while (index >= 0);

            return str.Trim();
        }
    }
}
