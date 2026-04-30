namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Cache Repository Interface - For caching frequently accessed data
    /// </summary>
    public interface ICacheRepository
    {
        /// <summary>
        /// Get cached item by key
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Set cache item with key and value, with optional expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Remove cached item by key
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Check if cache contains a key
        /// </summary>
        Task<bool> ContainsKeyAsync(string key);

        /// <summary>
        /// Remove multiple cache items by pattern
        /// </summary>
        Task<int> RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Clear all cache
        /// </summary>
        Task ClearAllAsync();
    }
}