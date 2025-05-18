using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCampaignTracker.Shared
{
    public class MediaPlatform
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string PlatformName { get; set; }
        public int NumberOfPosts { get; set; }
    }

}
