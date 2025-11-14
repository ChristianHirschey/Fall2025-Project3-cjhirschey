using Fall2025_Project3_cjhirschey.Models;

namespace Fall2025_Project3_cjhirschey.ViewModels
{
    public class GeneratedReview
    {
        public string ReviewText { get; set; }
        public double SentimentScore { get; set; }
    }

    public class MovieDetailViewModel
    {
        public Movie Movie { get; set; }
        public List<GeneratedReview> Reviews { get; set; } = new List<GeneratedReview>();
        public double AverageSentiment { get; set; }
    }
}