using Fall2025_Project3_cjhirschey.Models;

namespace Fall2025_Project3_cjhirschey.ViewModels
{
    public class GeneratedTweet
    {
        public string TweetText { get; set; }
        public double SentimentScore { get; set; }
    }

    public class ActorDetailViewModel
    {
        public Actor Actor { get; set; }
        public List<GeneratedTweet> Tweets { get; set; } = new List<GeneratedTweet>();
        public double OverallSentiment { get; set; }
    }
}