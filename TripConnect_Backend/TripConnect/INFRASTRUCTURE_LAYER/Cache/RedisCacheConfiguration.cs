using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Redis Cache Configuration - Settings for Redis connection and behavior
    /// </summary>
    public class RedisCacheConfiguration
    {
        /// <summary>
        /// Redis connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Database number (0-15)
        /// </summary>
        public int DatabaseIndex { get; set; } = 0;

        /// <summary>
        /// Default cache key prefix
        /// </summary>
        public string KeyPrefix { get; set; } = "tripconnect:";

        /// <summary>
        /// Default absolute expiration in seconds (1 hour)
        /// </summary>
        public int DefaultAbsoluteExpirationSeconds { get; set; } = 3600;

        /// <summary>
        /// Default sliding expiration in seconds (30 minutes)
        /// </summary>
        public int DefaultSlidingExpirationSeconds { get; set; } = 1800;

        /// <summary>
        /// Connection timeout in milliseconds
        /// </summary>
        public int ConnectTimeoutMilliseconds { get; set; } = 5000;

        /// <summary>
        /// Enable automatic cleanup of expired entries
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// Cleanup interval in minutes
        /// </summary>
        public int CleanupIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Enable compression for cache values
        /// </summary>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// Create ConfigurationOptions from this configuration
        /// </summary>
        public ConfigurationOptions GetConfigurationOptions()
        {
            var options = ConfigurationOptions.Parse(ConnectionString);
            options.ConnectTimeout = ConnectTimeoutMilliseconds;
            options.DefaultDatabase = DatabaseIndex;
            options.AbortOnConnectFail = false;
            return options;
        }
    }

    /// <summary>
    /// Redis Connection Factory - Manages Redis connections
    /// </summary>
    public static class RedisConnectionFactory
    {
        private static IConnectionMultiplexer _connection;
        private static readonly object _lock = new object();

        /// <summary>
        /// Get or create Redis connection (singleton pattern)
        /// </summary>
        public static IConnectionMultiplexer GetConnection(RedisCacheConfiguration configuration)
        {
            if (_connection != null && _connection.IsConnected)
                return _connection;

            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected)
                    return _connection;

                var options = configuration.GetConfigurationOptions();
                _connection = ConnectionMultiplexer.Connect(options);

                return _connection;
            }
        }

        /// <summary>
        /// Close and dispose the connection
        /// </summary>
        public static void CloseConnection()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
