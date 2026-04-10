using DOMAIN_LAYER.Enum;

namespace DOMAIN_LAYER.Entity.TripRequest
{
    /// <summary>
    /// TripRequest Entity - Represents a join request for a trip
    /// </summary>
    public class TripRequest
    {
        /// <summary>
        /// Unique identifier for the trip request
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Trip ID that the user is requesting to join
        /// </summary>
        public int TripId { get; set; }

        /// <summary>
        /// User ID of the person requesting to join
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Status of the request (Pending, Accepted, Rejected, Cancelled)
        /// </summary>
        public TripRequestStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the join request was created
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Timestamp when the organizer responded to the request
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Navigation property - User who sent the request (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User User { get; set; }

        /// <summary>
        /// Navigation property - Trip for which request is sent (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.Trip.Trip Trip { get; set; }
    }
}
