namespace DOMAIN_LAYER.Entity.Cache
{
    /// <summary>
    /// Represents a cached entry
    /// </summary>
    public class CacheEntry
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cache key
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// Cached data (serialized)
        /// </summary>
        public string CachedData { get; set; }

        /// <summary>
        /// Data type that was cached
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Expiration time (null if no expiration)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// When the entry was cached
        /// </summary>
        public DateTime CachedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Number of times this cache entry was accessed
        /// </summary>
        public long AccessCount { get; set; } = 0;

        /// <summary>
        /// Last time the cache entry was accessed
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }

        /// <summary>
        /// Cache policy associated with this entry
        /// </summary>
        public int? CachePolicyId { get; set; }

        /// <summary>
        /// Navigation property for cache policy
        /// </summary>
        public CachePolicy CachePolicy { get; set; }

        /// <summary>
        /// Check if cache entry has expired
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

        /// <summary>
        /// Update last access time
        /// </summary>
        public void UpdateAccessInfo()
        {
            LastAccessedAt = DateTime.UtcNow;
            AccessCount++;
        }
    }
}
