using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(
                    OnRemoteCertificateValidationCallback);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VSMarketplaceBadge", "1.0"));
        }
        
        private static bool OnRemoteCertificateValidationCallback(
            Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors) => true;

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
