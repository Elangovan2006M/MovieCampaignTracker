using Dapper;
using Microsoft.AspNetCore.Mvc;
using MovieCampaignTracker.Shared;
using System.Data.SqlClient;

namespace MovieCampaignTracker.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CampaignController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        // GET to display all campaigns with pagination and filters for "All(All campaign status may be null), Planned, Ongoing, Completed"
        [HttpGet]
        public async Task<ActionResult> GetCampaigns([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            int offset = (page - 1) * pageSize;

            // Base query
            string baseCampaignsQuery = "SELECT * FROM Campaigns";
            string baseCountQuery = "SELECT COUNT(*) FROM Campaigns";
            string filter = "";

            if (!string.IsNullOrEmpty(status))
            {
                filter = " WHERE Status = @Status";
            }

            string pagedQuery = $@"
        {baseCampaignsQuery}{filter}
        ORDER BY Id
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string countQuery = $"{baseCountQuery}{filter};";

            var campaigns = await connection.QueryAsync<Campaigns>(pagedQuery, new
            {
                Offset = offset,
                PageSize = pageSize,
                Status = status
            });

            var media = await connection.QueryAsync<MediaPlatforms>("SELECT * FROM MediaPlatforms");

            var result = campaigns.Select(c => new CampaignWithMediaDto
            {
                Campaigns = c,
                MediaPlatforms = media.Where(m => m.CampaignId == c.Id).ToList()
            }).ToList();

            var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, new { Status = status });

            return Ok(new
            {
                data = result,
                totalCount,
                currentPage = page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }


        //POST to add new campaign
        [HttpPost]
        public async Task<IActionResult> AddCampaign(CampaignWithMediaDto dto)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var promoElementName = await connection.ExecuteScalarAsync<string>(
                    "SELECT PromotionElement FROM PromotionalElements WHERE Id = @Id",
                    new { Id = dto.Campaigns.PromotionalElementId },
                    transaction
                );
                var campaignId = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO Campaigns (PromotionalElementId, ProjectId, StartDate, EndDate, Status, PromotionalElementName)
                      VALUES (@PromotionalElementId, @ProjectId, @StartDate, @EndDate, @Status, @PromotionalElementName);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new
                    {
                        dto.Campaigns.PromotionalElementId,
                        dto.Campaigns.ProjectId,
                        dto.Campaigns.StartDate,
                        dto.Campaigns.EndDate,
                        dto.Campaigns.Status,
                        PromotionalElementName = promoElementName
                    },
                    transaction
                );

                foreach (var media in dto.MediaPlatforms)
                {
                    media.CampaignId = campaignId;
                    await connection.ExecuteAsync(
                        @"INSERT INTO MediaPlatforms (CampaignId, PlatformName, NumberOfPosts)
                          VALUES (@CampaignId, @PlatformName, @NumberOfPosts);",
                        media,
                        transaction
                    );
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Campaign added" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // PUT to update existing campaign
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCampaign(int id, CampaignWithMediaDto dto)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Step 1: Get the Promotional Element name
                var promoElementName = await connection.ExecuteScalarAsync<string>(
                    "SELECT PromotionElement FROM PromotionalElements WHERE Id = @Id",
                    new { Id = dto.Campaigns.PromotionalElementId },
                    transaction
                );

                // Step 2: Update Campaign details
                await connection.ExecuteAsync(
                    @"UPDATE Campaigns SET
                PromotionalElementId = @PromotionalElementId,
                ProjectId = @ProjectId,
                StartDate = @StartDate,
                EndDate = @EndDate,
                Status = @Status,
                PromotionalElementName = @PromotionalElementName
              WHERE Id = @Id;",
                    new
                    {
                        dto.Campaigns.PromotionalElementId,
                        dto.Campaigns.ProjectId,
                        dto.Campaigns.StartDate,
                        dto.Campaigns.EndDate,
                        dto.Campaigns.Status,
                        PromotionalElementName = promoElementName,
                        Id = id
                    },
                    transaction
                );

                // Step 3: Get all existing MediaPlatform IDs from DB for the campaign
                var existingMediaIds = (await connection.QueryAsync<int>(
                    "SELECT Id FROM MediaPlatforms WHERE CampaignId = @CampaignId",
                    new { CampaignId = id },
                    transaction
                )).ToList();

                // Step 4: Track which IDs are in the current request
                var incomingMediaIds = dto.MediaPlatforms
                    .Where(m => m.Id != 0)
                    .Select(m => m.Id)
                    .ToList();

                // Step 5: Delete media platforms that exist in DB but not in the incoming request
                var toDelete = existingMediaIds.Except(incomingMediaIds);
                foreach (var deleteId in toDelete)
                {
                    await connection.ExecuteAsync(
                        "DELETE FROM MediaPlatforms WHERE Id = @Id",
                        new { Id = deleteId },
                        transaction
                    );
                }

                // Step 6: Insert or update the incoming media platforms
                foreach (var media in dto.MediaPlatforms)
                {
                    media.CampaignId = id;

                    if (media.Id == 0)
                    {
                        // New entry
                        await connection.ExecuteAsync(
                            @"INSERT INTO MediaPlatforms (CampaignId, PlatformName, NumberOfPosts)
                      VALUES (@CampaignId, @PlatformName, @NumberOfPosts);",
                            media,
                            transaction
                        );
                    }
                    else
                    {
                        // Existing entry
                        await connection.ExecuteAsync(
                            @"UPDATE MediaPlatforms SET
                        PlatformName = @PlatformName,
                        NumberOfPosts = @NumberOfPosts
                      WHERE Id = @Id;",
                            media,
                            transaction
                        );
                    }
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Campaign updated with retained media platforms." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }


        // DELETE campaign
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync("DELETE FROM MediaPlatforms WHERE CampaignId = @id", new { id }, transaction);
                await connection.ExecuteAsync("DELETE FROM Campaigns WHERE Id = @id", new { id }, transaction);

                await transaction.CommitAsync();
                return Ok(new { message = "Campaign deleted" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }
    }
}


