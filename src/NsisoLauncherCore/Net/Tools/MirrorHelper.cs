using NsisoLauncherCore.Net.Mirrors;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Net.Tools
{
    public static class MirrorHelper
    {
        public static async Task<IMirror> ChooseBestMirror(IEnumerable<IMirror> mirrors)
        {
            if (mirrors == null)
            {
                return null;
            }
            int count = mirrors.Count();
            if (count == 0)
            {
                return null;
            }
            if (count == 1)
            {
                return mirrors.First();
            }
            using (Ping ping = new Ping())
            {
                long currentLowestPing = 0;
                IMirror currentLowestMirror = null;

                foreach (var item in mirrors)
                {
                    var result = await ping.SendPingAsync(item.MCDownloadUri.Host, 1000);
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
}
