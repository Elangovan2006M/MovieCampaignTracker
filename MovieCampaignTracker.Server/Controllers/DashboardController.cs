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
    public async Task<IActionResult> GetAllMetrics([FromQuery] DateTime? date = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10000)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@FetchedAt", date?.Date);
        parameters.Add("@Offset", (pageNumber - 1) * pageSize);
        parameters.Add("@PageSize", pageSize);

        var result = await _db.QueryAsync<SocialMediaMetric>("GetSocialMediaMetrics", parameters, commandType: CommandType.StoredProcedure);
        return Ok(result);
    }

}
