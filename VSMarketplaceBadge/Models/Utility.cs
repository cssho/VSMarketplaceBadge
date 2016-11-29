using Graphite.StatsD;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public static class Utility
    {
        private static readonly string apiKey = ConfigurationManager.AppSettings.Get("HOSTEDGRAPHITE_APIKEY");
        public static async Task SendMetrics(BadgeType type)
        {
            await Task.Run(() =>
            {
                using (var client = new StatsDClient("statsd.hostedgraphite.com",
                port: 8125, keyPrefix: apiKey))
                {
                    client.Increment("access");
                    client.Increment($"access.{type.ToString()}");
                }
            });
        }

        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
            {
                Trace.TraceError(x.Exception.ToString());
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}