using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.TripRequest;
using DOMAIN_LAYER.Enum;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// TripRequest Repository Implementation - Specific operations for TripRequest entity
    /// </summary>
    public class TripRequestRepository : Repository<TripRequest>, ITripRequestRepository
    {
        public TripRequestRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get requests by trip
        /// </summary>
        public async Task<IEnumerable<TripRequest>> GetRequestsByTripAsync(int tripId)
        {
            return await _dbSet.Where(tr => tr.TripId == tripId).ToListAsync();
        }

        /// <summary>
        /// Get requests by user
        /// </summary>
        public async Task<IEnumerable<TripRequest>> GetRequestsByUserAsync(int userId)
        {
            return await _dbSet.Where(tr => tr.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Get requests by status
        /// </summary>
        public async Task<IEnumerable<TripRequest>> GetRequestsByStatusAsync(TripRequestStatus status)
        {
            return await _dbSet.Where(tr => tr.Status == status).ToListAsync();
        }

        /// <summary>
        /// Get pending requests for trip
        /// </summary>
        public async Task<IEnumerable<TripRequest>> GetPendingRequestsByTripAsync(int tripId)
        {
            return await _dbSet.Where(tr => tr.TripId == tripId && tr.Status == TripRequestStatus.Pending).ToListAsync();
        }

        /// <summary>
        /// Check if user already requested to join trip
        /// </summary>
        public async Task<bool> HasUserRequestedAsync(int userId, int tripId)
        {
            return await _dbSet.AnyAsync(tr => tr.UserId == userId && tr.TripId == tripId);
        }

        /// <summary>
        /// Get request by user and trip
        /// </summary>
        public async Task<TripRequest> GetRequestByUserAndTripAsync(int userId, int tripId)
        {
            return await _dbSet.FirstOrDefaultAsync(tr => tr.UserId == userId && tr.TripId == tripId);
        }

        /// <summary>
        /// Update request status
        /// </summary>
        public async Task UpdateRequestStatusAsync(int requestId, TripRequestStatus status)
        {
            var request = await GetByIdAsync(requestId);
            if (request != null)
            {
                request.Status = status;
                request.RespondedAt = DateTime.Now;
                await UpdateAsync(request);
            }
        }
    }
}
