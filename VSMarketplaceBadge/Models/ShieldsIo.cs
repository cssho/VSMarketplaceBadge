using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public static class ShieldsIo
    {
        public static async Task<string> LoadSvg(string uri)
        {
            using(var client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }
    }
}