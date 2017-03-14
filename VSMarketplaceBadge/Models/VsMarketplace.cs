using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public static class VsMarketplace
    {
        private static readonly Uri marketplaceUri = new Uri("https://marketplace.visualstudio.com/");
        private static readonly Uri marketplaceItemUri = new Uri(marketplaceUri, "items");
        private static readonly string itemQuery = "itemName";
        private static readonly string[] units = { "", "K", "M", "G" };
        private static readonly HttpClient client = new HttpClient();

        static VsMarketplace()
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VSMarketplaceBadge", "1.0"));
        }

        public static async Task<string> Load(string itemName, BadgeType type)
        {
            var json = await LoadVssItemData(itemName);
            switch (type)
            {
                case BadgeType.Version:
                case BadgeType.VersionShort:
                    return LoadVersion(json);
                case BadgeType.Installs:
                    return LoadInstalls(json);
                case BadgeType.InstallsShort:
                    return LoadInstalls(json, true);
                case BadgeType.Rating:
                    return LoadRating(json);
                case BadgeType.RatingShort:
                    return LoadRating(json, true);
                case BadgeType.TrendingDaily:
                    return LoadTrending(json, "trendingdaily");
                case BadgeType.TrendingMonthly:
                    return LoadTrending(json, "trendingmonthly");
                case BadgeType.TrendingWeekly:
                    return LoadTrending(json, "trendingweekly");
                default:
                    throw new ArgumentException();
            }
        }

        private static string LoadRating(JObject json, bool isShort = false)
        {
            var average = json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "averagerating")?.Value<double>("value");
            var count = json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "ratingcount")?.Value<int>("value");
            return isShort
                ? $"{Math.Round(average ?? 0, 2)}/5 ({count ?? 0})"
                : $"average: {Math.Round(average ?? 0, 2)}/5 ({count ?? 0} ratings)";
        }

        private static string LoadTrending(JObject json, string term)
        {
            var trend = json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == term)?.Value<double>("value");
            return $"{Math.Round(trend ?? 0, 2)}";
        }

        private static string LoadVersion(JObject json)
            => $"v{(string)json["versions"].Max(x => x["version"])}";


        private static string LoadInstalls(JObject json, bool isShort = false)
        {
            if (isShort)
            {
                var installs = (double)CountInstalls(json);
                return ApplyUnit(installs);
            }
            else
            {
                return CountInstalls(json).ToString();
            }
        }

        private static long CountInstalls(JObject json)
        {
            var installs = (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "install")["value"] ?? 0);
            installs += (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "migratedInstallCount")?["value"] ?? 0);
            return installs;
        }

        private static string ApplyUnit(double installs, int unitIdx = 0)
        {
            if (installs < 1000 || unitIdx == units.Length) return installs.ToString() + units[unitIdx];
            return ApplyUnit(Math.Round(installs / 1000, 2, MidpointRounding.AwayFromZero), unitIdx + 1);
        }

        private static async Task<JObject> LoadVssItemData(string itemName)
        {
            var html = new HtmlDocument();
            var builder = CreateItemUri(itemName);
            html.LoadHtml(await client.GetStringAsync(builder.ToString()));

            return JObject.Parse(html.DocumentNode.QuerySelectorAll(".vss-extension").First().InnerText);
        }

        private static UriBuilder CreateItemUri(string itemName)
        {
            return CreateUri(new Dictionary<string, string>() { { itemQuery, itemName } });
        }

        private static UriBuilder CreateUri(Dictionary<string, string> param)
        {
            var builder = new UriBuilder(marketplaceItemUri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var kv in param)
            {
                query[kv.Key] = kv.Value;
            }

            builder.Query = query.ToString();
            return builder;
        }
    }
}