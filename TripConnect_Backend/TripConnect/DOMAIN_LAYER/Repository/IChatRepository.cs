namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// ChatMessage Repository Interface - Specific operations for ChatMessage entity
    /// </summary>
    public interface IChatRepository : IRepository<Entity.ChatMessage.ChatMessage>
    {
        /// <summary>
        /// Get messages by trip
        /// </summary>
        Task<IEnumerable<Entity.ChatMessage.ChatMessage>> GetMessagesByTripAsync(int tripId);

        /// <summary>
        /// Get messages sent by user
        /// </summary>
        Task<IEnumerable<Entity.ChatMessage.ChatMessage>> GetMessagesBySenderAsync(int userId);

        /// <summary>
        /// Get messages by trip with pagination
        /// </summary>
        Task<IEnumerable<Entity.ChatMessage.ChatMessage>> GetMessagesByTripPaginatedAsync(int tripId, int pageNumber, int pageSize);

        /// <summary>
        /// Get latest messages from trip
        /// </summary>
        Task<IEnumerable<Entity.ChatMessage.ChatMessage>> GetLatestMessagesAsync(int tripId, int count);

        /// <summary>
        /// Get messages after specific timestamp
        /// </summary>
        Task<IEnumerable<Entity.ChatMessage.ChatMessage>> GetMessagesFromAsync(int tripId, DateTime dateTime);

        /// <summary>
        /// Search messages by content
        /// </summary>
        Task<IEnumerable<Entity.ChatMessage.ChatMessage>> SearchMessagesAsync(int tripId, string searchText);

        /// <summary>
        /// Get message count for trip
        /// </summary>
        Task<int> GetMessageCountAsync(int tripId);

        /// <summary>
        /// Delete messages by trip (when trip is deleted)
        /// </summary>
        Task DeleteTripMessagesAsync(int tripId);
    }
}
