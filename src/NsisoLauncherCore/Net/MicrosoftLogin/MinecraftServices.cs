using NsisoLauncherCore.Net.MicrosoftLogin.Modules;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.User;

namespace NsisoLauncherCore.Net.MicrosoftLogin
{
    public class MinecraftServices
    {
        public Uri MinecraftServicesUri { get; set; } = new Uri("https://api.minecraftservices.com/");
        public Uri AuthenticateUri { get; set; } = new Uri("https://api.minecraftservices.com/authentication/login_with_xbox");
        public Uri CheckGameOwnershipUri { get; set; } = new Uri("https://api.minecraftservices.com/entitlements/mcstore");
        public Uri ProfileUri { get; set; } = new Uri("https://api.minecraftservices.com/minecraft/profile");

        public async Task<MinecraftToken> Authenticate(XboxLiveToken token, CancellationToken cancellation)
        {
            MinecraftAuthenticateRequest request = new MinecraftAuthenticateRequest()
            {
                IdentityToken = string.Format("XBL3.0 x={0};{1}", token.Uhs, token.XstsToken)
            };
            string json_str = JsonConvert.SerializeObject(request);

            HttpContent content = new StringContent(json_str);
            content.Headers.ContentType.MediaType = "application/json";

            var result = await NetRequester.HttpPostAsync(AuthenticateUri, content, cancellation);
            result.EnsureSuccessStatusCode();

            var respond_str = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MinecraftToken>(respond_str);
        }

        public async Task<bool> CheckHaveGameOwnership(MinecraftToken token, CancellationToken cancellation)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, CheckGameOwnershipUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var result = await NetRequester.HttpSendAsync(request, cancellation);
            result.EnsureSuccessStatusCode();

            var respond_str = await result.Content.ReadAsStringAsync();
            Ownership owner = JsonConvert.DeserializeObject<Ownership>(respond_str);

            if (owner == null || owner.Items == null || owner.Items.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<MicrosoftUser> GetProfile(string ms_token, MinecraftToken mc_token, CancellationToken cancellation)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ProfileUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", mc_token.AccessToken);
            var result = await NetRequester.HttpSendAsync(request, cancellation);
            result.EnsureSuccessStatusCode();

            var respond_str = await result.Content.ReadAsStringAsync();
            Console.WriteLine(respond_str);
            MicrosoftUser profile = JsonConvert.DeserializeObject<MicrosoftUser>(respond_str);
            profile.MinecraftToken = mc_token;

            return profile;
        }
    }
}
