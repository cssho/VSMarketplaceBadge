using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VSMarketplaceBadge.Models
{
    public static class ShieldsIo
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<byte[]> LoadSvg(string subject, string status, string color, string query,string ext)
        {
            if (status == null)
            {
                status = "unknown";
                color = "lightgrey";
            }
            return await client.GetByteArrayAsync($"https://img.shields.io/badge/{subject}-{status}-{color}.{ext}" + query);
        }
    }
}