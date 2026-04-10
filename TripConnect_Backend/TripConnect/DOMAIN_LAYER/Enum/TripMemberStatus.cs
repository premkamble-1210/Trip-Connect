namespace DOMAIN_LAYER.Enum
{
    /// <summary>
    /// Enum for TripMember Status
    /// </summary>
    public enum TripMemberStatus
    {
        /// <summary>
        /// Member is actively part of the trip
        /// </summary>
        Active = 1,

        /// <summary>
        /// Member left the trip on their own
        /// </summary>
        Left = 2,

        /// <summary>
        /// Member was removed from the trip by host
        /// </summary>
        Removed = 3
    }
}
