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
            switch (type)
            {
                case BadgeType.Version:
                case BadgeType.VersionShort:
                    return await LoadVersion(itemName);
                case BadgeType.Installs:
                    return await LoadInstalls(itemName);
                case BadgeType.InstallsShort:
                    return await LoadInstalls(itemName, true);
                case BadgeType.Rating:
                    return await LoadRating(itemName);
                case BadgeType.RatingShort:
                    return await LoadRating(itemName, true);
                default:
                    throw new ArgumentException();
            }
        }

        private static async Task<string> LoadRating(string itemName, bool isShort = false)
        {
            var json = await LoadVssItemData(itemName);
            var average = json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "averagerating")?.Value<double>("value");
            var count = json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "ratingcount")?.Value<int>("value");
            return isShort
                ? $"{Math.Round(average ?? 0, 2)}/5 ({count ?? 0})"
                : $"average: {Math.Round(average ?? 0, 2)}/5 ({count ?? 0} ratings)";
        }

        private static async Task<string> LoadVersion(string itemName)
        {
            var json = await LoadVssItemData(itemName);
            return $"v{(string)json["versions"].Max(x => x["version"])}";
        }

        private static async Task<string> LoadInstalls(string itemName, bool isShort = false)
        {
            var json = await LoadVssItemData(itemName);
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