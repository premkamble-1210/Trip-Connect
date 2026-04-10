namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// TripRating Repository Interface - Specific operations for TripRating entity
    /// </summary>
    public interface ITripRatingRepository : IRepository<Entity.TripRating.TripRating>
    {
        /// <summary>
        /// Get ratings for a trip
        /// </summary>
        Task<IEnumerable<Entity.TripRating.TripRating>> GetRatingsByTripAsync(int tripId);

        /// <summary>
        /// Get ratings given by a user
        /// </summary>
        Task<IEnumerable<Entity.TripRating.TripRating>> GetRatingsGivenByUserAsync(int userId);

        /// <summary>
        /// Get ratings received by a user
        /// </summary>
        Task<IEnumerable<Entity.TripRating.TripRating>> GetRatingsReceivedByUserAsync(int userId);

        /// <summary>
        /// Get average rating for a user
        /// </summary>
        Task<double> GetAverageRatingForUserAsync(int userId);

        /// <summary>
        /// Get rating given by user for another user in a trip
        /// </summary>
        Task<Entity.TripRating.TripRating> GetRatingByUserAsync(int tripId, int ratedBy, int ratedUserId);

        /// <summary>
        /// Check if user already rated another user in trip
        /// </summary>
        Task<bool> HasUserRatedAsync(int tripId, int ratedBy, int ratedUserId);

        /// <summary>
        /// Get all ratings for a user in a specific trip
        /// </summary>
        Task<IEnumerable<Entity.TripRating.TripRating>> GetTripRatingsForUserAsync(int tripId, int userId);

        /// <summary>
        /// Get ratings count for user
        /// </summary>
        Task<int> GetRatingsCountAsync(int userId);

        /// <summary>
        /// Get ratings with review (non-empty reviews)
        /// </summary>
        Task<IEnumerable<Entity.TripRating.TripRating>> GetRatingsWithReviewAsync(int userId);

        /// <summary>
        /// Get average rating for trip
        /// </summary>
        Task<double> GetAverageRatingForTripAsync(int tripId);
    }
}
