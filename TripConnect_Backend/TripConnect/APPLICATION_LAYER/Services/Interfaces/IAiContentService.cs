using APPLICATION_LAYER.DTOs.Ai;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Orchestrates AI-assisted trip creation: auto content generation,
    /// itinerary building, and banner image generation.
    /// </summary>
    public interface IAiContentService
    {
        /// <summary>
        /// Generate a compelling trip title and description from trip inputs.
        /// </summary>
        Task<GeneratedContentDto> GenerateTripContentAsync(GenerateContentDto dto);

        /// <summary>
        /// Generate a day-by-day itinerary for a trip from trip inputs.
        /// </summary>
        Task<GeneratedItineraryDto> GenerateItineraryAsync(GenerateItineraryDto dto);

        /// <summary>
        /// Generate a hero banner image for a trip from its title/location/description.
        /// </summary>
        Task<GeneratedBannerDto> GenerateBannerAsync(GenerateBannerDto dto);
    }
}