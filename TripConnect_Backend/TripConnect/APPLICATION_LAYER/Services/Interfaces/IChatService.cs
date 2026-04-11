using APPLICATION_LAYER.DTOs.Chat;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Chat Service Interface - Handles real-time messaging
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Send message to trip chat
        /// </summary>
        Task<ChatMessageResponseDto> SendMessageAsync(SendMessageDto dto, int userId);

        /// <summary>
        /// Get messages for trip
        /// </summary>
        Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripAsync(int tripId);

        /// <summary>
        /// Get messages for trip with pagination
        /// </summary>
        Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripPaginatedAsync(int tripId, int pageNumber, int pageSize);

        /// <summary>
        /// Get latest messages from trip
        /// </summary>
        Task<IEnumerable<ChatMessageResponseDto>> GetLatestMessagesAsync(int tripId, int count);

        /// <summary>
        /// Get messages sent by user
        /// </summary>
        Task<IEnumerable<ChatMessageResponseDto>> GetMessagesBySenderAsync(int userId);

        /// <summary>
        /// Search messages in trip
        /// </summary>
        Task<IEnumerable<ChatMessageResponseDto>> SearchMessagesAsync(int tripId, string searchText);

        /// <summary>
        /// Get message count for trip
        /// </summary>
        Task<int> GetMessageCountAsync(int tripId);

        /// <summary>
        /// Delete message
        /// </summary>
        Task<bool> DeleteMessageAsync(int messageId, int userId);
    }
}
