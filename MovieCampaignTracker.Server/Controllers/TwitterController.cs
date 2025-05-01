using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MovieCampaignTracker.Shared;
using MovieCampaignTracker.Infrastructure.Data;

namespace MovieCampaignTracker.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwitterController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly string _bearerToken;
        private readonly string _tweetId;

        public TwitterController(DatabaseHelper dbHelper, IConfiguration configuration)
        {
            _dbHelper = dbHelper;
            _bearerToken = configuration["Twitter:BearerToken"]
                ?? throw new Exception("Twitter Bearer Token is not configured.");
            _tweetId = configuration["Twitter:TweetId"]
                ?? throw new Exception("Twitter TweetId is not configured.");
        }

        // GET: api/Twitter
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using var http = new HttpClient();
                http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");

                var url = $"https://api.twitter.com/2/tweets/{_tweetId}?tweet.fields=public_metrics,text";
                var response = await http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, error);
                }

                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var data = json["data"];
                var metrics = data?["public_metrics"];

                var result = new TwitterMetrics
                {
                    TweetId = _tweetId,
                    Text = (string?)data?["text"],
                    LikeCount = (int?)metrics?["like_count"] ?? 0,
                    RetweetCount = (int?)metrics?["retweet_count"] ?? 0,
                    ReplyCount = (int?)metrics?["reply_count"] ?? 0,
                    FetchedAt = DateTime.UtcNow
                };

                await _dbHelper.SaveTwitterMetricsAsync(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching Twitter data: {ex.Message}");
            }
        }

        // GET: api/Twitter/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _dbHelper.GetAllTwitterMetricsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching metrics: {ex.Message}");
            }
        }
    }
}