using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public static class VsMarketplace
    {
        private static readonly Uri marketplaceUri = new Uri("https://marketplace.visualstudio.com/");
        private static readonly Uri marketplaceItemUri = new Uri(marketplaceUri, "items");
        private static readonly string itemQuery = "itemName";

        public static async Task<string> Load(string itemName, BadgeType type)
        {
            switch (type)
            {
                case BadgeType.Version:
                case BadgeType.VersionShort:
                    return await LoadVersion(itemName);
                case BadgeType.Installs:
                    return await LoadInstalls(itemName);
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
            var average = json["statistics"].FirstOrDefault(x => (string)x["statisticName"] == "averagerating").Value<double>("value");
            var count = json["statistics"].FirstOrDefault(x => (string)x["statisticName"] == "ratingcount").Value<int>("value");
            return isShort
                ? $"{Math.Round(average, 2)}/5 ({count})"
                : $"average: {Math.Round(average, 2)}/5 ({count} ratings)";
        }

        private static async Task<string> LoadVersion(string itemName)
        {
            var json = await LoadVssItemData(itemName);
            return $"v{(string)json["versions"].Max(x => x["version"])}";
        }

        private static async Task<string> LoadInstalls(string itemName)
        {
            var json = await LoadVssItemData(itemName);
            return (string)json["statistics"].FirstOrDefault(x => (string)x["statisticName"] == "install")["value"];
        }

        private static async Task<JObject> LoadVssItemData(string itemName)
        {
            var html = new HtmlDocument();
            using (var client = new HttpClient())
            {
                var builder = CreateItemUri(itemName);
                html.LoadHtml(await client.GetStringAsync(builder.ToString()));
            }
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