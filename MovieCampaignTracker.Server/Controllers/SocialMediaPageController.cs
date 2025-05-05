using Microsoft.AspNetCore.Mvc;
using MovieCampaignTracker.Infrastructure.Data;
using MovieCampaignTracker.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieCampaignTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialMediaPageController : ControllerBase
    {
        private readonly IDatabaseHelper _db;

        public SocialMediaPageController(IDatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IEnumerable<SocialMediaPage>> GetAll()
        {
            return await _db.GetAllSocialMediaPagesAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SocialMediaPage>> Get(int id)
        {
            var page = await _db.GetPageByIdAsync(id);
            if (page == null)
                return NotFound();
            return Ok(page);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SocialMediaPage page)
        {
            await _db.AddPageAsync(page);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SocialMediaPage page)
        {
            await _db.UpdatePageAsync(page);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _db.DeletePageAsync(id);
            return Ok();
        }
    }
}
