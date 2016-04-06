using System.Net.Http;
using System.Threading.Tasks;

namespace VSMarketplaceBadge.Models
{
    public static class ShieldsIo
    {
        public static async Task<string> LoadSvg(string uri)
        {
            using(var client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }
    }
}