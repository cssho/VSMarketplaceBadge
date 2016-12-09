using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VSMarketplaceBadge.Models;

namespace VSMarketplaceBadge
{
    public static class Utility
    {
        private static readonly string apiKey = ConfigurationManager.AppSettings.Get("LOGGLY_KEY");
        private static readonly HttpClient client = new HttpClient();
        public static async Task SendMetrics(string itemName, BadgeType type)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { Item = itemName, Type = type.ToString() }), Encoding.UTF8, "application/json");
            await client.PostAsync($"https://logs-01.loggly.com/inputs/762645d2-2ee6-4a2d-be18-4b7be6373cb8/tag/http/", content);
        }

        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
            {
                SendError(x.Exception).Wait();
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static async Task SendError(Exception e)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { Exception = e.ToString() }), Encoding.UTF8, "application/json");
            await client.PostAsync($"https://logs-01.loggly.com/inputs/762645d2-2ee6-4a2d-be18-4b7be6373cb8/tag/http/", content);
        }
    }
}