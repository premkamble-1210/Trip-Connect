using DOMAIN_LAYER.Repository;
using StackExchange.Redis;

namespace INFRASTRUCTURE_LAYER.Repository.Cache
{
    /// <summary>
    /// Cache Invalidation Repository Implementation - Manages cache invalidation rules and tags
    /// </summary>
    public class CacheInvalidationRepository : ICacheInvalidationRepository
    {
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private const string CACHE_TAG_PREFIX = "tag:";
        private const string CACHE_KEY_TAGS_PREFIX = "key_tags:";
        private const string INVALIDATION_DEPENDENCY_PREFIX = "invalidation:";

        /// <summary>
        /// Constructor - Initializes Redis connection for invalidation management
        /// </summary>
        public CacheInvalidationRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _redisDb = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Invalidate cache by key pattern
        /// </summary>
        public async Task<int> InvalidateByPatternAsync(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return 0;

            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);

            int count = 0;
            foreach (var key in keys)
            {
                if (await _redisDb.KeyDeleteAsync(key))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Invalidate cache by tag
        /// </summary>
        public async Task<int> InvalidateByTagAsync(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return 0;

            var tagKey = $"{CACHE_TAG_PREFIX}{tag}";
            var cacheKeys = await _redisDb.SetMembersAsync(tagKey);

            int count = 0;
            foreach (var cacheKey in cacheKeys)
            {
                if (await _redisDb.KeyDeleteAsync((RedisKey)cacheKey.ToString()))
                    count++;
            }

            // Remove the tag key itself
            await _redisDb.KeyDeleteAsync(tagKey);

            return count;
        }

        /// <summary>
        /// Invalidate multiple tags
        /// </summary>
        public async Task<int> InvalidateByTagsAsync(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return 0;

            int totalCount = 0;

            foreach (var tag in tags)
            {
                totalCount += await InvalidateByTagAsync(tag);
            }

            return totalCount;
        }

        /// <summary>
        /// Add tag to cache key for invalidation tracking
        /// </summary>
        public async Task AddTagToCacheAsync(string cacheKey, string tag)
        {
            if (string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(tag))
                throw new ArgumentNullException();

            // Add cache key to tag set
            var tagKey = $"{CACHE_TAG_PREFIX}{tag}";
            await _redisDb.SetAddAsync(tagKey, cacheKey);

            // Add tag to cache key's tags
            var keyTagsKey = $"{CACHE_KEY_TAGS_PREFIX}{cacheKey}";
            await _redisDb.SetAddAsync(keyTagsKey, tag);

            // Set expiration for tag key (1 year)
            await _redisDb.KeyExpireAsync(tagKey, TimeSpan.FromDays(365));
            await _redisDb.KeyExpireAsync(keyTagsKey, TimeSpan.FromDays(365));
        }

        /// <summary>
        /// Add multiple tags to cache key
        /// </summary>
        public async Task AddTagsToCacheAsync(string cacheKey, params string[] tags)
        {
            if (string.IsNullOrEmpty(cacheKey) || tags == null || tags.Length == 0)
                throw new ArgumentNullException();

            foreach (var tag in tags)
            {
                await AddTagToCacheAsync(cacheKey, tag);
            }
        }

        /// <summary>
        /// Get all tags for a cache key
        /// </summary>
        public async Task<IEnumerable<string>> GetTagsAsync(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));

            var keyTagsKey = $"{CACHE_KEY_TAGS_PREFIX}{cacheKey}";
            var tags = await _redisDb.SetMembersAsync(keyTagsKey);

            return tags.Select(t => t.ToString());
        }

        /// <summary>
        /// Get all cache keys with a specific tag
        /// </summary>
        public async Task<IEnumerable<string>> GetCacheKeysByTagAsync(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            var tagKey = $"{CACHE_TAG_PREFIX}{tag}";
            var cacheKeys = await _redisDb.SetMembersAsync(tagKey);

            return cacheKeys.Select(k => k.ToString());
        }

        /// <summary>
        /// Register invalidation dependency
        /// </summary>
        public async Task RegisterInvalidationAsync(string cacheKeyPattern, string dependencyPattern)
        {
            if (string.IsNullOrEmpty(cacheKeyPattern) || string.IsNullOrEmpty(dependencyPattern))
                throw new ArgumentNullException();

            var dependencyKey = $"{INVALIDATION_DEPENDENCY_PREFIX}{cacheKeyPattern}";
            await _redisDb.SetAddAsync(dependencyKey, dependencyPattern);
            await _redisDb.KeyExpireAsync(dependencyKey, TimeSpan.FromDays(365));
        }

        /// <summary>
        /// Get all dependent cache patterns
        /// </summary>
        public async Task<IEnumerable<string>> GetDependentPatternsAsync(string cacheKeyPattern)
        {
            if (string.IsNullOrEmpty(cacheKeyPattern))
                throw new ArgumentNullException(nameof(cacheKeyPattern));

            var dependencyKey = $"{INVALIDATION_DEPENDENCY_PREFIX}{cacheKeyPattern}";
            var patterns = await _redisDb.SetMembersAsync(dependencyKey);

            return patterns.Select(p => p.ToString());
        }
    }
}
