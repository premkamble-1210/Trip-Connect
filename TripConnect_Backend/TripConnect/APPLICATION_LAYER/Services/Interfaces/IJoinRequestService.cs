using APPLICATION_LAYER.DTOs.JoinRequest;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Join Request Service Interface - Handles trip join requests
    /// </summary>
    public interface IJoinRequestService
    {
        /// <summary>
        /// Send join request for a trip
        /// </summary>
        Task<JoinRequestResponseDto> SendJoinRequestAsync(SendJoinRequestDto dto, int userId);

        /// <summary>
        /// Get join request by ID
        /// </summary>
        Task<JoinRequestResponseDto> GetJoinRequestByIdAsync(int requestId);

        /// <summary>
        /// Get pending requests for a trip
        /// </summary>
        Task<IEnumerable<JoinRequestResponseDto>> GetPendingRequestsByTripAsync(int tripId);

        /// <summary>
        /// Get requests sent by user
        /// </summary>
        Task<IEnumerable<JoinRequestResponseDto>> GetRequestsByUserAsync(int userId);

        /// <summary>
        /// Accept join request
        /// </summary>
        Task<bool> AcceptJoinRequestAsync(int requestId, int tripHostId);

        /// <summary>
        /// Reject join request
        /// </summary>
        Task<bool> RejectJoinRequestAsync(int requestId, int tripHostId);

        /// <summary>
        /// Cancel join request
        /// </summary>
        Task<bool> CancelJoinRequestAsync(int requestId, int userId);

        /// <summary>
        /// Check if user already requested to join
        /// </summary>
        Task<bool> HasUserRequestedAsync(int userId, int tripId);

        /// <summary>
        /// Get all requests for trip
        /// </summary>
        Task<IEnumerable<JoinRequestResponseDto>> GetAllRequestsByTripAsync(int tripId);
    }
}
