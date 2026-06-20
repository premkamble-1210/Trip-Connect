using StackExchange.Redis;
using System.Text.Json;
using DOMAIN_LAYER.Repository;

namespace INFRASTRUCTURE_LAYER.Repository.Cache
{
    /// <summary>
    /// Redis Cache Repository Implementation - Uses Redis for distributed caching
    /// </summary>
    public class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        /// <summary>
        /// Constructor - Initializes Redis connection
        /// </summary>
        public CacheRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _redisDb = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Get cached item by key
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var value = await _redisDb.StringGetAsync(key);

                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            catch (JsonException)
            {
                return default;
            }
            catch (RedisException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis unavailable for GetAsync key '{key}': {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Set cache item with key and value, with optional expiration
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
            {
                await RemoveAsync(key);
                return;
            }

            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                
                if (expiration.HasValue)
                {
                    await _redisDb.StringSetAsync(key, serializedValue, expiration.Value);
                }
                else
                {
                    await _redisDb.StringSetAsync(key, serializedValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis unavailable for SetAsync key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Remove cached item by key
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                await _redisDb.KeyDeleteAsync(key);
            }
            catch (RedisException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis unavailable for RemoveAsync key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Check if cache contains a key
        /// </summary>
        public async Task<bool> ContainsKeyAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                return await _redisDb.KeyExistsAsync(key);
            }
            catch (RedisException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get multiple cache items by keys
        /// </summary>
        public async Task<Dictionary<string, T?>> GetManyAsync<T>(params string[] keys)
        {
            var result = new Dictionary<string, T?>();

            if (keys == null || keys.Length == 0)
                return result;

            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            RedisValue[] values;
            try
            {
                values = await _redisDb.StringGetAsync(redisKeys);
            }
            catch (RedisException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis unavailable for GetManyAsync: {ex.Message}");
                return result;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                if (values[i].HasValue)
                {
                    try
                    {
                        var deserializedValue = JsonSerializer.Deserialize<T>(values[i].ToString());
                        if (deserializedValue != null)
                        {
                            result[keys[i]] = deserializedValue;
                        }
                    }
                    catch
                    {
                        // Skip corrupted values
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove multiple cache items by pattern
        /// </summary>
        public async Task<int> RemoveByPatternAsync(string pattern)
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
        /// Get all keys matching a pattern
        /// </summary>
        public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            return server.Keys(pattern: pattern).Select(k => k.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Clear all cache
        /// </summary>
        public async Task ClearAllAsync()
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            await server.FlushDatabaseAsync();
        }

        /// <summary>
        /// Get cache server info
        /// </summary>
        public string GetServerInfo()
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            return server.Info().ToString();
        }
    }
}
