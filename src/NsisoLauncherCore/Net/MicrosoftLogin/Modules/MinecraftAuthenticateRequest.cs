using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NsisoLauncherCore.Net.MicrosoftLogin.Modules
{
    public class MinecraftAuthenticateRequest
    {
        [JsonProperty("identityToken")]
        public string IdentityToken { get; set; }
    }

    public class MinecraftToken
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class Ownership
    {
        [JsonProperty("items")]
        public List<OwnershipItem> Items { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("keyId")]
        public string KeyId { get; set; }
    }

    public class OwnershipItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
