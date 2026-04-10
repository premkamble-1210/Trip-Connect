namespace DOMAIN_LAYER.Enum
{
    /// <summary>
    /// Enum for TripRequest Status
    /// </summary>
    public enum TripRequestStatus
    {
        /// <summary>
        /// Request is pending organizer's response
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Request has been accepted by the organizer
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// Request has been rejected by the organizer
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Request has been cancelled by the user
        /// </summary>
        Cancelled = 4
    }
}
