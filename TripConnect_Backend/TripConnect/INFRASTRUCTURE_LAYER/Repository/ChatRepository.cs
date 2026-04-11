using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.ChatMessage;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// ChatMessage Repository Implementation - Specific operations for ChatMessage entity
    /// </summary>
    public class ChatRepository : Repository<ChatMessage>, IChatRepository
    {
        public ChatRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get messages by trip
        /// </summary>
        public async Task<IEnumerable<ChatMessage>> GetMessagesByTripAsync(int tripId)
        {
            return await _dbSet.Where(m => m.TripId == tripId)
                               .OrderBy(m => m.CreatedAt)
                               .ToListAsync();
        }

        /// <summary>
        /// Get messages sent by user
        /// </summary>
        public async Task<IEnumerable<ChatMessage>> GetMessagesBySenderAsync(int userId)
        {
            return await _dbSet.Where(m => m.SenderId == userId)
                               .OrderBy(m => m.CreatedAt)
                               .ToListAsync();
        }

        /// <summary>
        /// Get messages by trip with pagination
        /// </summary>
        public async Task<IEnumerable<ChatMessage>> GetMessagesByTripPaginatedAsync(int tripId, int pageNumber, int pageSize)
        {
            return await _dbSet.Where(m => m.TripId == tripId)
                               .OrderBy(m => m.CreatedAt)
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        }

        /// <summary>
        /// Get latest messages from trip
        /// </summary>
        public async Task<IEnumerable<ChatMessage>> GetLatestMessagesAsync(int tripId, int count)
        {
            return await _dbSet.Where(m => m.TripId == tripId)
                               .OrderByDescending(m => m.CreatedAt)
                               .Take(count)
                               .Reverse()
                               .ToListAsync();
        }

        /// <summary>
        /// Get messages after specific timestamp
        /// </summary>
        public async Task<IEnumerable<ChatMessage>> GetMessagesFromAsync(int tripId, DateTime dateTime)
        {
            return await _dbSet.Where(m => m.TripId == tripId && m.CreatedAt >= dateTime)
                               .OrderBy(m => m.CreatedAt)
                               .ToListAsync();
        }

        /// <summary>
        /// Search messages by content
        /// </summary>
        public async Task<IEnumerable<ChatMessage>> SearchMessagesAsync(int tripId, string searchText)
        {
            return await _dbSet.Where(m => m.TripId == tripId && m.Message.Contains(searchText))
                               .OrderBy(m => m.CreatedAt)
                               .ToListAsync();
        }

        /// <summary>
        /// Get message count for trip
        /// </summary>
        public async Task<int> GetMessageCountAsync(int tripId)
        {
            return await _dbSet.CountAsync(m => m.TripId == tripId);
        }

        /// <summary>
        /// Delete messages by trip (when trip is deleted)
        /// </summary>
        public async Task DeleteTripMessagesAsync(int tripId)
        {
            var messages = await _dbSet.Where(m => m.TripId == tripId).ToListAsync();
            _dbSet.RemoveRange(messages);
            await SaveChangesAsync();
        }
    }
}
