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
        private const string InstallsSubject = "installs";
        private const string VersionSubject = "Visual%20Studio%20Marketplace";
        private const string VersionShortSubject = "VS%20Marketplace";
        private const string RatingSubject = "rating";
        private const string DefaultColor = "brightgreen";
        private const string TrendingDailySubject = "trending--daily";
        private const string TrendingWeeklySubject = "trending--weekly";
        private const string TrendingMonthlySubject = "trending--monthly";

        [HttpGet]
        [Route("version/{id}.svg")]
        public async Task<HttpResponseMessage> Version(string id, string subject = VersionSubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.Version, subject, color);
        }

        [HttpGet]
        [Route("version-short/{id}.svg")]
        public async Task<HttpResponseMessage> VersionShort(string id, string subject = VersionShortSubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.VersionShort, subject, color);
        }

        [HttpGet]
        [Route("installs/{id}.svg")]
        public async Task<HttpResponseMessage> Installs(string id, string subject = InstallsSubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.Installs, subject, color);
        }

        [HttpGet]
        [Route("installs-short/{id}.svg")]
        public async Task<HttpResponseMessage> InstallsShort(string id, string subject = InstallsSubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.InstallsShort, subject, color);
        }

        [HttpGet]
        [Route("rating/{id}.svg")]
        public async Task<HttpResponseMessage> Rating(string id, string subject = RatingSubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.Rating, subject, color);
        }

        [HttpGet]
        [Route("rating-short/{id}.svg")]
        public async Task<HttpResponseMessage> RatingShort(string id, string subject = RatingSubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.RatingShort, subject, color);
        }

        [HttpGet]
        [Route("ranking")]
        public RankingViewModel Ranking()
        {
            return Loggly.Ranking;
        }

        [HttpGet]
        [Route("trending-daily/{id}.svg")]
        public async Task<HttpResponseMessage> TrendingDaily(string id, string subject = TrendingDailySubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.TrendingDaily, subject, color);
        }

        [HttpGet]
        [Route("trending-weekly/{id}.svg")]
        public async Task<HttpResponseMessage> TrendingWeekly(string id, string subject = TrendingWeeklySubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.TrendingWeekly, subject, color);
        }

        [HttpGet]
        [Route("trending-monthly/{id}.svg")]
        public async Task<HttpResponseMessage> TrendingMonthly(string id, string subject = TrendingMonthlySubject, string color = DefaultColor)
        {
            return await CreateResponse(id, BadgeType.TrendingMonthly, subject, color);
        }

        private async Task<HttpResponseMessage> CreateResponse(string itemName, BadgeType type, string subject, string color)
        {
            var status = await VsMarketplace.Load(itemName, type);
            Loggly.SendAccess(itemName, type, subject, color).FireAndForget();
            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.Content = new StringContent(await ShieldsIo.LoadSvg(subject, status, color, Request.RequestUri.Query),
                Encoding.UTF8, "image/svg+xml");
            res.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            return res;
        }
    }
}
