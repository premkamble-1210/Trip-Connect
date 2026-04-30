using DOMAIN_LAYER.Repository;
using System.Text;

namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Cache Statistics Service Implementation - Monitors and reports cache performance metrics
    /// </summary>
    public class CacheStatisticsService : ICacheStatisticsService
    {
        private readonly ICacheEntryRepository _cacheEntryRepository;
        private readonly ICacheService _cacheService;
        private long _totalHits = 0;
        private long _totalMisses = 0;

        /// <summary>
        /// Constructor - Initializes with repositories
        /// </summary>
        public CacheStatisticsService(
            ICacheEntryRepository cacheEntryRepository,
            ICacheService cacheService)
        {
            _cacheEntryRepository = cacheEntryRepository ?? throw new ArgumentNullException(nameof(cacheEntryRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        /// <summary>
        /// Get comprehensive cache statistics
        /// </summary>
        public async Task<DetailedCacheStatistics> GetDetailedStatisticsAsync()
        {
            var cacheStats = await _cacheEntryRepository.GetCacheStatisticsAsync();
            var cacheServiceStats = await _cacheService.GetStatisticsAsync();

            var allEntries = await _cacheEntryRepository.GetAllAsync();
            var entriesList = allEntries.ToList();

            // Group by data type
            var byDataType = entriesList
                .GroupBy(e => e.DataType)
                .ToDictionary(g => g.Key, g => g.Count());

            var memoryPressure = CalculateMemoryPressure(cacheStats.CacheSizeBytes);

            return new DetailedCacheStatistics
            {
                TotalEntries = cacheStats.TotalEntries,
                ActiveEntries = cacheStats.ActiveEntries,
                ExpiredEntries = cacheStats.ExpiredEntries,
                TotalHits = cacheServiceStats.CacheHits,
                TotalMisses = cacheServiceStats.CacheMisses,
                TotalSizeBytes = cacheStats.CacheSizeBytes,
                MostAccessedKey = cacheStats.MostAccessedKey,
                AverageEntryAgeHours = cacheStats.AverageAgeHours,
                EntriesByDataType = byDataType,
                MemoryPressurePercent = memoryPressure
            };
        }

        /// <summary>
        /// Get cache hit rate percentage
        /// </summary>
        public async Task<double> GetCacheHitRateAsync()
        {
            var stats = await _cacheService.GetStatisticsAsync();
            var total = stats.CacheHits + stats.CacheMisses;
            return total > 0 ? (double)stats.CacheHits / total * 100 : 0;
        }

        /// <summary>
        /// Get cache size in bytes
        /// </summary>
        public async Task<long> GetCacheSizeAsync()
        {
            var stats = await _cacheEntryRepository.GetCacheStatisticsAsync();
            return stats.CacheSizeBytes;
        }

        /// <summary>
        /// Get number of active cache entries
        /// </summary>
        public async Task<int> GetActiveCacheCountAsync()
        {
            var stats = await _cacheEntryRepository.GetCacheStatisticsAsync();
            return stats.ActiveEntries;
        }

        /// <summary>
        /// Get most accessed cache keys
        /// </summary>
        public async Task<IEnumerable<CacheKeyAccess>> GetMostAccessedKeysAsync(int top = 10)
        {
            var allEntries = await _cacheEntryRepository.GetAllAsync();
            
            return allEntries
                .OrderByDescending(e => e.AccessCount)
                .Take(top)
                .Select(e => new CacheKeyAccess
                {
                    CacheKey = e.CacheKey,
                    DataType = e.DataType,
                    AccessCount = e.AccessCount,
                    LastAccessedAt = e.LastAccessedAt,
                    SizeBytes = Encoding.UTF8.GetByteCount(e.CachedData ?? ""),
                    ExpiresAt = e.ExpiresAt
                })
                .ToList();
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public async Task ResetStatisticsAsync()
        {
            _totalHits = 0;
            _totalMisses = 0;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Calculate memory pressure based on cache size
        /// </summary>
        private int CalculateMemoryPressure(long cacheSizeBytes)
        {
            // Assuming max cache size of 500MB, adjust as needed
            const long maxCacheSize = 500 * 1024 * 1024;
            
            if (cacheSizeBytes <= 0)
                return 0;

            var pressure = (cacheSizeBytes * 100) / maxCacheSize;
            return (int)Math.Min(pressure, 100);
        }
    }
}
