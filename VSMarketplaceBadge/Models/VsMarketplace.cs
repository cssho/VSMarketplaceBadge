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

        public static async Task<string> Load(string itemName,BadgeType type)
        {
            switch (type)
            {
                case BadgeType.Version:
                    return await LoadVersion(itemName);
                case BadgeType.Installs:
                    return await LoadInstalls(itemName);
                default:
                    throw new ArgumentException();
            }
        }

        private static async Task<string> LoadVersion(string itemName)
        {
            var json = await LoadVssItemData(itemName);
            return (string)json["versions"].Max(x => x["version"]);
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