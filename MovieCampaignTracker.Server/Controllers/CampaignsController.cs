using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using MovieCampaignTracker.Shared;

[ApiController]
[Route("api/[controller]")]
public class CampaignController : ControllerBase
{
    private readonly IDbConnection _db;

    public CampaignController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet("by-project/{projectId}")]
    public async Task<IActionResult> GetCampaignsByProject(int projectId)
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
        using var multi = await _db.QueryMultipleAsync(
            "GetCampaignsByProject",
            new { ProjectId = projectId },
            commandType: CommandType.StoredProcedure);

        // First result set: Campaigns
        var campaigns = (await multi.ReadAsync<Campaign>()).ToList();

        // Second result set: MediaPlatforms
        var mediaPlatforms = (await multi.ReadAsync<MediaPlatform>()).ToList();

        return Ok(new
        {
            campaigns,
            mediaPlatforms
        });
    }
    [HttpPost]
    public async Task<IActionResult> AddCampaign([FromBody] CampaignWithMediaDto dto)
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
        using var transaction = _db.BeginTransaction();
        try
        {
            // Add Campaign and get new CampaignId
            var campaignId = await _db.QuerySingleAsync<int>(
                "AddCampaign",
                new
                {
                    PromotionalElementId = dto.Campaign.PromotionalElementId,
                    ProjectId = dto.Campaign.ProjectId,
                    StartDate = dto.Campaign.StartDate,
                    EndDate = dto.Campaign.EndDate,
                    Status = dto.Campaign.Status
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction);

            // Add MediaPlatforms for this campaign
            foreach (var media in dto.MediaPlatforms)
            {
                await _db.ExecuteAsync(
                    "AddMediaPlatform",
                    new
                    {
                        CampaignId = campaignId,
                        PlatformName = media.PlatformName,
                        NumberOfPosts = media.NumberOfPosts
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: transaction);
            }

            transaction.Commit();
            return CreatedAtAction(nameof(GetCampaignsByProject), new { projectId = dto.Campaign.ProjectId }, new { campaignId });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("{campaignId}")]
    public async Task<IActionResult> UpdateCampaign(int campaignId, [FromBody] CampaignWithMediaDto dto)
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
        using var transaction = _db.BeginTransaction();
        try
        {
            // Update Campaign
            await _db.ExecuteAsync(
                "UpdateCampaign",
                new
                {
                    Id = campaignId,
                    PromotionalElementId = dto.Campaign.PromotionalElementId,
                    ProjectId = dto.Campaign.ProjectId,
                    StartDate = dto.Campaign.StartDate,
                    EndDate = dto.Campaign.EndDate,
                    Status = dto.Campaign.Status
                },
                commandType: CommandType.StoredProcedure,
                transaction: transaction);

            // Update or Add MediaPlatforms
            foreach (var media in dto.MediaPlatforms)
            {
                if (media.Id > 0)
                {
                    // Update existing media platform
                    await _db.ExecuteAsync(
                        "UpdateMediaPlatform",
                        new
                        {
                            Id = media.Id,
                            CampaignId = campaignId,
                            PlatformName = media.PlatformName,
                            NumberOfPosts = media.NumberOfPosts
                        },
                        commandType: CommandType.StoredProcedure,
                        transaction: transaction);
                }
                else
                {
                    // Add new media platform
                    await _db.ExecuteAsync(
                        "AddMediaPlatform",
                        new
                        {
                            CampaignId = campaignId,
                            PlatformName = media.PlatformName,
                            NumberOfPosts = media.NumberOfPosts
                        },
                        commandType: CommandType.StoredProcedure,
                        transaction: transaction);
                }
            }

            transaction.Commit();
            return NoContent();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return BadRequest(ex.Message);
        }
    }
    [HttpDelete("{campaignId}")]
    public async Task<IActionResult> DeleteCampaign(int campaignId)
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
        try
        {
            await _db.ExecuteAsync(
                "DeleteCampaign",
                new { CampaignId = campaignId },
                commandType: CommandType.StoredProcedure);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
