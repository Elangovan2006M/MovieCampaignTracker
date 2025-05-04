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


    }
}
