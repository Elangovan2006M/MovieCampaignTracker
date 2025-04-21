using Dapper;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace MovieCampaignTracker.Infrastructure;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(string storedProcedure, DynamicParameters parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ExecuteStoredProcedureAsync(string storedProcedure, DynamicParameters parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }
}
