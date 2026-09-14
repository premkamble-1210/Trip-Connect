using DOMAIN_LAYER.Enum;

namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Trip Repository Interface - Specific operations for Trip entity
    /// </summary>
    public interface ITripRepository : IRepository<Entity.Trip.Trip>
    {
        /// <summary>
        /// Get trips by status
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetTripsByStatusAsync(TripStatus status);

        /// <summary>
        /// Get trips by location
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetTripsByLocationAsync(string location);

        /// <summary>
        /// Get trips by host/organizer
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetTripsByHostAsync(int hostId);

        /// <summary>
        /// Get upcoming trips (not started yet)
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetUpcomingTripsAsync();

        /// <summary>
        /// Get trips within budget range
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetTripsByBudgetAsync(decimal minBudget, decimal maxBudget);

        /// <summary>
        /// Get trips by travel type
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetTripsByTravelTypeAsync(string travelType);

        /// <summary>
        /// Search trips by multiple criteria
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> SearchTripsAsync(string location, DateTime? startDate, decimal? minBudget, decimal? maxBudget, string travelType, DateTime? endDate);

        /// <summary>
        /// Get the ranked discover feed (Planned, ended in future, seats available),
        /// ordered by popularity / host trust / freshness / availability
        /// </summary>
        Task<(IEnumerable<Entity.Trip.Trip> Trips, int TotalCount)> GetDiscoverTripsAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Get trips the user has interacted with (hosted, active member, or pending/accepted join request)
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetUserHistoryTripsAsync(int userId);

        /// <summary>
        /// Get the top ranked discover candidates (excluding the user's own / joined / requested trips)
        /// </summary>
        Task<IEnumerable<Entity.Trip.Trip>> GetRecommendationCandidatesAsync(int userId, int take);

        /// <summary>
        /// Get trip with all related data (includes members, requests, expenses)
        /// </summary>
        Task<Entity.Trip.Trip> GetTripWithDetailsAsync(int tripId);
    }
}
