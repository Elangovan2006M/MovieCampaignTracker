using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCampaignTracker.Shared
{
    public class SocialMediaPage
    {
        public int Id { get; set; }
        public string Platform { get; set; }
        public string PageUrl { get; set; }
        public string PageName { get; set; }
        public long FollowersCount { get; set; }
        public string AdminName { get; set; }
        public string AdminMobile { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }

    }
}
