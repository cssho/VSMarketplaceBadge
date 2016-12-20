using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VSMarketplaceBadge.Models;

namespace VSMarketplaceBadge
{
    public class GetRankingModule : IHttpModule
    {
        private static int initializedModuleCount;
        private static Timer timer;
        private static readonly string LogglyId = ConfigurationManager.AppSettings.Get("LOGGLY_ID");
        private static readonly string LogglyPw = ConfigurationManager.AppSettings.Get("LOGGLY_PW");
        private static readonly string VSMarketplaceBaseUrl = "https://marketplace.visualstudio.com/items?itemName=";
        private static readonly HttpClient Client = new HttpClient();

        public void Init(HttpApplication context)
        {
            var count = Interlocked.Increment(ref initializedModuleCount);
            if (count != 1) return;

            timer = new Timer(_ =>
            {
                try
                {
                    if (LogglyId != null && LogglyPw != null)
                    {
                        var basicParam = Encoding.ASCII.GetBytes($"{LogglyId}:{LogglyPw}");
                        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(basicParam));
                        Loggly.Ranking.Hourly = GetRankingData("h");
                        Loggly.Ranking.Daily = GetRankingData("d");
                        Loggly.Ranking.Weekly = GetRankingData("w");
                        Loggly.SendJob(nameof(GetRankingModule)).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Loggly.SendError(ex).FireAndForget();
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        }

        private List<RankingItemViewModel> GetRankingData(string fromQuery)
        {
            var result = Client.GetStringAsync($"http://{LogglyId}.loggly.com/apiv2/fields/json.Item/?q=*&from=-1{fromQuery}&until=now&facet_size=5").Result;
            var data = JsonConvert.DeserializeObject<RankingData>(result.Replace("json.Item", "Item"));
            return data.Item.Select(x => new RankingItemViewModel() { ItemName = $"{x.Term} ({x.Count})", Url = VSMarketplaceBaseUrl + x.Term }).ToList();
        }


        public void Dispose()
        {
            var count = Interlocked.Decrement(ref initializedModuleCount);
            if (count == 0)
            {
                var target = Interlocked.Exchange(ref timer, null);
                if (target != null)
                {
                    target.Dispose();
                }
            }
        }
    }
}