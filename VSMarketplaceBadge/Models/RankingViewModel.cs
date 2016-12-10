using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VSMarketplaceBadge.Models
{
    public class RankingViewModel
    {
        public RankingViewModel()
        {
            Daily = new List<RankingItemViewModel>();
            Weekly = new List<RankingItemViewModel>();
            Hourly = new List<RankingItemViewModel>();
        }
        public List<RankingItemViewModel> Daily { get; set; }

        public List<RankingItemViewModel> Weekly { get; set; }

        public List<RankingItemViewModel> Hourly { get; set; }
    }
}