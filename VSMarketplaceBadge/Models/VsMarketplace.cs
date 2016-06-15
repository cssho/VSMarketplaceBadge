using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public static class VsMarketplace
    {
        private static readonly Uri marketplaceUri = new Uri("https://marketplace.visualstudio.com/items");
        private static readonly string Query = "itemName";

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
                var builder = CreateUri(itemName);
                html.LoadHtml(await client.GetStringAsync(builder.ToString()));
            }
            return JObject.Parse(html.DocumentNode.QuerySelectorAll(".vss-extension").First().InnerText);
        }

        private static UriBuilder CreateUri(string itemName)
        {
            var builder = new UriBuilder(marketplaceUri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query[Query] = itemName;

            builder.Query = query.ToString();
            return builder;
        }
    }
}