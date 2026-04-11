using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.TripMember;
using DOMAIN_LAYER.Enum;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// TripMember Repository Implementation - Specific operations for TripMember entity
    /// </summary>
    public class TripMemberRepository : Repository<TripMember>, ITripMemberRepository
    {
        public TripMemberRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get members of a trip
        /// </summary>
        public async Task<IEnumerable<TripMember>> GetMembersByTripAsync(int tripId)
        {
            return await _dbSet.Where(tm => tm.TripId == tripId).ToListAsync();
        }

        /// <summary>
        /// Get trips a user is member of
        /// </summary>
        public async Task<IEnumerable<TripMember>> GetMembershipsByUserAsync(int userId)
        {
            return await _dbSet.Where(tm => tm.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Get members by role
        /// </summary>
        public async Task<IEnumerable<TripMember>> GetMembersByRoleAsync(int tripId, TripMemberRole role)
        {
            return await _dbSet.Where(tm => tm.TripId == tripId && tm.Role == role).ToListAsync();
        }

        /// <summary>
        /// Check if user is member of trip
        /// </summary>
        public async Task<bool> IsMemberAsync(int userId, int tripId)
        {
            return await _dbSet.AnyAsync(tm => tm.UserId == userId && tm.TripId == tripId);
        }

        /// <summary>
        /// Get member details
        /// </summary>
        public async Task<TripMember> GetMemberAsync(int userId, int tripId)
        {
            return await _dbSet.FirstOrDefaultAsync(tm => tm.UserId == userId && tm.TripId == tripId);
        }

        /// <summary>
        /// Get active members
        /// </summary>
        public async Task<IEnumerable<TripMember>> GetActiveMembersAsync(int tripId)
        {
            return await _dbSet.Where(tm => tm.TripId == tripId && tm.Status == TripMemberStatus.Active).ToListAsync();
        }

        /// <summary>
        /// Update member status
        /// </summary>
        public async Task UpdateMemberStatusAsync(int memberId, TripMemberStatus status)
        {
            var member = await GetByIdAsync(memberId);
            if (member != null)
            {
                member.Status = status;
                await UpdateAsync(member);
            }
        }

        /// <summary>
        /// Get member count for trip
        /// </summary>
        public async Task<int> GetMemberCountAsync(int tripId)
        {
            return await _dbSet.CountAsync(tm => tm.TripId == tripId && tm.Status == TripMemberStatus.Active);
        }
    }
}
