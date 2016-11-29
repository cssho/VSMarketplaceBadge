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
        private static readonly string VersionShortSubject = "VS%20Marketplace";
        private static readonly string RatingSubject = "rating";

        [HttpGet]
        [Route("version/{id}.svg")]
        public async Task<HttpResponseMessage> Version(string id)
        {
            return await CreateResponse(id, BadgeType.Version);
        }

        [HttpGet]
        [Route("version-short/{id}.svg")]
        public async Task<HttpResponseMessage> VersionShort(string id)
        {
            return await CreateResponse(id, BadgeType.VersionShort);
        }

        [HttpGet]
        [Route("installs/{id}.svg")]
        public async Task<HttpResponseMessage> Installs(string id)
        {
            return await CreateResponse(id, BadgeType.Installs);
        }

        [HttpGet]
        [Route("installs-short/{id}.svg")]
        public async Task<HttpResponseMessage> InstallsShort(string id)
        {
            return await CreateResponse(id, BadgeType.InstallsShort);
        }

        [HttpGet]
        [Route("rating/{id}.svg")]
        public async Task<HttpResponseMessage> Rating(string id)
        {
            return await CreateResponse(id, BadgeType.Rating);
        }

        [HttpGet]
        [Route("rating-short/{id}.svg")]
        public async Task<HttpResponseMessage> RatingShort(string id)
        {
            return await CreateResponse(id, BadgeType.RatingShort);
        }


        private async Task<HttpResponseMessage> CreateResponse(string itemName, BadgeType type)
        {
            var status = await VsMarketplace.Load(itemName, type);
            Utility.SendMetrics(type).FireAndForget();
            var subejct = SelectSubject(type);
            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.Content = new StringContent(await ShieldsIo.LoadSvg($"https://img.shields.io/badge/{subejct}-{status}-brightgreen.svg"),
                Encoding.UTF8, "image/svg+xml");
            res.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            return res;
        }

        private string SelectSubject(BadgeType type)
        {
            switch (type)
            {
                case BadgeType.Version:
                    return VersionSubject;
                case BadgeType.Installs:
                case BadgeType.InstallsShort:
                    return InstallsSubject;
                case BadgeType.Rating:
                case BadgeType.RatingShort:
                    return RatingSubject;
                case BadgeType.VersionShort:
                    return VersionShortSubject;
                default:
                    return "";
            }
        }
    }
}
