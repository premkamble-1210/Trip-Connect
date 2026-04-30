namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Cache Entry Repository Interface - For managing cache entries tracking
    /// </summary>
    public interface ICacheEntryRepository : IRepository<Entity.Cache.CacheEntry>
    {
        /// <summary>
        /// Get cache entry by cache key
        /// </summary>
        Task<Entity.Cache.CacheEntry> GetByCacheKeyAsync(string cacheKey);

        /// <summary>
        /// Get all expired cache entries
        /// </summary>
        Task<IEnumerable<Entity.Cache.CacheEntry>> GetExpiredEntriesAsync();

        /// <summary>
        /// Get cache entries by data type
        /// </summary>
        Task<IEnumerable<Entity.Cache.CacheEntry>> GetByDataTypeAsync(string dataType);

        /// <summary>
        /// Delete expired cache entries
        /// </summary>
        Task<int> DeleteExpiredEntriesAsync();

        /// <summary>
        /// Delete cache entries by pattern
        /// </summary>
        Task<int> DeleteByPatternAsync(string pattern);

        /// <summary>
        /// Clear all cache entries
        /// </summary>
        Task<int> ClearAllAsync();

        /// <summary>
        /// Get cache statistics
        /// </summary>
        Task<CacheStatistics> GetCacheStatisticsAsync();

        /// <summary>
        /// Update cache entry access info
        /// </summary>
        Task UpdateAccessInfoAsync(int cacheEntryId);
    }

    /// <summary>
    /// Cache statistics model
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// Total number of cache entries
        /// </summary>
        public int TotalEntries { get; set; }

        /// <summary>
        /// Number of expired entries
        /// </summary>
        public int ExpiredEntries { get; set; }

        /// <summary>
        /// Number of active entries
        /// </summary>
        public int ActiveEntries { get; set; }

        /// <summary>
        /// Total access count across all entries
        /// </summary>
        public long TotalAccessCount { get; set; }

        /// <summary>
        /// Average cache entry age in hours
        /// </summary>
        public double AverageAgeHours { get; set; }

        /// <summary>
        /// Most accessed cache key
        /// </summary>
        public string MostAccessedKey { get; set; }

        /// <summary>
        /// Cache size in bytes
        /// </summary>
        public long CacheSizeBytes { get; set; }
    }
}
