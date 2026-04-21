namespace DOMAIN_LAYER.Entity.User
{
    /// <summary>
    /// User Entity - Represents a user in the TripConnect platform
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's email address (unique)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username for authentication (unique)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// User's rating/reputation score (0.0 to 5.0)
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Flag indicating if phone number is verified
        /// </summary>
        public bool PhoneVerified { get; set; }

        /// <summary>
        /// Flag indicating if ID/identity is verified
        /// </summary>
        public bool IdVerified { get; set; }

        /// <summary>
        /// Hashed password (never store plain)
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Password salt for secure hashing
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Timestamp when the user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Refresh token for JWT authentication (nullable)
        /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Expiration time of the refresh token
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Flag indicating if the user's token has been blacklisted
        /// </summary>
        public bool IsTokenBlacklisted { get; set; } = false;

    /// <summary>
    /// Navigation property - Trips created by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.Trip.Trip> CreatedTrips { get; set; } = new List<DOMAIN_LAYER.Entity.Trip.Trip>();

    /// <summary>
    /// Navigation property - Trip requests sent by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.TripRequest.TripRequest> TripRequests { get; set; } = new List<DOMAIN_LAYER.Entity.TripRequest.TripRequest>();

    /// <summary>
    /// Navigation property - Trip memberships for this user (M:M via TripMembers)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.TripMember.TripMember> TripMembers { get; set; } = new List<DOMAIN_LAYER.Entity.TripMember.TripMember>();

    /// <summary>
    /// Navigation property - Expenses paid by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.Expense.Expense> PaidExpenses { get; set; } = new List<DOMAIN_LAYER.Entity.Expense.Expense>();

    /// <summary>
    /// Navigation property - Expense splits owed by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.ExpenseSplit.ExpenseSplit> ExpenseSplits { get; set; } = new List<DOMAIN_LAYER.Entity.ExpenseSplit.ExpenseSplit>();

    /// <summary>
    /// Navigation property - Chat messages sent by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.ChatMessage.ChatMessage> ChatMessages { get; set; } = new List<DOMAIN_LAYER.Entity.ChatMessage.ChatMessage>();

    /// <summary>
    /// Navigation property - Ratings given by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.TripRating.TripRating> RatingsGiven { get; set; } = new List<DOMAIN_LAYER.Entity.TripRating.TripRating>();

    /// <summary>
    /// Navigation property - Ratings received by this user (1:M)
    /// </summary>
    public ICollection<DOMAIN_LAYER.Entity.TripRating.TripRating> RatingsReceived { get; set; } = new List<DOMAIN_LAYER.Entity.TripRating.TripRating>();
    }
}