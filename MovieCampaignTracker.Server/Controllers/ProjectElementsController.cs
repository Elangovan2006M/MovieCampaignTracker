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
    public class ProjectElementsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ProjectElementsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        // GET: api/ProjectElements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectElement>>> GetAll()
        {
            using var db = Connection;
            var sql = "SELECT * FROM ProjectElements";
            var result = await db.QueryAsync<ProjectElement>(sql);
            return Ok(result);
        }

        // POST: api/ProjectElements
        [HttpPost]
        public async Task<ActionResult> Add(ProjectElement project)
        {
            using var db = Connection;
            var sql = "INSERT INTO ProjectElements (ProjectName, ImageUrl) VALUES (@ProjectName, @ImageUrl)";
            await db.ExecuteAsync(sql, project);
            return Ok();
        }

        // DELETE: api/ProjectElements/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            using var db = Connection;
            var sql = "DELETE FROM ProjectElements WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
            return Ok();
        }

        // PUT: api/ProjectElements/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, ProjectElement project)
        {
            if (id != project.Id)
                return BadRequest("Project ID mismatch");

            using var db = Connection;
            var sql = "UPDATE ProjectElements SET ProjectName = @ProjectName, ImageUrl = @ImageUrl WHERE Id = @Id";
            await db.ExecuteAsync(sql, project);
            return Ok();
        }

    }

    public class ProjectElement
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string ImageUrl { get; set; }
    }
}
