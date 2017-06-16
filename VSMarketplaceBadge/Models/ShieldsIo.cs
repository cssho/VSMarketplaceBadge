using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VSMarketplaceBadge.Models
{
    public static class ShieldsIo
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<string> LoadSvg(string subject, string status, string color, string query)
        {
            if (status == null)
            {
                status = "unknown";
                color = "lightgrey";
            }
            return await client.GetStringAsync($"https://img.shields.io/badge/{subject}-{status}-{color}.svg" + query);
        }
    }
}