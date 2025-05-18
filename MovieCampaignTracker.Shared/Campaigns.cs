using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Shared
{
    public class Campaign
    {
        public int Id { get; set; }
        public int PromotionalElementId { get; set; }
        public int ProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
