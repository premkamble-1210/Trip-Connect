namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Cache Invalidation Repository Interface - For managing cache invalidation rules and tags
    /// </summary>
    public interface ICacheInvalidationRepository
    {
        /// <summary>
        /// Invalidate cache by key pattern
        /// </summary>
        Task<int> InvalidateByPatternAsync(string pattern);

        /// <summary>
        /// Invalidate cache by tag
        /// </summary>
        Task<int> InvalidateByTagAsync(string tag);

        /// <summary>
        /// Invalidate multiple tags
        /// </summary>
        Task<int> InvalidateByTagsAsync(params string[] tags);

        /// <summary>
        /// Add tag to cache key for invalidation tracking
        /// </summary>
        Task AddTagToCacheAsync(string cacheKey, string tag);

        /// <summary>
        /// Add multiple tags to cache key
        /// </summary>
        Task AddTagsToCacheAsync(string cacheKey, params string[] tags);

        /// <summary>
        /// Get all tags for a cache key
        /// </summary>
        Task<IEnumerable<string>> GetTagsAsync(string cacheKey);

        /// <summary>
        /// Get all cache keys with a specific tag
        /// </summary>
        Task<IEnumerable<string>> GetCacheKeysByTagAsync(string tag);

        /// <summary>
        /// Register invalidation dependency
        /// </summary>
        Task RegisterInvalidationAsync(string cacheKeyPattern, string dependencyPattern);

        /// <summary>
        /// Get all dependent cache patterns
        /// </summary>
        Task<IEnumerable<string>> GetDependentPatternsAsync(string cacheKeyPattern);
    }
}
