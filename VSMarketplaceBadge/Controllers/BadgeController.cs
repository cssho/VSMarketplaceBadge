using System.Collections.Generic;
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
        private const string DownloadsSubject = "downloads";
        private const string VersionSubject = "Visual%20Studio%20Marketplace";
        private const string VersionShortSubject = "VS%20Marketplace";
        private const string RatingSubject = "rating";
        private const string DefaultColor = "brightgreen";
        private const string DefaultBadgeType = "svg";
        private const string TrendingDailySubject = "trending--daily";
        private const string TrendingWeeklySubject = "trending--weekly";
        private const string TrendingMonthlySubject = "trending--monthly";

        [HttpGet]
        [Route("version/{id}.{ext}")]
        public async Task<HttpResponseMessage> Version(string id, string subject = VersionSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.Version, subject, color, ext);
        }

        [HttpGet]
        [Route("version-short/{id}.{ext}")]
        public async Task<HttpResponseMessage> VersionShort(string id, string subject = VersionShortSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.VersionShort, subject, color, ext);
        }

        [HttpGet]
        [Route("installs/{id}.{ext}")]
        public async Task<HttpResponseMessage> Installs(string id, string subject = InstallsSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.Installs, subject, color, ext);
        }

        [HttpGet]
        [Route("installs-short/{id}.{ext}")]
        public async Task<HttpResponseMessage> InstallsShort(string id, string subject = InstallsSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.InstallsShort, subject, color, ext);
        }

        [HttpGet]
        [Route("downloads/{id}.{ext}")]
        public async Task<HttpResponseMessage> Downloads(string id, string subject = DownloadsSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.Downloads, subject, color, ext);
        }

        [HttpGet]
        [Route("downloads-short/{id}.{ext}")]
        public async Task<HttpResponseMessage> DownloadsShort(string id, string subject = DownloadsSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.DownloadsShort, subject, color, ext);
        }

        [HttpGet]
        [Route("rating/{id}.{ext}")]
        public async Task<HttpResponseMessage> Rating(string id, string subject = RatingSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.Rating, subject, color, ext);
        }

        [HttpGet]
        [Route("rating-short/{id}.{ext}")]
        public async Task<HttpResponseMessage> RatingShort(string id, string subject = RatingSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.RatingShort, subject, color, ext);
        }

        [HttpGet]
        [Route("rating-star/{id}.{ext}")]
        public async Task<HttpResponseMessage> RatingStar(string id, string subject = RatingSubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.RatingStar, subject, color, ext);
        }

        [HttpGet]
        [Route("ranking")]
        public RankingViewModel Ranking()
        {
            return Loggly.Ranking;
        }

        [HttpGet]
        [Route("trending-daily/{id}.{ext}")]
        public async Task<HttpResponseMessage> TrendingDaily(string id, string subject = TrendingDailySubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.TrendingDaily, subject, color, ext);
        }

        [HttpGet]
        [Route("trending-weekly/{id}.{ext}")]
        public async Task<HttpResponseMessage> TrendingWeekly(string id, string subject = TrendingWeeklySubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.TrendingWeekly, subject, color, ext);
        }

        [HttpGet]
        [Route("trending-monthly/{id}.{ext}")]
        public async Task<HttpResponseMessage> TrendingMonthly(string id, string subject = TrendingMonthlySubject, string color = DefaultColor, string ext = DefaultBadgeType)
        {
            return await CreateResponse(id, BadgeType.TrendingMonthly, subject, color, ext);
        }

        private async Task<HttpResponseMessage> CreateResponse(string itemName, BadgeType type, string subject, string color, string ext)
        {
            Loggly.SendAccess(itemName, type, subject, color, ext).FireAndForget();
            var status = await VsMarketplace.Load(itemName, type);
            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.Content = new ByteArrayContent(await ShieldsIo.LoadSvg(subject, status, color, Request.RequestUri.Query, ext));
            res.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeMap[ext]);
            res.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            return res;
        }

        private static readonly Dictionary<string, string> mimeMap = new Dictionary<string, string>()
        {
            {"svg","image/svg+xml" },{"png","image/png" },{"gif","image/gif" }
        };
    }
}
