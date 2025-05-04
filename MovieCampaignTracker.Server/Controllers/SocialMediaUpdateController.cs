using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Data;
using System.Net.Http.Headers;
using System.Text.Json;
using MovieCampaignTracker.Shared;

[ApiController]
[Route("api/[controller]")]
public class SocialMediaUpdateController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IDbConnection _db;
    private readonly HttpClient _http;

    public SocialMediaUpdateController(IConfiguration config, IDbConnection db, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _db = db;
        _http = httpClientFactory.CreateClient();
    }

    [HttpPost("fetch-and-store")]
    public async Task<IActionResult> FetchAndStoreMetrics()
    {
        var date = DateTime.UtcNow.Date;
        var movie = _config["SocialMedia:MovieName"];

        // === YouTube ===
        var ytApiKey = _config["SocialMedia:YouTube:ApiKey"];
        var ytVideoId = _config["SocialMedia:YouTube:VideoId"];
        var ytUrl = $"https://www.googleapis.com/youtube/v3/videos?part=statistics,snippet&id={ytVideoId}&key={ytApiKey}";

        var ytResponse = await _http.GetStringAsync(ytUrl);
        var ytJson = JsonDocument.Parse(ytResponse);
        var ytData = ytJson.RootElement.GetProperty("items")[0];
        var ytStats = ytData.GetProperty("statistics");
        var ytSnippet = ytData.GetProperty("snippet");

        var ytMetric = new SocialMediaMetric
        {
            MovieName = movie,
            Platform = "YouTube",
            TitleOrText = ytSnippet.GetProperty("title").GetString(),
            ViewCount = long.Parse(ytStats.GetProperty("viewCount").GetString() ?? "0"),
            LikeCount = long.Parse(ytStats.GetProperty("likeCount").GetString() ?? "0"),
            CommentCount = long.Parse(ytStats.GetProperty("commentCount").GetString() ?? "0"),
            ShareCount = 0,
            FetchedAt = date
        };

        // === Twitter ===
        var twitterToken = _config["SocialMedia:Twitter:BearerToken"];
        var tweetId = _config["SocialMedia:Twitter:TweetId"];
        var twitterUrl = $"https://api.twitter.com/2/tweets/{tweetId}?tweet.fields=public_metrics,text";

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", twitterToken);
        var twResponse = await _http.GetStringAsync(twitterUrl);
        var twJson = JsonDocument.Parse(twResponse);
        var twData = twJson.RootElement.GetProperty("data");
        var twStats = twData.GetProperty("public_metrics");

        var twMetric = new SocialMediaMetric
        {
            MovieName = movie,
            Platform = "Twitter",
            TitleOrText = twData.GetProperty("text").GetString(),
            ViewCount = twStats.TryGetProperty("impression_count", out var impressions) ? impressions.GetInt64() : 0,
            LikeCount = twStats.GetProperty("like_count").GetInt64(),
            CommentCount = twStats.GetProperty("reply_count").GetInt64(),
            ShareCount = twStats.GetProperty("retweet_count").GetInt64(),
            FetchedAt = date
        };

        // === Save to DB using MERGE (UPSERT) ===
        string mergeSql = @"
            MERGE INTO SocialMediaMetrics AS Target
            USING (SELECT @MovieName AS MovieName, @Platform AS Platform, @FetchedAt AS FetchedAt) AS Source
            ON Target.MovieName = Source.MovieName AND Target.Platform = Source.Platform AND Target.FetchedAt = Source.FetchedAt
            WHEN MATCHED THEN
                UPDATE SET
                    TitleOrText = @TitleOrText,
                    ViewCount = @ViewCount,
                    LikeCount = @LikeCount,
                    CommentCount = @CommentCount,
                    ShareCount = @ShareCount
            WHEN NOT MATCHED THEN
                INSERT (MovieName, Platform, TitleOrText, ViewCount, LikeCount, CommentCount, ShareCount, FetchedAt)
                VALUES (@MovieName, @Platform, @TitleOrText, @ViewCount, @LikeCount, @CommentCount, @ShareCount, @FetchedAt);
        ";

        await _db.ExecuteAsync(mergeSql, ytMetric);
        await _db.ExecuteAsync(mergeSql, twMetric);

        return Ok(new { message = "YouTube and Twitter metrics fetched and upserted successfully." });
    }
}
