using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.User;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.MojangAPI
{
    public class APIHandler
    {
        public string BaseUrl { get; set; } = "https://api.mojang.com";

        public string SessionUrl { get; set; } = "https://sessionserver.mojang.com/session/minecraft";

        public string SkinUrl { get => SessionUrl + "/session/minecraft/profile"; }

        public async Task<YggdrasilSkin> GetSkins(YggdrasilUser user, PlayerProfile profile)
        {

            string url = string.Format("{0}/{1}", SkinUrl, profile.Id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.GameAccessToken);
            var result = await NetRequester.HttpSendAsync(request);
            result.EnsureSuccessStatusCode();
            string json_str = await result.Content.ReadAsStringAsync();
            var session_profile = JsonConvert.DeserializeObject<APIModules.SessionProfile>(json_str);
            var texture_prop = session_profile.Properties.Find((x) => x.Name == "textures");
            if (texture_prop == null)
            {
                return null;
            }
            else
            {
                byte[] txt_byte = Convert.FromBase64String(texture_prop.Value);
                string txt_str = Encoding.Default.GetString(txt_byte);
                APIModules.Textures textures = JsonConvert.DeserializeObject<APIModules.Textures>(txt_str);

                if (textures.TexturesObjs.ContainsKey("SKIN"))
                {
                    JObject skin_jobj = textures.TexturesObjs["SKIN"].ToObject<JObject>();
                    string skin_url = skin_jobj["url"].ToString();

                    YggdrasilSkin skin = new YggdrasilSkin();
                    skin.Url = skin_url;
                    skin.Name = textures.ProfileName;
                    if (skin_jobj.ContainsKey("metadata"))
                    {
                        skin.Metadata = skin_jobj["metadata"].ToObject<JObject>();
                    }

                    return skin;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
