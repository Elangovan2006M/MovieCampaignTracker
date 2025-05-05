using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Infrastructure.Data
{
    public interface IDatabaseHelper
    {
        Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(string storedProcedure, DynamicParameters parameters);
        Task<int> ExecuteStoredProcedureAsync(string storedProcedure, DynamicParameters parameters);
        Task SaveTwitterMetricsAsync(TwitterMetrics metrics);
        Task SaveYouTubeMetricsAsync(YouTubeMetrics metrics);
        Task<IEnumerable<TwitterMetrics>> GetAllTwitterMetricsAsync();
        Task<IEnumerable<YouTubeMetrics>> GetAllYouTubeMetricsAsync();

        Task<IEnumerable<SocialMediaPage>> GetAllSocialMediaPagesAsync();
        Task<SocialMediaPage> GetPageByIdAsync(int id);
        Task AddPageAsync(SocialMediaPage page);
        Task UpdatePageAsync(SocialMediaPage page);
        Task DeletePageAsync(int id);
    }
}
