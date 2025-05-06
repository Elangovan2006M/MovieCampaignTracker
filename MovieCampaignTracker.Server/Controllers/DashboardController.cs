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
    public async Task<IActionResult> GetAllMetrics([FromQuery] DateTime? date = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        string baseSql = @"SELECT * FROM SocialMediaMetrics WHERE (@FetchedAt IS NULL OR CAST(FetchedAt AS DATE) = @FetchedAt)";
        string pagedSql = $@"
        {baseSql}
        ORDER BY ViewCount DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            FetchedAt = date?.Date,
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize
        };

        var result = await _db.QueryAsync<SocialMediaMetric>(pagedSql, parameters);
        return Ok(result);
    }

}
