using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Data;
using MovieCampaignTracker.Shared;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDbConnection _db;

    public DashboardController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllMetrics([FromQuery] DateTime? date = null)
    {
        string sql;
        object param;

        if (date.HasValue)
        {
            sql = @"SELECT * FROM SocialMediaMetrics 
                    WHERE FetchedAt = @FetchedAt 
                    ORDER BY FetchedAt DESC, Platform";
            param = new { FetchedAt = date.Value.Date };
        }
        else
        {
            sql = @"SELECT * FROM SocialMediaMetrics 
                    ORDER BY FetchedAt DESC, Platform";
            param = null;
        }

        var result = await _db.QueryAsync<SocialMediaMetric>(sql, param);
        return Ok(result);
    }
}
