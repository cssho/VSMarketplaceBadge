using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VSMarketplaceBadge.Models;

namespace VSMarketplaceBadge
{
    public static class Utility
    {
        private static readonly string apiKey = ConfigurationManager.AppSettings.Get("LOGGLY_KEY");
        private static readonly HttpClient client = new HttpClient();
        public static RankingViewModel Ranking = new RankingViewModel();
        public static async Task SendAccess(string itemName, BadgeType type)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { Item = itemName, Type = type.ToString() }), Encoding.UTF8, "application/json");
            await client.PostAsync($"https://logs-01.loggly.com/inputs/{apiKey}/tag/access/", content);
        }

        public static async Task SendJob(string eventName)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { Event = eventName }), Encoding.UTF8, "application/json");
            await client.PostAsync($"https://logs-01.loggly.com/inputs/{apiKey}/tag/event/", content);
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
            var content = new StringContent(JsonConvert.SerializeObject(new { Exception = e.ToString(), Request = HttpContext.Current?.Request.Url.PathAndQuery }), Encoding.UTF8, "application/json");
            await client.PostAsync($"https://logs-01.loggly.com/inputs/{apiKey}/tag/exception/", content);
        }
    }
}