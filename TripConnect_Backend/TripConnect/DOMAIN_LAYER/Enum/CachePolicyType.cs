namespace DOMAIN_LAYER.Enum
{
    /// <summary>
    /// Enum for different cache policy types
    /// </summary>
    public enum CachePolicyType
    {
        /// <summary>
        /// Cache with absolute expiration (expires at specific time)
        /// </summary>
        Absolute = 0,

        /// <summary>
        /// Cache with sliding expiration (resets on access)
        /// </summary>
        Sliding = 1,

        /// <summary>
        /// Cache without expiration (permanent)
        /// </summary>
        NoExpiration = 2,

        /// <summary>
        /// Cache expires on specific event
        /// </summary>
        EventBased = 3
    }
}
