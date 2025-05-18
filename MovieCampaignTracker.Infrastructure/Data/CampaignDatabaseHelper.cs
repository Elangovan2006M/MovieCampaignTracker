//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Dapper;
//using Microsoft.Extensions.Configuration;
//using MovieCampaignTracker.Shared;

//namespace MovieCampaignTracker.Infrastructure.Data
//{
//    public class CampaignDatabaseHelper : ICampaignDatabaseHelper
//    {
//        private readonly IConfiguration _config;
//        private readonly string _connStr;

//        public CampaignDatabaseHelper(IConfiguration config)
//        {
//            _config = config;
//            _connStr = _config.GetConnectionString("DefaultConnection");
//        }

//        public async Task<IEnumerable<Campaigns>> GetAllCampaignsAsync()
//        {
//            using var conn = new SqlConnection(_connStr);
//            var query = "SELECT * FROM Campaigns";
//            return await conn.QueryAsync<Campaigns>(query);
//        }

//        public async Task<Campaigns> GetCampaignByIdAsync(int id)
//        {
//            using var conn = new SqlConnection(_connStr);
//            return await conn.QueryFirstOrDefaultAsync<Campaigns>("SELECT * FROM Campaigns WHERE Id = @Id", new { Id = id });
//        }

//        public async Task<int> AddCampaignAsync(Campaigns campaign)
//        {
//            using var conn = new SqlConnection(_connStr);
//            var query = @"INSERT INTO Campaigns (ProjectId, PromotionalElement, StartDate, EndDate, Status)
//                      VALUES (@ProjectId, @PromotionalElement, @StartDate, @EndDate, @Status)";
//            return await conn.ExecuteAsync(query, campaign);
//        }

//        public async Task<IEnumerable<MediaPlatform>> GetAllMediaPlatformsAsync()
//        {
//            using var conn = new SqlConnection(_connStr);
//            return await conn.QueryAsync<MediaPlatform>("SELECT * FROM MediaPlatforms");
//        }

//        public async Task<int> AddMediaPlatformAsync(MediaPlatform platform)
//        {
//            using var conn = new SqlConnection(_connStr);
//            var query = "INSERT INTO MediaPlatforms (Name) VALUES (@Name)";
//            return await conn.ExecuteAsync(query, platform);
//        }

//        public async Task<IEnumerable<CampaignPlatformPost>> GetAllCampaignPlatformPostsAsync()
//        {
//            using var conn = new SqlConnection(_connStr);
//            return await conn.QueryAsync<CampaignPlatformPost>("SELECT * FROM CampaignPlatformPosts");
//        }

//        public async Task<int> AddCampaignPlatformPostAsync(CampaignPlatformPost post)
//        {
//            using var conn = new SqlConnection(_connStr);
//            var query = @"INSERT INTO CampaignPlatformPosts (CampaignId, PlatformId, NumberOfPosts)
//                      VALUES (@CampaignId, @PlatformId, @NumberOfPosts)";
//            return await conn.ExecuteAsync(query, post);
//        }
//    }

//}
