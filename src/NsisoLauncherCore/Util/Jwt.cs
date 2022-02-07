using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Text;

namespace NsisoLauncherCore.Util
{
    public static class Jwt
    {
        public static bool TryParse(string token_str, out JwtSecurityToken token)
        {
            try
            {
                token = new JwtSecurityToken(token_str);
                return true;
            }
            catch (Exception)
            {
                token = null;
                return false;
            }
        }

        public static bool ValidateExp(JwtSecurityToken securityToken)
        {
            try
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                int time_stamp = Convert.ToInt32(ts.TotalSeconds);
                int? exp = securityToken.Payload.Exp;
                return exp != null && time_stamp < exp;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
