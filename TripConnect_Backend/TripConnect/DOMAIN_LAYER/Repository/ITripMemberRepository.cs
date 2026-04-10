using DOMAIN_LAYER.Enum;

namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// TripMember Repository Interface - Specific operations for TripMember entity
    /// </summary>
    public interface ITripMemberRepository : IRepository<Entity.TripMember.TripMember>
    {
        /// <summary>
        /// Get members of a trip
        /// </summary>
        Task<IEnumerable<Entity.TripMember.TripMember>> GetMembersByTripAsync(int tripId);

        /// <summary>
        /// Get trips a user is member of
        /// </summary>
        Task<IEnumerable<Entity.TripMember.TripMember>> GetMembershipsByUserAsync(int userId);

        /// <summary>
        /// Get members by role
        /// </summary>
        Task<IEnumerable<Entity.TripMember.TripMember>> GetMembersByRoleAsync(int tripId, TripMemberRole role);

        /// <summary>
        /// Check if user is member of trip
        /// </summary>
        Task<bool> IsMemberAsync(int userId, int tripId);

        /// <summary>
        /// Get member details
        /// </summary>
        Task<Entity.TripMember.TripMember> GetMemberAsync(int userId, int tripId);

        /// <summary>
        /// Get active members
        /// </summary>
        Task<IEnumerable<Entity.TripMember.TripMember>> GetActiveMembersAsync(int tripId);

        /// <summary>
        /// Update member status
        /// </summary>
        Task UpdateMemberStatusAsync(int memberId, TripMemberStatus status);

        /// <summary>
        /// Get member count for trip
        /// </summary>
        Task<int> GetMemberCountAsync(int tripId);
    }
}
