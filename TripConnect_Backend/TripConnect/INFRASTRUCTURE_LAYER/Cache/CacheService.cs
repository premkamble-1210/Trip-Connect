using DOMAIN_LAYER.Repository;

namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Cache Service Implementation - High-level API for cache operations
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;
        private readonly ICacheInvalidationRepository _invalidationRepository;
        private readonly ICachePolicyRepository _policyRepository;
        private readonly ICacheEntryRepository _entryRepository;
        private readonly RedisCacheConfiguration _configuration;

        // Statistics tracking
        private long _cacheHits = 0;
        private long _cacheMisses = 0;

        /// <summary>
        /// Constructor - Initializes cache service with repositories
        /// </summary>
        public CacheService(
            ICacheRepository cacheRepository,
            ICacheInvalidationRepository invalidationRepository,
            ICachePolicyRepository policyRepository,
            ICacheEntryRepository entryRepository,
            RedisCacheConfiguration configuration)
        {
            _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
            _invalidationRepository = invalidationRepository ?? throw new ArgumentNullException(nameof(invalidationRepository));
            _policyRepository = policyRepository ?? throw new ArgumentNullException(nameof(policyRepository));
            _entryRepository = entryRepository ?? throw new ArgumentNullException(nameof(entryRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get cached item by key with type
        /// </summary>
        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var fullKey = GetFullCacheKey(key);
                var result = await _cacheRepository.GetAsync<T>(fullKey);

                if (result != null)
                {
                    Interlocked.Increment(ref _cacheHits);
                }
                else
                {
                    Interlocked.Increment(ref _cacheMisses);
                }

                return result;
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"Cache unavailable for key '{key}', falling back to source: {ex.Message}");
                Interlocked.Increment(ref _cacheMisses);
                return default;
            }
        }

        /// <summary>
        /// Set cache item with automatic expiration based on policy
        /// </summary>
        public async Task SetAsync<T>(string key, T value, string policyName = null)
        {
            TimeSpan? expiration = null;

            if (!string.IsNullOrEmpty(policyName))
            {
                var policy = await _policyRepository.GetByPolicyNameAsync(policyName);
                expiration = policy?.GetExpirationTimeSpan();
            }

            if (!expiration.HasValue)
            {
                expiration = TimeSpan.FromSeconds(_configuration.DefaultAbsoluteExpirationSeconds);
            }

            await SetAsync(key, value, expiration);
        }

        /// <summary>
        /// Set cache item with custom expiration
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var fullKey = GetFullCacheKey(key);
                await _cacheRepository.SetAsync(fullKey, value, expiration);
            }
            catch (Exception ex)
            {
                // Log serialization/cache errors but don't throw
                Serilog.Log.Warning($"Failed to set cache for key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Set cache item with tags for group invalidation
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration, params string[] tags)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var fullKey = GetFullCacheKey(key);

                if (!expiration.HasValue)
                {
                    expiration = TimeSpan.FromSeconds(_configuration.DefaultAbsoluteExpirationSeconds);
                }

                await _cacheRepository.SetAsync(fullKey, value, expiration);

                // Add tags
                if (tags != null && tags.Length > 0)
                {
                    await _invalidationRepository.AddTagsToCacheAsync(fullKey, tags);
                }
            }
            catch (Exception ex)
            {
                // Log serialization/cache errors but don't throw
                Serilog.Log.Warning($"Failed to set cache for key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullKey = GetFullCacheKey(key);
            await _cacheRepository.RemoveAsync(fullKey);
        }

        /// <summary>
        /// Remove items by pattern
        /// </summary>
        public async Task<int> RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return 0;

            var fullPattern = GetFullCacheKey(pattern);
            return await _cacheRepository.RemoveByPatternAsync(fullPattern);
        }

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var fullKey = GetFullCacheKey(key);
            return await _cacheRepository.ContainsKeyAsync(fullKey);
        }

        /// <summary>
        /// Get or set cache (if not exists, execute factory and cache result)
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            try
            {
                // Try to get from cache
                var cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                    return cachedValue;
            }
            catch (Exception ex)
            {
                // Log cache retrieval error but continue with factory execution
                Serilog.Log.Warning($"Cache retrieval failed for key '{key}': {ex.Message}. Falling back to database.");
            }

            // If not in cache, execute factory
            var value = await factory();

            // Try to cache the result, but don't fail if caching fails
            if (value != null)
            {
                try
                {
                    if (!expiration.HasValue)
                    {
                        expiration = TimeSpan.FromSeconds(_configuration.DefaultAbsoluteExpirationSeconds);
                    }

                    await SetAsync(key, value, expiration);
                }
                catch (Exception ex)
                {
                    // Log caching error but don't break the request
                    Serilog.Log.Warning($"Failed to cache result for key '{key}': {ex.Message}. Returning data from database.");
                }
            }

            return value;
        }

        /// <summary>
        /// Invalidate cache by tag
        /// </summary>
        public async Task<int> InvalidateByTagAsync(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            return await _invalidationRepository.InvalidateByTagAsync(tag);
        }

        /// <summary>
        /// Invalidate cache by multiple tags
        /// </summary>
        public async Task<int> InvalidateByTagsAsync(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return 0;

            return await _invalidationRepository.InvalidateByTagsAsync(tags);
        }

        /// <summary>
        /// Clear all cache
        /// </summary>
        public async Task ClearAllAsync()
        {
            await _cacheRepository.ClearAllAsync();
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public async Task<CacheServiceStatistics> GetStatisticsAsync()
        {
            var entryStats = await _entryRepository.GetCacheStatisticsAsync();

            return new CacheServiceStatistics
            {
                CacheHits = _cacheHits,
                CacheMisses = _cacheMisses,
                CacheItemCount = entryStats.ActiveEntries,
                CacheSizeBytes = entryStats.CacheSizeBytes
            };
        }

        /// <summary>
        /// Get full cache key with prefix
        /// </summary>
        private string GetFullCacheKey(string key)
        {
            return $"{_configuration.KeyPrefix}{key}";
        }
    }
}
