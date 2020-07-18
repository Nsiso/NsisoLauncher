using System;
using System.Threading.Tasks;
using NsisoLauncherCore.Net.MojangApi.Api;
using static NsisoLauncherCore.Net.MojangApi.Responses.AuthenticateResponse;

namespace NsisoLauncherCore.Auth
{
    public class OfflineAuthenticator : IAuthenticator
    {
        public OfflineAuthenticator(string displayname)
        {
            Displayname = displayname;
            ProfileUUID = new Uuid
            {
                PlayerName = Displayname,
                Value = Guid.NewGuid().ToString("N")
            };
            UserData = new UserData
            {
                ID = Guid.NewGuid().ToString("N")
            };
        }

        public OfflineAuthenticator(string displayname, UserData userData, string profileUUID)
        {
            Displayname = displayname;
            UserData = userData;
            ProfileUUID = new Uuid
            {
                PlayerName = Displayname,
                Value = profileUUID
            };
        }

        public string Displayname { get; set; }
        public UserData UserData { get; set; }
        public Uuid ProfileUUID { get; set; }

        public async Task<AuthenticateResult> DoAuthenticateAsync()
        {
            return await Task.Factory.StartNew(() => { return DoAuthenticate(); });
        }

        public AuthenticateResult DoAuthenticate()
        {
            var accessToken = Guid.NewGuid().ToString("N");

            return new AuthenticateResult(AuthState.SUCCESS)
                {AccessToken = accessToken, SelectedProfileUUID = ProfileUUID, UserData = UserData};
        }
    }
}