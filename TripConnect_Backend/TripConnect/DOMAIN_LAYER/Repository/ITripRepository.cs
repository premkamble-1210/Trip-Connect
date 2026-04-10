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
        Task<IEnumerable<Entity.Trip.Trip>> SearchTripsAsync(string location, DateTime? startDate, decimal? maxBudget, string travelType);

        /// <summary>
        /// Get trip with all related data (includes members, requests, expenses)
        /// </summary>
        Task<Entity.Trip.Trip> GetTripWithDetailsAsync(int tripId);
    }
}
