using MovieCampaignTracker.Infrastructure.Data;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Service.Services
{
    public class YouTubeService
    {
        private readonly DatabaseHelper _db;

        public YouTubeService(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task SaveYouTubeMetricsAsync(YouTubeMetrics metrics)
        {
            await _db.SaveYouTubeMetricsAsync(metrics);
        }

        public async Task<IEnumerable<YouTubeMetrics>> GetAllYouTubeMetricsAsync()
        {
            return await _db.GetAllYouTubeMetricsAsync();
        }
    }

}
