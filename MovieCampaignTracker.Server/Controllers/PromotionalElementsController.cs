using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using MovieCampaignTracker.Shared;
using Dapper;
[ApiController]
[Route("api/[controller]")]
public class PromotionalElementsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IDbConnection _db;

    public PromotionalElementsController(IConfiguration config)
    {
        _config = config;
        _db = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }

    [HttpGet]
    public async Task<IEnumerable<PromotionalElement>> GetAll()
    {
        return await _db.QueryAsync<PromotionalElement>("SELECT * FROM PromotionalElements");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromotionalElement element)
    {
        var sql = "INSERT INTO PromotionalElements (PromotionElement, ProjectNameId) VALUES (@PromotionElement, @ProjectNameId)";
        await _db.ExecuteAsync(sql, element);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PromotionalElement element)
    {
        var sql = "UPDATE PromotionalElements SET PromotionElement = @PromotionElement, ProjectNameId = @ProjectNameId WHERE Id = @Id";
        element.Id = id;
        await _db.ExecuteAsync(sql, element);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.ExecuteAsync("DELETE FROM PromotionalElements WHERE Id = @Id", new { Id = id });
        return Ok();
    }

    public class PromotionalElement
    {
        public int Id { get; set; }
        public string PromotionElement { get; set; }
        public int ProjectNameId { get; set; }
    }
}
