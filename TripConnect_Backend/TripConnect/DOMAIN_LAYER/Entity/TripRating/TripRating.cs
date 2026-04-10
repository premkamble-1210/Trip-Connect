namespace DOMAIN_LAYER.Entity.TripRating
{
    /// <summary>
    /// TripRating Entity - Represents a rating given for a user in a trip
    /// </summary>
    public class TripRating
    {
        /// <summary>
        /// Unique identifier for the trip rating
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Trip ID this rating is for
        /// </summary>
        public int TripId { get; set; }

        /// <summary>
        /// User ID of the person giving the rating
        /// </summary>
        public int RatedBy { get; set; }

        /// <summary>
        /// User ID of the person being rated
        /// </summary>
        public int RatedUserId { get; set; }

        /// <summary>
        /// Rating score (1.0 to 5.0)
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Review/comment for the rating
        /// </summary>
        public string Review { get; set; }

        /// <summary>
        /// Timestamp when the rating was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property - Trip this rating is for (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.Trip.Trip Trip { get; set; }

        /// <summary>
        /// Navigation property - User who gave the rating (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User RatedByUser { get; set; }

        /// <summary>
        /// Navigation property - User being rated (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User RatedUser { get; set; }
    }
}
