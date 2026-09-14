using APPLICATION_LAYER.DTOs.Ai;
using APPLICATION_LAYER.DTOs.Trip;
using APPLICATION_LAYER.Services.Interfaces;
using INFRASTRUCTURE_LAYER.Services;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    /// <summary>
    /// Orchestrates the three AI-assisted trip creation features using the
    /// local Ollama model (text) and pollinations.ai (banner images). All
    /// external calls fail gracefully so manual entry remains the fallback.
    /// </summary>
    public class AiContentService : IAiContentService
    {
        private const int MaxTitleLength = 120;
        private const int MaxDescriptionLength = 3000;
        private const int MaxDays = 30;
        private const string BannerFolder = "trip-banners";

        private static readonly string ContentSystemPrompt =
            "You are a professional travel copywriter. You write engaging, concise trip listing " +
            "titles and descriptions that make travelers excited to join. " +
            "Always respond with valid JSON ONLY in exactly this shape, no markdown, no code fences: " +
            "{\"title\": string, \"description\": string}. " +
            "The title must be under 100 characters. The description must be 2-4 sentences (80-400 characters) " +
            "and highlight the experience, the destination, and the travel style.";

        private readonly IOllamaService _ollamaService;
        private readonly IBannerService _bannerService;
        private readonly IImageKitService _imageKitService;
        private readonly ILogger _logger;

        public AiContentService(IOllamaService ollamaService, IBannerService bannerService, IImageKitService imageKitService, ILogger logger)
        {
            _ollamaService = ollamaService;
            _bannerService = bannerService;
            _imageKitService = imageKitService;
            _logger = logger;
        }

        public async Task<GeneratedContentDto> GenerateTripContentAsync(GenerateContentDto dto)
        {
            ValidateBasics(dto.Location, dto.StartDate, dto.EndDate);

            var userPrompt =
                $"Create a trip listing with the following details:\n" +
                $"- Destination: {dto.Location}\n" +
                $"- Budget: {dto.Budget}\n" +
                $"- Travel type: {dto.TravelType}\n" +
                $"- Travel window: {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}\n" +
                $"{(dto.Seats.HasValue ? $"- Seats available: {dto.Seats}\n" : string.Empty)}" +
                $"Respond with JSON only.";

            var result = await _ollamaService.GenerateJsonAsync<GeneratedContentDto>(ContentSystemPrompt, userPrompt);
            if (result == null || string.IsNullOrWhiteSpace(result.Title) || string.IsNullOrWhiteSpace(result.Description))
            {
                _logger.Warning("AI content generation failed for location: {Location}", dto.Location);
                throw new InvalidOperationException("AI content generation failed. Please check that Ollama is running, or enter the details manually.");
            }

            result.Title = CapLength(result.Title.Trim(), MaxTitleLength);
            result.Description = CapLength(result.Description.Trim(), MaxDescriptionLength);

            return result;
        }

        public async Task<GeneratedItineraryDto> GenerateItineraryAsync(GenerateItineraryDto dto)
        {
            ValidateBasics(dto.Location, dto.StartDate, dto.EndDate);

            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("End Date must be after or equal to Start Date");

            var dayCount = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;
            if (dayCount < 1)
                dayCount = 1;
            if (dayCount > MaxDays)
            {
                _logger.Warning("Itinerary request clamped from {Requested} to {Max} days", dayCount, MaxDays);
                dayCount = MaxDays;
            }

            var userPrompt =
                $"Plan a {dayCount}-day trip:\n" +
                $"- Destination/region: {dto.Location}\n" +
                $"- Budget: {dto.Budget}\n" +
                $"- Travel type: {dto.TravelType}\n" +
                $"- Trip dates: {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}\n\n" +
                $"Respond with valid JSON ONLY in exactly this shape, no markdown, no code fences:\n" +
                $"{{\"trip_days\": [{{\"location\": \"City or area name\", \"description\": \"2-3 sentences of activities for that day\"}}]}}\n" +
                $"Provide exactly {dayCount} entries, one per day, ordered chronologically. Each description should mention activities, food, or sights specific to {dto.Location} and appropriate for {dto.TravelType} travel.";

            var systemPrompt =
                "You are an expert travel itinerary planner. You craft realistic, memorable day-by-day travel plans. " +
                "Always respond with valid JSON ONLY and nothing else.";

            var parsed = await _ollamaService.GenerateJsonAsync<ItineraryJsonResponse>(systemPrompt, userPrompt);
            if (parsed?.TripDays == null || parsed.TripDays.Count == 0)
            {
                _logger.Warning("AI itinerary generation failed for location: {Location}", dto.Location);
                throw new InvalidOperationException("AI itinerary generation failed. Please check that Ollama is running, or build the itinerary manually.");
            }

            var days = new List<TripDayDto>();
            for (var i = 0; i < Math.Min(parsed.TripDays.Count, dayCount); i++)
            {
                var item = parsed.TripDays[i];
                days.Add(new TripDayDto
                {
                    Day = i + 1,
                    Location = string.IsNullOrWhiteSpace(item.Location) ? dto.Location : CapLength(item.Location.Trim(), 200),
                    Date = DateOnly.FromDateTime(dto.StartDate.Date.AddDays(i)),
                    Description = string.IsNullOrWhiteSpace(item.Description) ? null : CapLength(item.Description.Trim(), MaxDescriptionLength),
                    Latitude = null,
                    Longitude = null
                });
            }

            return new GeneratedItineraryDto { TripDays = days };
        }

        public async Task<GeneratedBannerDto> GenerateBannerAsync(GenerateBannerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Location))
                throw new InvalidOperationException("Location is required for banner generation");

            var prompt = BuildBannerPrompt(dto.Title, dto.Location, dto.Description);
            var directUrl = _bannerService.BuildBannerUrl(prompt);

            var bytes = await _bannerService.DownloadBannerImageAsync(prompt);
            if (bytes == null)
                throw new InvalidOperationException("Banner generation failed. Please try again later or upload an image manually.");

            var fileName = $"banner-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.jpg";
            var upload = await _imageKitService.UploadImageAsync(new MemoryStream(bytes), fileName, BannerFolder);

            if (upload.Success && !string.IsNullOrWhiteSpace(upload.Url))
            {
                _logger.Information("AI banner uploaded to ImageKit: {Url}", upload.Url);
                return new GeneratedBannerDto { ImgUrl = upload.Url, Source = "imagekit" };
            }

            _logger.Warning("AI banner uploaded to ImageKit failed ({Error}); returning direct URL.", upload.ErrorMessage);
            return new GeneratedBannerDto { ImgUrl = directUrl, Source = "pollinations" };
        }

        private static string BuildBannerPrompt(string title, string location, string description)
        {
            var titlePart = string.IsNullOrWhiteSpace(title) ? "a scenic group travel trip" : title.Trim();
            var descPart = string.IsNullOrWhiteSpace(description)
                ? string.Empty
                : SanitizePrompt(description.Trim());
            var descSuffix = string.IsNullOrWhiteSpace(descPart) ? string.Empty : $" including {descPart}";

            var prompt =
                $"Postcard-style professional travel banner image for a trip titled \"{SanitizePrompt(titlePart)}\" " +
                $"in {SanitizePrompt(location)}{descSuffix}. Vibrant colors, epic landscape, photorealistic, wide 16:9 composition, no text overlay.";

            return prompt;
        }

        private static string SanitizePrompt(string value)
        {
            var sanitized = value.Replace('"', ' ').Replace('\'', ' ').Replace('\r', ' ').Replace('\n', ' ');
            return sanitized.Length > 400 ? sanitized[..400] : sanitized;
        }

        private static void ValidateBasics(string location, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new InvalidOperationException("Location is required");
            if (startDate == default)
                throw new InvalidOperationException("Start Date is required");
            if (endDate == default)
                throw new InvalidOperationException("End Date is required");
        }

        private static string CapLength(string value, int maxLength)
            => value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>
    /// Internal shape the itinerary model returns (see GenerateItineraryAsync).
    /// </summary>
    public class ItineraryJsonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("trip_days")]
        public List<ItineraryDayItem>? TripDays { get; set; }
    }

    public class ItineraryDayItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("location")]
        public string? Location { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}