using APPLICATION_LAYER.DTOs.Trip;
using APPLICATION_LAYER.DTOs.TripMember;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Trip Service Interface - Handles trip management and operations
    /// </summary>
    public interface ITripService
    {
        /// <summary>
        /// Create a new trip
        /// </summary>
        Task<TripResponseDto> CreateTripAsync(CreateTripDto dto, int userId);

        /// <summary>
        /// Get trip by ID
        /// </summary>
        Task<TripResponseDto> GetTripByIdAsync(int tripId);

        /// <summary>
        /// Get all trips with pagination
        /// </summary>
        Task<IEnumerable<TripResponseDto>> GetAllTripsAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Get trips by status (Planned, Ongoing, Completed, Cancelled)
        /// </summary>
        Task<IEnumerable<TripResponseDto>> GetTripsByStatusAsync(string status);

        /// <summary>
        /// Get upcoming trips
        /// </summary>
        Task<IEnumerable<TripResponseDto>> GetUpcomingTripsAsync();

        /// <summary>
        /// Get trips by location
        /// </summary>
        Task<IEnumerable<TripResponseDto>> SearchTripsByLocationAsync(string location);

        /// <summary>
        /// Search trips by multiple criteria
        /// </summary>
        Task<IEnumerable<TripResponseDto>> SearchTripsAsync(string location, DateTime? startDate, decimal? minBudget, decimal? maxBudget, string travelType, DateTime? endDate);

        /// <summary>
        /// Get trips created by user
        /// </summary>
        Task<IEnumerable<TripResponseDto>> GetTripsCreatedByUserAsync(int userId);

        /// <summary>
        /// Update trip
        /// </summary>
        Task<TripResponseDto> UpdateTripAsync(int tripId, UpdateTripDto dto, int userId);

        /// <summary>
        /// Cancel trip
        /// </summary>
        Task<bool> CancelTripAsync(int tripId, int userId);

        /// <summary>
        /// Get trips user is member of
        /// </summary>
        Task<IEnumerable<TripResponseDto>> GetUserTripsAsync(int userId);

        /// <summary>
        /// Get members of a trip
        /// </summary>
        Task<IEnumerable<TripMemberResponseDto>> GetTripMembersAsync(int tripId);
    }
}
