using Dapper;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MovieCampaignTracker.Shared;
using MySql.Data.MySqlClient;

namespace MovieCampaignTracker.Infrastructure.Data
{
    public class DatabaseHelper
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

        public async Task<(DataTable campaigns, DataTable mediaPlatforms)> GetCampaignsByProjectAsync(int projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetCampaignsByProject", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProjectId", projectId);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            var campaigns = new DataTable();
            campaigns.Load(reader);

            DataTable mediaPlatforms = null;
            if (await reader.NextResultAsync())
            {
                mediaPlatforms = new DataTable();
                mediaPlatforms.Load(reader);
            }

            return (campaigns, mediaPlatforms);
        }

        // Add Campaign, returns new campaign id
        public async Task<int> AddCampaignAsync(int promotionalElementId, int projectId, DateTime startDate, DateTime endDate, string status)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("AddCampaign", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PromotionalElementId", promotionalElementId);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            cmd.Parameters.AddWithValue("@Status", status);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Add MediaPlatform
        public async Task AddMediaPlatformAsync(int campaignId, string platformName, int numberOfPosts)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("AddMediaPlatform", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CampaignId", campaignId);
            cmd.Parameters.AddWithValue("@PlatformName", platformName);
            cmd.Parameters.AddWithValue("@NumberOfPosts", numberOfPosts);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // Delete Campaign
        public async Task DeleteCampaignAsync(int campaignId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DeleteCampaign", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CampaignId", campaignId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // Update Campaign
        public async Task UpdateCampaignAsync(int id, int promotionalElementId, int projectId, DateTime startDate, DateTime endDate, string status)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("UpdateCampaign", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@PromotionalElementId", promotionalElementId);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            cmd.Parameters.AddWithValue("@Status", status);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // Update MediaPlatform
        public async Task UpdateMediaPlatformAsync(int id, int campaignId, string platformName, int numberOfPosts)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("UpdateMediaPlatform", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@CampaignId", campaignId);
            cmd.Parameters.AddWithValue("@PlatformName", platformName);
            cmd.Parameters.AddWithValue("@NumberOfPosts", numberOfPosts);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
