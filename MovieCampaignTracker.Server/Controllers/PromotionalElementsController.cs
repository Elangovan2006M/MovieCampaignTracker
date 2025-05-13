using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionalElementsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public PromotionalElementsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        // GET: api/PromotionalElements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionalElement>>> GetAll()
        {
            using var db = Connection;
            var sql = "SELECT * FROM PromotionalElements";
            var result = await db.QueryAsync<PromotionalElement>(sql);
            return Ok(result);
        }

        // GET: api/PromotionalElements/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionalElement>> GetById(int id)
        {
            using var db = Connection;
            var sql = "SELECT * FROM PromotionalElements WHERE Id = @Id";
            var result = await db.QuerySingleOrDefaultAsync<PromotionalElement>(sql, new { Id = id });

            if (result == null)
                return NotFound($"Promotional element with ID {id} not found.");

            return Ok(result);
        }

        // POST: api/PromotionalElements
        [HttpPost]
        public async Task<ActionResult> Add(PromotionalElement promotionalElement)
        {
            if (string.IsNullOrWhiteSpace(promotionalElement.PromotionElement))
                return BadRequest("Promotion Element name cannot be empty.");

            using var db = Connection;
            var sql = "INSERT INTO PromotionalElements (PromotionElement, ProjectNameId) VALUES (@PromotionElement, @ProjectNameId)";
            await db.ExecuteAsync(sql, promotionalElement);
            return Ok();
        }

        // PUT: api/PromotionalElements/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, PromotionalElement promotionalElement)
        {
            if (id != promotionalElement.Id)
                return BadRequest("Promotional Element ID mismatch.");

            using var db = Connection;
            var sql = "UPDATE PromotionalElements SET PromotionElement = @PromotionElement, ProjectNameId = @ProjectNameId WHERE Id = @Id";
            await db.ExecuteAsync(sql, promotionalElement);
            return Ok();
        }

        // DELETE: api/PromotionalElements/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            using var db = Connection;
            var sql = "DELETE FROM PromotionalElements WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
            return Ok();
        }
    }

    public class PromotionalElement
    {
        public int Id { get; set; }
        public string PromotionElement { get; set; }
        public int ProjectNameId { get; set; }
    }
}