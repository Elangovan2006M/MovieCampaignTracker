using MovieCampaignTracker.Infrastructure.Data;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Service.Services
{
    public class TwitterService
    {
        private readonly DatabaseHelper _db;

        public TwitterService(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task SaveTwitterMetricsAsync(TwitterMetrics metrics)
        {
            await _db.SaveTwitterMetricsAsync(metrics);
        }

        public async Task<IEnumerable<TwitterMetrics>> GetAllTwitterMetricsAsync()
        {
            return await _db.GetAllTwitterMetricsAsync();
        }
    }

}
