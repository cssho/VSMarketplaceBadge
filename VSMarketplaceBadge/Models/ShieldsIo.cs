using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VSMarketplaceBadge.Models
{
    public static class ShieldsIo
    {
        private static readonly HttpClient client = new HttpClient();

        static ShieldsIo()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public static async Task<byte[]> LoadSvg(string subject, string status, string color, string query, string ext)
        {
            if (status == null)
            {
                status = "unknown";
                color = "lightgrey";
            }

            var key = string.Join(":",new[]{ subject,status,color,ext,query}.Where(x=>!string.IsNullOrEmpty(x)));

            var image = await RedisClient.GetImage(key) ?? await client.GetByteArrayAsync($"https://img.shields.io/badge/{subject}-{status}-{color}.{ext}" + query);
            RedisClient.SetImage(key, image);
            return image;
        }
    }
}