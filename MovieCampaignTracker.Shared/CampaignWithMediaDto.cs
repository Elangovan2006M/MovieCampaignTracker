using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCampaignTracker.Shared
{
    public class CampaignWithMediaDto
    {
        public Campaign Campaign { get; set; }
        public List<MediaPlatform> MediaPlatforms { get; set; } = new();
    }
}

