using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Background service for cleaning up expired cache entries
    /// </summary>
    public class CacheCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RedisCacheConfiguration _configuration;
        private Timer _cleanupTimer;

        /// <summary>
        /// Constructor - Initializes the cleanup service
        /// </summary>
        public CacheCleanupBackgroundService(
            IServiceProvider serviceProvider,
            RedisCacheConfiguration configuration)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Execute the background service
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_configuration.EnableAutoCleanup)
            {
                Debug.WriteLine("Cache cleanup service is disabled");
                return;
            }

            Debug.WriteLine($"Cache cleanup service starting - cleanup interval: {_configuration.CleanupIntervalMinutes} minutes");

            _cleanupTimer = new Timer(
                callback: async _ => await CleanupExpiredEntriesAsync(stoppingToken),
                state: null,
                dueTime: TimeSpan.FromMinutes(_configuration.CleanupIntervalMinutes),
                period: TimeSpan.FromMinutes(_configuration.CleanupIntervalMinutes)
            );

            await Task.CompletedTask;
        }

        /// <summary>
        /// Cleanup expired cache entries
        /// </summary>
        private async Task CleanupExpiredEntriesAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cacheEntryRepository = scope.ServiceProvider.GetRequiredService<ICacheEntryRepository>();
                    var cacheRepository = scope.ServiceProvider.GetRequiredService<ICacheRepository>();

                    // Get all expired entries
                    var expiredEntries = await cacheEntryRepository.GetExpiredEntriesAsync();
                    var expiredList = expiredEntries.ToList();

                    if (expiredList.Count > 0)
                    {
                        // Remove from Redis
                        foreach (var entry in expiredList)
                        {
                            await cacheRepository.RemoveAsync(entry.CacheKey);
                        }

                        // Remove from database
                        int deletedCount = await cacheEntryRepository.DeleteExpiredEntriesAsync();
                        Debug.WriteLine($"Cleanup completed: {deletedCount} expired cache entries removed");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cache cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the background service
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cleanupTimer?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public override void Dispose()
        {
            _cleanupTimer?.Dispose();
            base.Dispose();
        }
    }
}
