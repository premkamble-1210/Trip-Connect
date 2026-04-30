# Redis Cache Implementation Guide

## Overview
This document provides comprehensive guidance on using the Redis cache infrastructure implemented in the TripConnect application.

## Architecture

### Layer Structure
```
DOMAIN_LAYER
├── Enum/
│   ├── CachePolicyType.cs (Absolute, Sliding, NoExpiration, EventBased)
│   └── CacheKeyConstants.cs (Predefined cache keys for all entities)
├── Entity/Cache/
│   ├── CachePolicy.cs (Cache policy configuration)
│   └── CacheEntry.cs (Cache entry tracking)
└── Repository/
    ├── ICacheRepository.cs (Basic cache operations)
    ├── ICachePolicyRepository.cs (Policy management)
    ├── ICacheEntryRepository.cs (Entry tracking)
    └── ICacheInvalidationRepository.cs (Invalidation management)

INFRASTRUCTURE_LAYER
├── Repository/Cache/
│   ├── CacheRepository.cs (Redis implementation)
│   ├── CachePolicyRepository.cs (EF Core implementation)
│   ├── CacheEntryRepository.cs (EF Core implementation)
│   └── CacheInvalidationRepository.cs (Redis implementation)
├── Cache/
│   ├── RedisCacheConfiguration.cs (Configuration & connection factory)
│   ├── ICacheService.cs (High-level cache API interface)
│   └── CacheService.cs (High-level cache API implementation)
└── DependencyInjection.cs (Dependency registration)
```

## Configuration

### appsettings.json
```json
"RedisCache": {
  "ConnectionString": "localhost:6379",
  "DatabaseIndex": 0,
  "KeyPrefix": "tripconnect:",
  "DefaultAbsoluteExpirationSeconds": 3600,
  "DefaultSlidingExpirationSeconds": 1800,
  "ConnectTimeoutMilliseconds": 5000,
  "EnableAutoCleanup": true,
  "CleanupIntervalMinutes": 60,
  "EnableCompression": false
}
```

**Configuration Options:**
- `ConnectionString`: Redis server address (host:port)
- `DatabaseIndex`: Redis database number (0-15)
- `KeyPrefix`: Prefix for all cache keys
- `DefaultAbsoluteExpirationSeconds`: Default absolute expiration (1 hour)
- `DefaultSlidingExpirationSeconds`: Default sliding expiration (30 minutes)
- `ConnectTimeoutMilliseconds`: Connection timeout (5 seconds)
- `EnableAutoCleanup`: Enable automatic cleanup of expired entries
- `CleanupIntervalMinutes`: Cleanup task interval (60 minutes)
- `EnableCompression`: Enable value compression (false by default)

## Usage Patterns

### 1. Basic Cache Operations

#### Using ICacheRepository (Low-level)
```csharp
public class UserService
{
    private readonly ICacheRepository _cacheRepository;

    public UserService(ICacheRepository cacheRepository)
    {
        _cacheRepository = cacheRepository;
    }

    // Get from cache
    public async Task<User> GetUserAsync(int userId)
    {
        var cacheKey = $"user:id:{userId}";
        var cachedUser = await _cacheRepository.GetAsync<User>(cacheKey);
        
        if (cachedUser != null)
            return cachedUser;

        // Fetch from database
        var user = await _repository.GetByIdAsync(userId);
        
        // Cache for 1 hour
        if (user != null)
        {
            await _cacheRepository.SetAsync(
                cacheKey, 
                user, 
                TimeSpan.FromHours(1)
            );
        }

        return user;
    }

    // Remove from cache
    public async Task UpdateUserAsync(int userId, User user)
    {
        await _repository.UpdateAsync(user);
        
        var cacheKey = $"user:id:{userId}";
        await _cacheRepository.RemoveAsync(cacheKey);
    }
}
```

#### Using ICacheService (High-level)
```csharp
public class UserService
{
    private readonly ICacheService _cacheService;

    public UserService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    // Get or Set pattern
    public async Task<User> GetUserAsync(int userId)
    {
        return await _cacheService.GetOrSetAsync(
            $"user:id:{userId}",
            async () => await _repository.GetByIdAsync(userId),
            TimeSpan.FromHours(1)
        );
    }

    // Set with tags
    public async Task CreateUserAsync(User user)
    {
        await _repository.AddAsync(user);
        
        await _cacheService.SetAsync(
            $"user:id:{user.Id}",
            user,
            TimeSpan.FromHours(1),
            "user:created",
            "user:all"
        );
    }
}
```

### 2. Cache Invalidation

#### By Pattern
```csharp
// Invalidate all trip caches
await _cacheService.RemoveByPatternAsync("trip:*");

// Invalidate trip-specific caches
await _cacheService.RemoveByPatternAsync("trip:123:*");
```

#### By Tag
```csharp
// Invalidate all user-related caches
await _cacheService.InvalidateByTagAsync("user:all");

// Invalidate multiple tags
await _cacheService.InvalidateByTagsAsync("user:all", "user:created");
```

### 3. Predefined Cache Keys

Use constants from `CacheKeyConstants`:

```csharp
// User cache keys
user:id:{userId}
user:email:{email}
user:all
user:profile:{userId}

// Trip cache keys
trip:id:{tripId}
trip:all
trip:user:{userId}
trip:members:{tripId}
trip:details:{tripId}

// Expense cache keys
expense:id:{expenseId}
expense:trip:{tripId}
expense:summary:{tripId}
expense:splits:{expenseId}

// Chat cache keys
chat:messages:{roomId}
chat:history:{userId}:{tripId}

// Join request cache keys
joinrequest:id:{requestId}
joinrequest:trip:{tripId}
joinrequest:user:{userId}

// Rating cache keys
rating:id:{ratingId}
rating:trip:{tripId}
rating:user:{userId}

// Trip member cache keys
tripmember:id:{memberId}
tripmember:trip:{tripId}
```

**Usage Example:**
```csharp
using DOMAIN_LAYER.Enum;

var tripCacheKey = string.Format(CacheKeyConstants.TRIP_BY_ID, tripId);
var tripMembers = string.Format(CacheKeyConstants.TRIP_MEMBERS, tripId);

await _cacheService.SetAsync(tripCacheKey, trip, TimeSpan.FromHours(1));
```

## Cache Policy Management

### Define Cache Policies

```csharp
public class CachePolicySeeder
{
    public async Task SeedPoliciesAsync(ICachePolicyRepository repository)
    {
        var policies = new[]
        {
            new CachePolicy
            {
                PolicyName = "UserPolicy",
                PolicyType = CachePolicyType.Absolute,
                AbsoluteExpirationSeconds = 3600 // 1 hour
            },
            new CachePolicy
            {
                PolicyName = "TripPolicy",
                PolicyType = CachePolicyType.Sliding,
                SlidingExpirationSeconds = 1800 // 30 minutes
            },
            new CachePolicy
            {
                PolicyName = "ExpensePolicy",
                PolicyType = CachePolicyType.NoExpiration
            }
        };

        foreach (var policy in policies)
        {
            await repository.AddAsync(policy);
        }

        await repository.SaveChangesAsync();
    }
}
```

### Use Cache Policies

```csharp
// Use default expiration
await _cacheService.SetAsync("user:123", user);

// Use policy by name
await _cacheService.SetAsync("trip:456", trip, "TripPolicy");

// Use custom expiration
await _cacheService.SetAsync("expense:789", expense, TimeSpan.FromMinutes(30));
```

## Cache Statistics

### Get Cache Statistics

```csharp
var stats = await _cacheService.GetStatisticsAsync();

Console.WriteLine($"Cache Hit Rate: {stats.HitRate}%");
Console.WriteLine($"Cache Items: {stats.CacheItemCount}");
Console.WriteLine($"Cache Size: {stats.CacheSizeBytes} bytes");
```

### Monitor Cache Entries

```csharp
var entryStats = await _cacheEntryRepository.GetCacheStatisticsAsync();

Console.WriteLine($"Total Entries: {entryStats.TotalEntries}");
Console.WriteLine($"Active Entries: {entryStats.ActiveEntries}");
Console.WriteLine($"Expired Entries: {entryStats.ExpiredEntries}");
Console.WriteLine($"Most Accessed Key: {entryStats.MostAccessedKey}");
Console.WriteLine($"Cache Size: {entryStats.CacheSizeBytes} bytes");
```

## Advanced Usage

### Tag-Based Invalidation

```csharp
// Cache with multiple tags
await _cacheService.SetAsync(
    $"trip:{tripId}",
    trip,
    TimeSpan.FromHours(1),
    $"trip:{tripId}",
    "trip:all",
    $"user:{userId}:trips"
);

// Later, invalidate all trips for a user
await _cacheService.InvalidateByTagAsync($"user:{userId}:trips");
```

### Conditional Caching

```csharp
public async Task<Trip> GetTripAsync(int tripId, bool bypassCache = false)
{
    if (bypassCache)
    {
        return await _tripRepository.GetByIdAsync(tripId);
    }

    return await _cacheService.GetOrSetAsync(
        $"trip:id:{tripId}",
        async () => await _tripRepository.GetByIdAsync(tripId),
        TimeSpan.FromHours(1)
    );
}
```

### Batch Operations

```csharp
// Get multiple cache items
var keys = new[] { "user:1", "user:2", "user:3" };
var users = await _cacheRepository.GetManyAsync<User>(keys);

// Remove by pattern
var removedCount = await _cacheService.RemoveByPatternAsync("expense:*");
Console.WriteLine($"Removed {removedCount} expense cache entries");
```

## Best Practices

1. **Use Predefined Keys**: Always use `CacheKeyConstants` for consistency
2. **Set Appropriate TTL**: Balance between freshness and performance
3. **Implement Cache Invalidation**: Update cache when data changes
4. **Use Tags**: For related cache entries that should be invalidated together
5. **Monitor Cache Stats**: Regularly check hit rates and clean up stale entries
6. **Handle Cache Failures**: Design to work if Redis is unavailable
7. **Avoid Circular Dependencies**: Don't cache data that depends on other cached data
8. **Use Async Operations**: All operations are async for better performance

## Error Handling

```csharp
public async Task<User> GetUserWithFallbackAsync(int userId)
{
    try
    {
        return await _cacheService.GetAsync<User>($"user:id:{userId}");
    }
    catch (Exception ex)
    {
        // Log the cache error
        _logger.LogError(ex, "Cache operation failed");
        
        // Fallback to database
        return await _repository.GetByIdAsync(userId);
    }
}
```

## Performance Considerations

- **Serialization**: Uses JSON serialization by default
- **Connection Pooling**: Managed by StackExchange.Redis
- **Key Naming**: Use meaningful patterns for easy debugging
- **Expiration**: Set appropriate TTLs to prevent memory bloat
- **Cleanup**: Enable auto-cleanup for expired entries

## Troubleshooting

### Redis Connection Issues
- Verify Redis is running on configured host:port
- Check connection string format: `localhost:6379`
- Verify network connectivity and firewall settings
- Check `ConnectTimeoutMilliseconds` setting

### Cache Not Working
- Verify Redis connection string is configured
- Check cache keys are consistent
- Monitor Redis using `redis-cli` or Redis GUI tools
- Review application logs for cache operation errors

### Performance Issues
- Monitor cache hit rate via statistics
- Check cache entry size and count
- Enable cleanup to remove expired entries
- Consider adjusting TTL values
- Profile serialization/deserialization performance

## Redis CLI Commands for Debugging

```bash
# Connect to Redis
redis-cli -h localhost -p 6379

# View all keys
KEYS *

# View keys matching pattern
KEYS "tripconnect:*"

# Get value
GET "tripconnect:user:id:1"

# Get key statistics
INFO keyspace

# Monitor operations
MONITOR

# Clear database
FLUSHDB

# Check memory usage
INFO memory
```

## Next Steps

1. Add caching to service methods in APPLICATION_LAYER
2. Configure proper TTLs for different entity types
3. Implement cache invalidation in update/delete operations
4. Add cache statistics monitoring and alerting
5. Set up Redis persistence and backup strategy
