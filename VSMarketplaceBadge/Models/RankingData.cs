using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public class RankingData
    {
        public List<RankingItem> Item { get; set; }
        public int TotalEvents { get; set; }
        public int UniqueFieldCount { get; set; }
    }
}