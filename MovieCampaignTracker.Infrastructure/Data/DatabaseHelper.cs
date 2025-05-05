using Dapper;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Infrastructure.Data
{
    public class DatabaseHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(string storedProcedure, DynamicParameters parameters)
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ExecuteStoredProcedureAsync(string storedProcedure, DynamicParameters parameters)
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task SaveTwitterMetricsAsync(TwitterMetrics metrics)
        {
            var query = @"INSERT INTO TwitterMetrics (TweetId, Text, LikeCount, RetweetCount, ReplyCount, FetchedAt)
                          VALUES (@TweetId, @Text, @LikeCount, @RetweetCount, @ReplyCount, @FetchedAt)";
            using var connection = GetConnection();
            await connection.ExecuteAsync(query, metrics);
        }

        public async Task SaveYouTubeMetricsAsync(YouTubeMetrics metrics)
        {
            var query = @"INSERT INTO YouTubeMetrics (VideoId, Title, ViewCount, LikeCount, CommentCount, FetchedAt)
                          VALUES (@VideoId, @Title, @ViewCount, @LikeCount, @CommentCount, @FetchedAt)";
            using var connection = GetConnection();
            await connection.ExecuteAsync(query, metrics);
        }

        public async Task<IEnumerable<TwitterMetrics>> GetAllTwitterMetricsAsync()
        {
            var query = "SELECT * FROM TwitterMetrics ORDER BY FetchedAt DESC";
            using var connection = GetConnection();
            return await connection.QueryAsync<TwitterMetrics>(query);
        }

        public async Task<IEnumerable<YouTubeMetrics>> GetAllYouTubeMetricsAsync()
        {
            var query = "SELECT * FROM YouTubeMetrics ORDER BY FetchedAt DESC";
            using var connection = GetConnection();
            return await connection.QueryAsync<YouTubeMetrics>(query);
        }

        public async Task<IEnumerable<SocialMediaPage>> GetAllSocialMediaPagesAsync()
        {
            var query = "SELECT * FROM SocialMediaPages ORDER BY Id DESC";
            using var connection = GetConnection();
            return await connection.QueryAsync<SocialMediaPage>(query);
        }

        public async Task<SocialMediaPage> GetPageByIdAsync(int id)
        {
            var query = "SELECT * FROM SocialMediaPages WHERE Id = @Id";
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<SocialMediaPage>(query, new { Id = id });
        }

        public async Task AddPageAsync(SocialMediaPage page)
        {
            var query = @"INSERT INTO SocialMediaPages (Platform, PageUrl, PageName, FollowersCount, AdminName, AdminMobile)
                          VALUES (@Platform, @PageUrl, @PageName, @FollowersCount, @AdminName, @AdminMobile, @ViewCount, @LikeCount, @CommentCount, @ShareCount)";
            using var connection = GetConnection();
            await connection.ExecuteAsync(query, page);
        }

        public async Task UpdatePageAsync(SocialMediaPage page)
        {
            var query = @"UPDATE SocialMediaPages SET Platform = @Platform, PageUrl = @PageUrl, PageName = @PageName, FollowersCount = @FollowersCount, AdminName = @AdminName, AdminMobile = @AdminMobile, ViewCount = @ViewCount, LikeCount = @LikeCount, C   CommentCount = @CommentCount, ShareCount = @ShareCount WHERE Id = @Id";
            using var connection = GetConnection();
            await connection.ExecuteAsync(query, page);
        }

        public async Task DeletePageAsync(int id)
        {
            var query = "DELETE FROM SocialMediaPages WHERE Id = @Id";
            using var connection = GetConnection();
            await connection.ExecuteAsync(query, new { Id = id });
        }
    }
}