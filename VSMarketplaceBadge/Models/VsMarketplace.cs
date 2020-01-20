using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public static class VsMarketplace
    {
        private static readonly Uri marketplaceApiUri = new Uri("https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery");
        private static readonly string[] units = { "", "K", "M", "G" };
        private static readonly HttpClient client = new HttpClient();

        static VsMarketplace()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client = new HttpClient(httpClientHandler)
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VSMarketplaceBadge", "1.0"));
            client.DefaultRequestHeaders.Add("Accept", "application/json;api-version=3.0-preview.1");
        }

        public static async Task<string> Load(string itemName, BadgeType type)
        {
            //var json = await LoadVssItemData(itemName);
            var json = await LoadVssItemDataFromApi(itemName);
            if (json == null) return null;
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
                case BadgeType.RatingStar:
                    return LoadRatingStar(json);
                case BadgeType.Downloads:
                    return LoadDownloads(json);
                case BadgeType.DownloadsShort:
                    return LoadDownloads(json, true);
                default:
                    throw new ArgumentException();
            }
        }

        private static readonly double[] FractionBoundaryValues = new[] { 7.0 / 8.0, 5.0 / 8.0, 3.0 / 8.0, 1.0 / 8.0 };

        private static string LoadRatingStar(JObject json)
        {
            var average = json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "averagerating")?.Value<double>("value");
            if (!average.HasValue) return "☆☆☆☆☆";
            var floored = Math.Floor(average.Value);
            var fraction = average.Value - floored;

            var stars = "";
            while (stars.Length < floored) stars += '★';

            stars += fraction >= FractionBoundaryValues[0] ? "★"
                : fraction >= FractionBoundaryValues[1] ? "¾"
                : fraction >= FractionBoundaryValues[2] ? "½"
                : fraction >= FractionBoundaryValues[3] ? "¼"
                : "";

            while (stars.Length < 5) stars += '☆';
            return stars;
        }

        private static async Task<JObject> LoadVssItemDataFromApi(string itemName)
        {
            var req = JsonConvert.SerializeObject(new { filters = new[] { new { criteria = new[] { new { filterType = 7, value = itemName }, new { filterType = 12, value = "4096" } } } }, flags = 914 });
            var result = await client.PostAsync(marketplaceApiUri, new StringContent(req, Encoding.UTF8, "application/json"));
            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                try
                {
                    var extensions = JObject.Parse(response)["results"][0]["extensions"];
                    if (extensions.Count() != 1) return null;
                    return (JObject)extensions[0];
                }
                catch (InvalidOperationException ex)
                {
                    ex.Data.Add("json", response);
                    throw ex;
                }

            }
            var e = new InvalidCastException("Invalid extension data");
            e.Data.Add("req", req.ToString());
            e.Data.Add("res", await result.Content.ReadAsStringAsync());
            throw e;

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
            => isShort ? ApplyUnit(CountInstalls(json)) : CountInstalls(json).ToString();
        private static string LoadDownloads(JObject json, bool isShort = false)
            => isShort ? ApplyUnit(CountDownloads(json)) : CountDownloads(json).ToString();

        private static long CountInstalls(JObject json)
            => (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "install")?["value"] ?? 0)
                + (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "migratedInstallCount")?["value"] ?? 0);

        private static long CountDownloads(JObject json)
        {
            var installs = (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "install")?["value"] ?? 0);
            installs += (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "updateCount")?["value"] ?? 0);
            installs += (long)(json["statistics"]?.FirstOrDefault(x => (string)x["statisticName"] == "migratedInstallCount")?["value"] ?? 0);
            return installs;
        }

        private static string ApplyUnit(double installs, int unitIdx = 0)
        {
            if (installs < 1000 || unitIdx == units.Length) return installs.ToString() + units[unitIdx];
            return ApplyUnit(Math.Round(installs / 1000, 2, MidpointRounding.AwayFromZero), unitIdx + 1);
        }
    }
}
