using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using System.Diagnostics;

namespace INFRASTRUCTURE_LAYER.Cache
{
    /// <summary>
    /// Cache Warming Service Implementation - Preloads frequently accessed data at startup
    /// </summary>
    public class CacheWarmingService : ICacheWarmingService
    {
        private readonly ICacheRepository _cacheRepository;
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;
        private CacheWarmingStatus _status;

        /// <summary>
        /// Constructor - Initializes with repositories
        /// </summary>
        public CacheWarmingService(
            ICacheRepository cacheRepository,
            ICacheService cacheService,
            IUserRepository userRepository,
            ITripRepository tripRepository)
        {
            _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));

            _status = new CacheWarmingStatus
            {
                IsWarming = false,
                Strategy = "Lazy Loading with On-Demand Warming"
            };
        }

        /// <summary>
        /// Warm up cache with commonly accessed data
        /// </summary>
        public async Task WarmupCacheAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _status.IsWarming = true;
            _status.StartedAt = DateTime.UtcNow;
            _status.EntriesWarmed = 0;
            _status.Errors.Clear();

            try
            {
                Debug.WriteLine("Starting cache warmup...");

                // Warm up users (top active users)
                var userCount = await WarmupEntityCacheAsync("User");
                _status.EntriesWarmed += userCount;

                // Warm up active trips
                var tripCount = await WarmupEntityCacheAsync("Trip");
                _status.EntriesWarmed += tripCount;

                // Warm up cache policies
                var policyCount = await WarmupCachePoliciesAsync();
                _status.EntriesWarmed += policyCount;

                stopwatch.Stop();
                _status.CompletedAt = DateTime.UtcNow;
                _status.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
                _status.IsWarming = false;

                Debug.WriteLine($"Cache warmup completed in {stopwatch.ElapsedMilliseconds}ms. {_status.EntriesWarmed} entries warmed.");
            }
            catch (Exception ex)
            {
                _status.IsWarming = false;
                _status.Errors.Add($"Warmup error: {ex.Message}");
                Debug.WriteLine($"Cache warmup error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Warm up specific entity type cache
        /// </summary>
        public async Task<int> WarmupEntityCacheAsync(string entityType)
        {
            int count = 0;

            try
            {
                switch (entityType.ToLower())
                {
                    case "user":
                        count = await WarmupUsersAsync();
                        break;

                    case "trip":
                        count = await WarmupTripsAsync();
                        break;

                    default:
                        Debug.WriteLine($"Unknown entity type for warming: {entityType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _status.Errors.Add($"Error warming {entityType}: {ex.Message}");
                Debug.WriteLine($"Error warming {entityType}: {ex.Message}");
            }

            return count;
        }

        /// <summary>
        /// Get cache warming status
        /// </summary>
        public async Task<CacheWarmingStatus> GetStatusAsync()
        {
            return await Task.FromResult(_status);
        }

        /// <summary>
        /// Warm up users cache
        /// </summary>
        private async Task<int> WarmupUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userList = users.ToList();

                foreach (var user in userList.Take(100)) // Limit to top 100 users
                {
                    var key = string.Format(CacheKeyConstants.USER_BY_ID, user.Id);
                    await _cacheService.SetAsync(key, user, TimeSpan.FromHours(2), "user:warmup", "user:all");
                }

                Debug.WriteLine($"Warmed up {Math.Min(userList.Count, 100)} user cache entries");
                return Math.Min(userList.Count, 100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error warming users: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Warm up trips cache
        /// </summary>
        private async Task<int> WarmupTripsAsync()
        {
            try
            {
                var trips = await _tripRepository.GetAllAsync();
                var tripList = trips.ToList();

                foreach (var trip in tripList.Take(50)) // Limit to top 50 trips
                {
                    var key = string.Format(CacheKeyConstants.TRIP_BY_ID, trip.Id);
                    await _cacheService.SetAsync(key, trip, TimeSpan.FromHours(1), "trip:warmup", "trip:all");
                }

                Debug.WriteLine($"Warmed up {Math.Min(tripList.Count, 50)} trip cache entries");
                return Math.Min(tripList.Count, 50);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error warming trips: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Warm up cache policies
        /// </summary>
        private async Task<int> WarmupCachePoliciesAsync()
        {
            try
            {
                var policies = new[]
                {
                    new DOMAIN_LAYER.Entity.Cache.CachePolicy
                    {
                        PolicyName = "DefaultPolicy",
                        PolicyType = DOMAIN_LAYER.Enum.CachePolicyType.Absolute,
                        AbsoluteExpirationSeconds = 3600,
                        IsActive = true
                    },
                    new DOMAIN_LAYER.Entity.Cache.CachePolicy
                    {
                        PolicyName = "ShortLivedPolicy",
                        PolicyType = DOMAIN_LAYER.Enum.CachePolicyType.Sliding,
                        SlidingExpirationSeconds = 300,
                        IsActive = true
                    },
                    new DOMAIN_LAYER.Entity.Cache.CachePolicy
                    {
                        PolicyName = "PermanentPolicy",
                        PolicyType = DOMAIN_LAYER.Enum.CachePolicyType.NoExpiration,
                        IsActive = true
                    }
                };

                foreach (var policy in policies)
                {
                    var key = $"cachepolicy:{policy.PolicyName}";
                    await _cacheRepository.SetAsync(key, policy, TimeSpan.FromDays(7));
                }

                Debug.WriteLine($"Warmed up {policies.Length} cache policy entries");
                return policies.Length;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error warming policies: {ex.Message}");
                return 0;
            }
        }
    }
}
