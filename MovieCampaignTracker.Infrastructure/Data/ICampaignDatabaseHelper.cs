//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MovieCampaignTracker.Shared;

//namespace MovieCampaignTracker.Infrastructure.Data
//{
//    public interface ICampaignDatabaseHelper
//    {
//        Task<IEnumerable<Campaigns>> GetAllCampaignsAsync();
//        Task<Campaigns> GetCampaignByIdAsync(int id);
//        Task<int> AddCampaignAsync(Campaigns campaign);

//        Task<IEnumerable<MediaPlatform>> GetAllMediaPlatformsAsync();
//        Task<int> AddMediaPlatformAsync(MediaPlatform platform);

//        Task<IEnumerable<CampaignPlatformPost>> GetAllCampaignPlatformPostsAsync();
//        Task<int> AddCampaignPlatformPostAsync(CampaignPlatformPost post);
//    }

//}
