using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.TripRating;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// TripRating Repository Implementation - Specific operations for TripRating entity
    /// </summary>
    public class TripRatingRepository : Repository<TripRating>, ITripRatingRepository
    {
        public TripRatingRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get ratings for a trip
        /// </summary>
        public async Task<IEnumerable<TripRating>> GetRatingsByTripAsync(int tripId)
        {
            return await _dbSet.Where(r => r.TripId == tripId).ToListAsync();
        }

        /// <summary>
        /// Get ratings given by a user
        /// </summary>
        public async Task<IEnumerable<TripRating>> GetRatingsGivenByUserAsync(int userId)
        {
            return await _dbSet.Where(r => r.RatedBy == userId).ToListAsync();
        }

        /// <summary>
        /// Get ratings received by a user
        /// </summary>
        public async Task<IEnumerable<TripRating>> GetRatingsReceivedByUserAsync(int userId)
        {
            return await _dbSet.Where(r => r.RatedUserId == userId).ToListAsync();
        }

        /// <summary>
        /// Get average rating for a user
        /// </summary>
        public async Task<double> GetAverageRatingForUserAsync(int userId)
        {
            var ratings = await _dbSet.Where(r => r.RatedUserId == userId).ToListAsync();
            return ratings.Count > 0 ? ratings.Average(r => r.Rating) : 0;
        }

        /// <summary>
        /// Get rating given by user for another user in a trip
        /// </summary>
        public async Task<TripRating> GetRatingByUserAsync(int tripId, int ratedBy, int ratedUserId)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.TripId == tripId && r.RatedBy == ratedBy && r.RatedUserId == ratedUserId);
        }

        /// <summary>
        /// Check if user already rated another user in trip
        /// </summary>
        public async Task<bool> HasUserRatedAsync(int tripId, int ratedBy, int ratedUserId)
        {
            return await _dbSet.AnyAsync(r => r.TripId == tripId && r.RatedBy == ratedBy && r.RatedUserId == ratedUserId);
        }

        /// <summary>
        /// Get all ratings for a user in a specific trip
        /// </summary>
        public async Task<IEnumerable<TripRating>> GetTripRatingsForUserAsync(int tripId, int userId)
        {
            return await _dbSet.Where(r => r.TripId == tripId && r.RatedUserId == userId).ToListAsync();
        }

        /// <summary>
        /// Get ratings count for user
        /// </summary>
        public async Task<int> GetRatingsCountAsync(int userId)
        {
            return await _dbSet.CountAsync(r => r.RatedUserId == userId);
        }

        /// <summary>
        /// Get ratings with review (non-empty reviews)
        /// </summary>
        public async Task<IEnumerable<TripRating>> GetRatingsWithReviewAsync(int userId)
        {
            return await _dbSet.Where(r => r.RatedUserId == userId && !string.IsNullOrEmpty(r.Review)).ToListAsync();
        }

        /// <summary>
        /// Get average rating for trip
        /// </summary>
        public async Task<double> GetAverageRatingForTripAsync(int tripId)
        {
            var ratings = await _dbSet.Where(r => r.TripId == tripId).ToListAsync();
            return ratings.Count > 0 ? ratings.Average(r => r.Rating) : 0;
        }
    }
}
