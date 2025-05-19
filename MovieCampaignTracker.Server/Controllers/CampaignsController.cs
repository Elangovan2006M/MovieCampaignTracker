using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using MovieCampaignTracker.Shared;
using static MovieCampaignTracker.Client.Pages.Campaign;

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

        var campaignList = (await multi.ReadAsync<Campaigns>()).ToList();
        var mediaList = (await multi.ReadAsync<MediaPlatforms>()).ToList();

        // Join campaign and media by CampaignId
        var result = campaignList.Select(c => new CampaignWithMediaDto
        {
            Campaigns = c,
            MediaPlatforms = mediaList.Where(m => m.CampaignId == c.Id).ToList()
        }).ToList();

        return Ok(new ApiResponse
        {
            Campaigns = result
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
                    PromotionalElementId = dto.Campaigns.PromotionalElementId,
                    ProjectId = dto.Campaigns.ProjectId,
                    StartDate = dto.Campaigns.StartDate,
                    EndDate = dto.Campaigns.EndDate,
                    Status = dto.Campaigns.Status
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
            return CreatedAtAction(nameof(GetCampaignsByProject), new { projectId = dto.Campaigns.ProjectId }, new { campaignId });
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
                    PromotionalElementId = dto.Campaigns.PromotionalElementId,
                    ProjectId = dto.Campaigns.ProjectId,
                    StartDate = dto.Campaigns.StartDate,
                    EndDate = dto.Campaigns.EndDate,
                    Status = dto.Campaigns.Status
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
