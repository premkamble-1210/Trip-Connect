using DOMAIN_LAYER.Enum;

namespace DOMAIN_LAYER.Entity.Trip
{
    /// <summary>
    /// Trip Entity - Represents a trip posting in the TripConnect platform
    /// </summary>
    public class Trip
    {
        /// <summary>
        /// Unique identifier for the trip
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title of the trip
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the trip
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Location/destination of the trip
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Budget for the trip
        /// </summary>
        public decimal Budget { get; set; }

        /// <summary>
        /// Start date of the trip
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the trip
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Number of available seats in the trip
        /// </summary>
        public int Seats { get; set; }

        /// <summary>
        /// Type of travel (e.g., Trekking, Beach, Biking, etc.)
        /// </summary>
        public string TravelType { get; set; }

        /// <summary>
        /// Status of the trip (Planned, Ongoing, Completed, Cancelled)
        /// </summary>
        public TripStatus Status { get; set; }

        /// <summary>
        /// User ID of the trip host/organizer
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        /// Timestamp when the trip was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property - Trip host/organizer (1:1 relationship)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User Host { get; set; }

        /// <summary>
        /// Navigation property - Trip join requests (1:M)
        /// </summary>
        public ICollection<DOMAIN_LAYER.Entity.TripRequest.TripRequest> TripRequests { get; set; } = new List<DOMAIN_LAYER.Entity.TripRequest.TripRequest>();

        /// <summary>
        /// Navigation property - Trip members (1:M via TripMembers)
        /// </summary>
        public ICollection<DOMAIN_LAYER.Entity.TripMember.TripMember> TripMembers { get; set; } = new List<DOMAIN_LAYER.Entity.TripMember.TripMember>();

        /// <summary>
        /// Navigation property - Trip expenses (1:M)
        /// </summary>
        public ICollection<DOMAIN_LAYER.Entity.Expense.Expense> Expenses { get; set; } = new List<DOMAIN_LAYER.Entity.Expense.Expense>();

        /// <summary>
        /// Navigation property - Chat messages for this trip (1:M)
        /// </summary>
        public ICollection<DOMAIN_LAYER.Entity.ChatMessage.ChatMessage> ChatMessages { get; set; } = new List<DOMAIN_LAYER.Entity.ChatMessage.ChatMessage>();

        /// <summary>
        /// Navigation property - Ratings given for this trip (1:M)
        /// </summary>
        public ICollection<DOMAIN_LAYER.Entity.TripRating.TripRating> Ratings { get; set; } = new List<DOMAIN_LAYER.Entity.TripRating.TripRating>();
    }
}
