using APPLICATION_LAYER.DTOs.Rating;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Rating Service Interface - Handles user ratings and reviews
    /// </summary>
    public interface IRatingService
    {
        /// <summary>
        /// Create rating for user
        /// </summary>
        Task<RatingResponseDto> CreateRatingAsync(CreateRatingDto dto, int userId);

        /// <summary>
        /// Get rating by ID
        /// </summary>
        Task<RatingResponseDto> GetRatingByIdAsync(int ratingId);

        /// <summary>
        /// Get all ratings for user
        /// </summary>
        Task<IEnumerable<RatingResponseDto>> GetRatingsForUserAsync(int userId);

        /// <summary>
        /// Get average rating for user
        /// </summary>
        Task<double> GetAverageRatingAsync(int userId);

        /// <summary>
        /// Get ratings given by user
        /// </summary>
        Task<IEnumerable<RatingResponseDto>> GetRatingsGivenByUserAsync(int userId);

        /// <summary>
        /// Get ratings for trip
        /// </summary>
        Task<IEnumerable<RatingResponseDto>> GetRatingsForTripAsync(int tripId);

        /// <summary>
        /// Check if user already rated another user in trip
        /// </summary>
        Task<bool> HasUserRatedAsync(int tripId, int ratedBy, int ratedUserId);

        /// <summary>
        /// Update rating
        /// </summary>
        Task<RatingResponseDto> UpdateRatingAsync(int ratingId, CreateRatingDto dto, int userId);

        /// <summary>
        /// Delete rating
        /// </summary>
        Task<bool> DeleteRatingAsync(int ratingId, int userId);

        /// <summary>
        /// Get ratings count for user
        /// </summary>
        Task<int> GetRatingsCountAsync(int userId);
    }
}
