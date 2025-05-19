using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCampaignTracker.Shared
{
    public class PromotionalElement
    {
        public int Id { get; set; }
        public string PromotionElement { get; set; }
        public int ProjectNameId { get; set; }
    }

}
