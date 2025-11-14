using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using VaderSharp2;
using Fall2025_Project3_cjhirschey.ViewModels;
using System.ClientModel;

namespace Fall2025_Project3_cjhirschey.Services
{
    public class AiApiService
    {
        private readonly ChatClient _client;
        private readonly SentimentIntensityAnalyzer _analyzer;
        private readonly string _deploymentName = "gpt-4.1-nano";

        public AiApiService(IConfiguration configuration)
        {
            // get strings from secrets.json
            string endpointString = configuration["OpenAI:Endpoint"];
            string apiKeyString = configuration["OpenAI:ApiKey"];

            // check for null/empty strings
            if (string.IsNullOrWhiteSpace(endpointString) || string.IsNullOrWhiteSpace(apiKeyString))
            {
                throw new InvalidOperationException("OpenAI:Endpoint or OpenAI:ApiKey is not set in secrets.json.");
            }

            // convert strings to the correct types
            var endpoint = new Uri(endpointString);
            var apiKey = new ApiKeyCredential(apiKeyString);

            // create the clients and assign them to correct fields
            var azureClient = new AzureOpenAIClient(endpoint, apiKey);
            _client = azureClient.GetChatClient(_deploymentName);
            _analyzer = new SentimentIntensityAnalyzer();
        }

        private async Task<string> GetChatContentAsync(string systemPrompt, string userContent, float temperature = 0.8f)
        {
            var options = new ChatCompletionOptions()
            {
                Temperature = temperature
            };

            var messages = new List<ChatMessage>()
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userContent)
            };

            ClientResult<ChatCompletion> response = await _client.CompleteChatAsync(messages, options);

            // extract the text from the response
            try
            {
                // extract content items with Text
                var first = response.Value.Content?.FirstOrDefault();
                var text = first?.Text;
                if (!string.IsNullOrWhiteSpace(text)) return text;

                // try ToString on the first content item as a fallback
                if (first != null)
                {
                    var alt = first.ToString();
                    if (!string.IsNullOrWhiteSpace(alt)) return alt;
                }

                // final fallback: stringify the whole response value
                var whole = response.Value?.ToString();
                return !string.IsNullOrWhiteSpace(whole) ? whole : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<List<GeneratedReview>> GetMovieReviewsAsync(string movieTitle)
        {
            try
            {
                var system = "You are a movie critic. Generate exactly 3 short, distinct movie reviews for the movie specified by the user. Separate each review with the '||' delimiter. Do not number them. Return only the movie reviews, with no extra text before or after the reviews.";
                var content = await GetChatContentAsync(system, movieTitle, temperature: 0.8f);

                var reviews = content
                    .Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .Where(r => !string.IsNullOrEmpty(r))
                    .Select(r =>
                    {
                        var score = _analyzer.PolarityScores(r).Compound;
                        return new GeneratedReview { ReviewText = r, SentimentScore = score };
                    })
                    .ToList();

                return reviews;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return new List<GeneratedReview>();
            }
        }

        public async Task<List<GeneratedTweet>> GetActorTweetsAsync(string actorName)
        {
            try
            {
                var system = "You are a social media simulator. Generate exactly 5 short, distinct tweets about the actor specified by the user, as if from different Twitter users. Include hashtags. Separate each tweet with the '||' delimiter. Return only the tweets, with no extra text before or after the tweets.";
                var content = await GetChatContentAsync(system, actorName, temperature: 1.0f);

                var tweets = content
                    .Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Select(t =>
                    {
                        var score = _analyzer.PolarityScores(t).Compound;
                        return new GeneratedTweet { TweetText = t, SentimentScore = score };
                    })
                    .ToList();

                return tweets;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return new List<GeneratedTweet>();
            }
        }
    }
}