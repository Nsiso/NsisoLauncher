using NsisoLauncherCore.Net.Mirrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Tools
{
    public static class MirrorHelper
    {
        public static async Task<IMirror> ChooseBestMirror(IEnumerable<IMirror> mirrors)
        {
            Ping ping = new Ping();
            long currentLowestPing = 0;
            IMirror currentLowestMirror = null;
            if (mirrors.Count() == 1)
            {
                return mirrors.First();
            }
            foreach (var item in mirrors)
            {
                var result = await ping.SendPingAsync(item.BaseDomain,1000);
                if (currentLowestPing <= 0)
                {
                    currentLowestPing = result.RoundtripTime;
                    currentLowestMirror = item;
                }
                else
                {
                    if (result.RoundtripTime < currentLowestPing)
                    {
                        currentLowestMirror = item;
                        currentLowestPing = result.RoundtripTime;
                    }
                }
            }
            return currentLowestMirror;
        }
    }
}
