using DOMAIN_LAYER.Repository;

namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Cache Statistics Service - Monitors and reports cache performance metrics
    /// </summary>
    public interface ICacheStatisticsService
    {
        /// <summary>
        /// Get comprehensive cache statistics
        /// </summary>
        Task<DetailedCacheStatistics> GetDetailedStatisticsAsync();

        /// <summary>
        /// Get cache hit rate percentage
        /// </summary>
        Task<double> GetCacheHitRateAsync();

        /// <summary>
        /// Get cache size in bytes
        /// </summary>
        Task<long> GetCacheSizeAsync();

        /// <summary>
        /// Get number of active cache entries
        /// </summary>
        Task<int> GetActiveCacheCountAsync();

        /// <summary>
        /// Get most accessed cache keys
        /// </summary>
        Task<IEnumerable<CacheKeyAccess>> GetMostAccessedKeysAsync(int top = 10);

        /// <summary>
        /// Reset statistics
        /// </summary>
        Task ResetStatisticsAsync();
    }

    /// <summary>
    /// Detailed cache statistics model
    /// </summary>
    public class DetailedCacheStatistics
    {
        /// <summary>
        /// Total cache entries
        /// </summary>
        public int TotalEntries { get; set; }

        /// <summary>
        /// Active (non-expired) entries
        /// </summary>
        public int ActiveEntries { get; set; }

        /// <summary>
        /// Expired entries
        /// </summary>
        public int ExpiredEntries { get; set; }

        /// <summary>
        /// Total cache hits
        /// </summary>
        public long TotalHits { get; set; }

        /// <summary>
        /// Total cache misses
        /// </summary>
        public long TotalMisses { get; set; }

        /// <summary>
        /// Hit rate percentage
        /// </summary>
        public double HitRatePercentage => TotalHits + TotalMisses > 0
            ? (double)TotalHits / (TotalHits + TotalMisses) * 100
            : 0;

        /// <summary>
        /// Total cache size in bytes
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// Average entry size in bytes
        /// </summary>
        public long AverageEntrySizeBytes => ActiveEntries > 0 ? TotalSizeBytes / ActiveEntries : 0;

        /// <summary>
        /// Most accessed cache key
        /// </summary>
        public string MostAccessedKey { get; set; }

        /// <summary>
        /// Average cache entry age in hours
        /// </summary>
        public double AverageEntryAgeHours { get; set; }

        /// <summary>
        /// Timestamp when statistics were generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Cache policies statistics
        /// </summary>
        public Dictionary<string, int> EntriesByDataType { get; set; } = new();

        /// <summary>
        /// Memory pressure percentage (0-100)
        /// </summary>
        public int MemoryPressurePercent { get; set; }
    }

    /// <summary>
    /// Cache key access information
    /// </summary>
    public class CacheKeyAccess
    {
        /// <summary>
        /// Cache key
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// Data type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Number of times accessed
        /// </summary>
        public long AccessCount { get; set; }

        /// <summary>
        /// Last accessed time
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }

        /// <summary>
        /// Entry size in bytes
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Expiration time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Is this entry expired
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    }
}
