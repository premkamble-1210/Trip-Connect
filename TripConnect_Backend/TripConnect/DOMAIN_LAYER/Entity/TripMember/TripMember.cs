using DOMAIN_LAYER.Enum;

namespace DOMAIN_LAYER.Entity.TripMember
{
    /// <summary>
    /// TripMember Entity - Represents a member in a trip group
    /// </summary>
    public class TripMember
    {
        /// <summary>
        /// Unique identifier for the trip member
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Trip ID that the member belongs to
        /// </summary>
        public int TripId { get; set; }

        /// <summary>
        /// User ID of the member
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Role of the member in the trip (Host, Member)
        /// </summary>
        public TripMemberRole Role { get; set; }

        /// <summary>
        /// Status of the member in the trip (Active, Left, Removed)
        /// </summary>
        public TripMemberStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the member joined the trip
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Navigation property - User reference (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User User { get; set; }

        /// <summary>
        /// Navigation property - Trip reference (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.Trip.Trip Trip { get; set; }
    }
}
