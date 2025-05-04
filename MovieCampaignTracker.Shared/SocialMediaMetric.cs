using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCampaignTracker.Shared
{
    public class SocialMediaMetric
    {
        public string MovieName { get; set; }
        public string Platform { get; set; }
        public string TitleOrText { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public long CommentCount { get; set; }
        public long ShareCount { get; set; }
        public DateTime FetchedAt { get; set; }
    }

}
