using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.Cache;
using Microsoft.EntityFrameworkCore;

namespace INFRASTRUCTURE_LAYER.Repository.Cache
{
    /// <summary>
    /// Cache Entry Repository Implementation - Tracks cache entries in database
    /// </summary>
    public class CacheEntryRepository : Repository<CacheEntry>, ICacheEntryRepository
    {
        /// <summary>
        /// Constructor - Initializes with database context
        /// </summary>
        public CacheEntryRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get cache entry by cache key
        /// </summary>
        public async Task<CacheEntry> GetByCacheKeyAsync(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));

            return await _dbSet.FirstOrDefaultAsync(ce => ce.CacheKey == cacheKey);
        }

        /// <summary>
        /// Get all expired cache entries
        /// </summary>
        public async Task<IEnumerable<CacheEntry>> GetExpiredEntriesAsync()
        {
            return await _dbSet
                .Where(ce => ce.ExpiresAt.HasValue && ce.ExpiresAt.Value <= DateTime.UtcNow)
                .ToListAsync();
        }

        /// <summary>
        /// Get cache entries by data type
        /// </summary>
        public async Task<IEnumerable<CacheEntry>> GetByDataTypeAsync(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                throw new ArgumentNullException(nameof(dataType));

            return await _dbSet
                .Where(ce => ce.DataType == dataType)
                .OrderByDescending(ce => ce.CachedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Delete expired cache entries
        /// </summary>
        public async Task<int> DeleteExpiredEntriesAsync()
        {
            var expiredEntries = await GetExpiredEntriesAsync();
            var count = expiredEntries.Count();

            foreach (var entry in expiredEntries)
            {
                await DeleteAsync(entry.Id);
            }

            await SaveChangesAsync();
            return count;
        }

        /// <summary>
        /// Delete cache entries by pattern
        /// </summary>
        public async Task<int> DeleteByPatternAsync(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return 0;

            var matchingEntries = await _dbSet
                .Where(ce => ce.CacheKey.Contains(pattern))
                .ToListAsync();

            var count = matchingEntries.Count;

            foreach (var entry in matchingEntries)
            {
                _dbSet.Remove(entry);
            }

            await SaveChangesAsync();
            return count;
        }

        /// <summary>
        /// Clear all cache entries
        /// </summary>
        public async Task<int> ClearAllAsync()
        {
            var count = _dbSet.Count();
            _dbSet.RemoveRange(await _dbSet.ToListAsync());
            await SaveChangesAsync();
            return count;
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            var allEntries = await _dbSet.ToListAsync();
            var expiredEntries = allEntries.Where(ce => ce.IsExpired).ToList();
            var activeEntries = allEntries.Where(ce => !ce.IsExpired).ToList();

            var mostAccessedEntry = allEntries.OrderByDescending(ce => ce.AccessCount).FirstOrDefault();
            var totalAccessCount = allEntries.Sum(ce => ce.AccessCount);
            var averageAge = allEntries.Count > 0
                ? allEntries.Average(ce => (DateTime.UtcNow - ce.CachedAt).TotalHours)
                : 0;

            var cacheSizeBytes = allEntries.Sum(ce => 
            {
                try
                {
                    return System.Text.Encoding.UTF8.GetByteCount(ce.CachedData ?? "");
                }
                catch
                {
                    return 0;
                }
            });

            return new CacheStatistics
            {
                TotalEntries = allEntries.Count,
                ExpiredEntries = expiredEntries.Count,
                ActiveEntries = activeEntries.Count,
                TotalAccessCount = totalAccessCount,
                AverageAgeHours = averageAge,
                MostAccessedKey = mostAccessedEntry?.CacheKey,
                CacheSizeBytes = cacheSizeBytes
            };
        }

        /// <summary>
        /// Update cache entry access info
        /// </summary>
        public async Task UpdateAccessInfoAsync(int cacheEntryId)
        {
            var entry = await GetByIdAsync(cacheEntryId);
            if (entry != null)
            {
                entry.UpdateAccessInfo();
                await UpdateAsync(entry);
                await SaveChangesAsync();
            }
        }
    }
}
