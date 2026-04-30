using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Cache Warming Service - Preloads frequently accessed data at startup
    /// </summary>
    public interface ICacheWarmingService
    {
        /// <summary>
        /// Warm up cache with commonly accessed data
        /// </summary>
        Task WarmupCacheAsync();

        /// <summary>
        /// Warm up specific entity type cache
        /// </summary>
        Task<int> WarmupEntityCacheAsync(string entityType);

        /// <summary>
        /// Get cache warming status
        /// </summary>
        Task<CacheWarmingStatus> GetStatusAsync();
    }

    /// <summary>
    /// Cache warming status information
    /// </summary>
    public class CacheWarmingStatus
    {
        /// <summary>
        /// Is cache currently warming up
        /// </summary>
        public bool IsWarming { get; set; }

        /// <summary>
        /// Timestamp when warming started
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Timestamp when warming completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Duration of warmup in milliseconds
        /// </summary>
        public long DurationMilliseconds { get; set; }

        /// <summary>
        /// Number of entries warmed
        /// </summary>
        public int EntriesWarmed { get; set; }

        /// <summary>
        /// Error messages if any
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Warmup strategy used
        /// </summary>
        public string Strategy { get; set; }
    }
}
