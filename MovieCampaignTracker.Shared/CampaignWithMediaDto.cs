using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCampaignTracker.Shared
{
    public class CampaignWithMediaDto
    {
        public Campaigns Campaigns { get; set; }
        public List<MediaPlatforms> MediaPlatforms { get; set; } = new();
    }
}

