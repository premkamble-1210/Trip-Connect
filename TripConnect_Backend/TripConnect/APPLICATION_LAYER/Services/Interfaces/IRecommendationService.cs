using APPLICATION_LAYER.DTOs.Trip;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Recommendation Service - Generates personalized trip recommendations
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Get personalized recommendations for a user.
        /// Falls back to the popular (Phase-A ranked) feed on cold start.
        /// </summary>
        Task<IEnumerable<TripResponseDto>> GetRecommendationsAsync(int userId, int count = 6);
    }
}