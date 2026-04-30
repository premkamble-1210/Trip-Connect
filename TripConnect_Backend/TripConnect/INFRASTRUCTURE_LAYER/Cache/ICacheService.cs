namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Cache Service Interface - High-level API for cache operations
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get cached item by key with type
        /// </summary>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Set cache item with automatic expiration based on policy
        /// </summary>
        Task SetAsync<T>(string key, T value, string policyName = null);

        /// <summary>
        /// Set cache item with custom expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// Set cache item with tags for group invalidation
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration, params string[] tags);

        /// <summary>
        /// Remove item from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Remove items by pattern
        /// </summary>
        Task<int> RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Get or set cache (if not exists, execute factory and cache result)
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Invalidate cache by tag
        /// </summary>
        Task<int> InvalidateByTagAsync(string tag);

        /// <summary>
        /// Invalidate cache by multiple tags
        /// </summary>
        Task<int> InvalidateByTagsAsync(params string[] tags);

        /// <summary>
        /// Clear all cache
        /// </summary>
        Task ClearAllAsync();

        /// <summary>
        /// Get cache statistics
        /// </summary>
        Task<CacheServiceStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Cache service statistics
    /// </summary>
    public class CacheServiceStatistics
    {
        /// <summary>
        /// Total cache hits
        /// </summary>
        public long CacheHits { get; set; }

        /// <summary>
        /// Total cache misses
        /// </summary>
        public long CacheMisses { get; set; }

        /// <summary>
        /// Hit rate percentage
        /// </summary>
        public double HitRate => CacheHits + CacheMisses > 0
            ? (double)CacheHits / (CacheHits + CacheMisses) * 100
            : 0;

        /// <summary>
        /// Number of cache items
        /// </summary>
        public int CacheItemCount { get; set; }

        /// <summary>
        /// Cache size in bytes
        /// </summary>
        public long CacheSizeBytes { get; set; }
    }
}
