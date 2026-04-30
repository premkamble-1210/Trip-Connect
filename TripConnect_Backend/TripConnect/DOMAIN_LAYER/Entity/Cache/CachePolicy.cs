namespace DOMAIN_LAYER.Entity.Cache
{
    /// <summary>
    /// Represents a cache policy with expiration settings
    /// </summary>
    public class CachePolicy
    {
        /// <summary>
        /// Unique identifier for cache policy
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Policy name/description
        /// </summary>
        public string PolicyName { get; set; }

        /// <summary>
        /// Cache policy type
        /// </summary>
        public Enum.CachePolicyType PolicyType { get; set; }

        /// <summary>
        /// Absolute expiration duration in seconds
        /// </summary>
        public int? AbsoluteExpirationSeconds { get; set; }

        /// <summary>
        /// Sliding expiration duration in seconds
        /// </summary>
        public int? SlidingExpirationSeconds { get; set; }

        /// <summary>
        /// Is this policy active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modified date
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Get the expiration timespan based on policy type
        /// </summary>
        public TimeSpan? GetExpirationTimeSpan()
        {
            return PolicyType switch
            {
                Enum.CachePolicyType.Absolute => AbsoluteExpirationSeconds.HasValue 
                    ? TimeSpan.FromSeconds(AbsoluteExpirationSeconds.Value)
                    : null,
                Enum.CachePolicyType.Sliding => SlidingExpirationSeconds.HasValue
                    ? TimeSpan.FromSeconds(SlidingExpirationSeconds.Value)
                    : null,
                Enum.CachePolicyType.NoExpiration => null,
                _ => null
            };
        }
    }
}
