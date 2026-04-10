using DOMAIN_LAYER.Enum;

namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// TripRequest Repository Interface - Specific operations for TripRequest entity
    /// </summary>
    public interface ITripRequestRepository : IRepository<Entity.TripRequest.TripRequest>
    {
        /// <summary>
        /// Get requests by trip
        /// </summary>
        Task<IEnumerable<Entity.TripRequest.TripRequest>> GetRequestsByTripAsync(int tripId);

        /// <summary>
        /// Get requests by user
        /// </summary>
        Task<IEnumerable<Entity.TripRequest.TripRequest>> GetRequestsByUserAsync(int userId);

        /// <summary>
        /// Get requests by status
        /// </summary>
        Task<IEnumerable<Entity.TripRequest.TripRequest>> GetRequestsByStatusAsync(TripRequestStatus status);

        /// <summary>
        /// Get pending requests for trip
        /// </summary>
        Task<IEnumerable<Entity.TripRequest.TripRequest>> GetPendingRequestsByTripAsync(int tripId);

        /// <summary>
        /// Check if user already requested to join trip
        /// </summary>
        Task<bool> HasUserRequestedAsync(int userId, int tripId);

        /// <summary>
        /// Get request by user and trip
        /// </summary>
        Task<Entity.TripRequest.TripRequest> GetRequestByUserAndTripAsync(int userId, int tripId);

        /// <summary>
        /// Update request status
        /// </summary>
        Task UpdateRequestStatusAsync(int requestId, TripRequestStatus status);
    }
}
