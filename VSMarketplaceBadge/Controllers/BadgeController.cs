using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using VSMarketplaceBadge.Models;

namespace VSMarketplaceBadge.Controllers
{
    public class BadgeController : ApiController
    {
        private static readonly string InstallsSubject = "installs";
        private static readonly string VersionSubject = "Visual%20Studio%20Marketplace";
        [HttpGet]
        [Route("version/{id}.svg")]
        public async Task<HttpResponseMessage> Version(string id)
        {
            return await CreateResponse(id, BadgeType.Version);
        }

        [HttpGet]
        [Route("installs/{id}.svg")]
        public async Task<HttpResponseMessage> Installs(string id)
        {
            return await CreateResponse(id, BadgeType.Installs);
        }

        private async Task<HttpResponseMessage> CreateResponse(string itemName, BadgeType type)
        {
            var status = await VsMarketplace.Load(itemName, type);
            var subejct = type == BadgeType.Version ? VersionSubject : InstallsSubject;
            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.Content = new StringContent(await ShieldsIo.LoadSvg($"https://img.shields.io/badge/{subejct}-{status}-brightgreen.svg"),
                Encoding.UTF8, "image/svg+xml");
            res.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            return res;
        }
    }
}
