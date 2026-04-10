namespace DOMAIN_LAYER.Enum
{
    /// <summary>
    /// Enum for Trip Status
    /// </summary>
    public enum TripStatus
    {
        /// <summary>
        /// Trip is planned but not started yet
        /// </summary>
        Planned = 1,

        /// <summary>
        /// Trip is currently ongoing
        /// </summary>
        Ongoing = 2,

        /// <summary>
        /// Trip has been completed
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Trip has been cancelled
        /// </summary>
        Cancelled = 4
    }
}
