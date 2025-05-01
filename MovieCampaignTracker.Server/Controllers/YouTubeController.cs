using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MovieCampaignTracker.Shared;
using MovieCampaignTracker.Infrastructure.Data;

namespace MovieCampaignTracker.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YouTubeController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly string _apiKey;
        private readonly string _videoId;

        public YouTubeController(DatabaseHelper dbHelper, IConfiguration configuration)
        {
            _dbHelper = dbHelper;
            _apiKey = configuration["YouTube:ApiKey"]
                ?? throw new Exception("YouTube ApiKey is not configured.");
            _videoId = configuration["YouTube:VideoId"]
                ?? throw new Exception("YouTube VideoId is not configured.");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var http = new HttpClient();
            var url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&id={_videoId}&key={_apiKey}";
            var response = await http.GetStringAsync(url);
            var json = JObject.Parse(response);
            var video = json["items"]?.FirstOrDefault();

            var result = new YouTubeMetrics
            {
                VideoId = _videoId,
                Title = (string?)video?["snippet"]?["title"],
                ViewCount = (string?)video?["statistics"]?["viewCount"],
                LikeCount = (string?)video?["statistics"]?["likeCount"],
                CommentCount = (string?)video?["statistics"]?["commentCount"],
                FetchedAt = DateTime.UtcNow
            };

            await _dbHelper.SaveYouTubeMetricsAsync(result);

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var results = await _dbHelper.GetAllYouTubeMetricsAsync();
            return Ok(results);
        }
    }
}