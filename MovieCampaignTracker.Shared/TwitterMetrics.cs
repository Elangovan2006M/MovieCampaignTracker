namespace MovieCampaignTracker.Shared
{
    public class TwitterMetrics
    {
        public string TweetId { get; set; } = string.Empty;
        public string? Text { get; set; }
        public int LikeCount { get; set; }
        public int RetweetCount { get; set; }
        public int ReplyCount { get; set; }
        public DateTime FetchedAt { get; set; }
    }
}
