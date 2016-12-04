using System.Net.Http;
using System.Threading.Tasks;

namespace VSMarketplaceBadge.Models
{
    public static class ShieldsIo
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<string> LoadSvg(string uri) => await client.GetStringAsync(uri);
    }
}