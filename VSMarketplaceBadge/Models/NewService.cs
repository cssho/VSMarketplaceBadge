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
    public static class NewService
    {
        private static readonly HttpClient client = new HttpClient();

        static NewService()
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VSMarketplaceBadge", "1.0"));
        }

        public static async Task<HttpResponseMessage> Relay(Uri oldUri)
        {
            var newUriBuilder = new UriBuilder(oldUri);
            newUriBuilder.Host = "vsmarketplacebadges.dev";
            newUriBuilder.Port = 443;
            newUriBuilder.Scheme = "https";
            return await client.GetAsync(newUriBuilder.Uri);
        }
    }
}
