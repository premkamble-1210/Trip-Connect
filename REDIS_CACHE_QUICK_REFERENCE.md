# Redis Cache Quick Reference

## Quick Setup (Already Done)
- ✅ Redis cache repositories implemented in INFRASTRUCTURE_LAYER
- ✅ Dependency injection configured
- ✅ appsettings.json configured
- ✅ ICacheService high-level API available

## Dependency Injection Registration
All services are registered in `INFRASTRUCTURE_LAYER.DependencyInjection`:
```csharp
services.AddInfrastructure(configuration);
```

## Interfaces Available for Injection

### High-Level API (Recommended)
```csharp
// Use this for most operations
private readonly ICacheService _cacheService;
```

### Low-Level APIs (For Advanced Use)
```csharp
private readonly ICacheRepository _cacheRepository;
private readonly ICachePolicyRepository _policyRepository;
private readonly ICacheEntryRepository _entryRepository;
private readonly ICacheInvalidationRepository _invalidationRepository;
```

## Common Operations

### 1. Get or Set (Most Common)
```csharp
var user = await _cacheService.GetOrSetAsync(
    $"user:id:{userId}",
    async () => await _userRepository.GetByIdAsync(userId),
    TimeSpan.FromHours(1)
);
```

### 2. Set Cache
```csharp
await _cacheService.SetAsync("key", value, TimeSpan.FromHours(1));
```

### 3. Get Cache
```csharp
var value = await _cacheService.GetAsync<T>("key");
```

### 4. Remove Cache
```csharp
await _cacheService.RemoveAsync("key");
```

### 5. Check Exists
```csharp
bool exists = await _cacheService.ExistsAsync("key");
```

### 6. Remove by Pattern
```csharp
await _cacheService.RemoveByPatternAsync("trip:*");
```

### 7. Invalidate by Tag
```csharp
await _cacheService.InvalidateByTagAsync("user:all");
```

## Cache Key Format

Use `CacheKeyConstants` for consistency:
```csharp
using DOMAIN_LAYER.Enum;

// User keys
string.Format(CacheKeyConstants.USER_BY_ID, userId)
string.Format(CacheKeyConstants.USER_BY_EMAIL, email)

// Trip keys
string.Format(CacheKeyConstants.TRIP_BY_ID, tripId)
string.Format(CacheKeyConstants.TRIP_MEMBERS, tripId)

// Expense keys
string.Format(CacheKeyConstants.EXPENSE_BY_ID, expenseId)
string.Format(CacheKeyConstants.EXPENSE_BY_TRIP, tripId)
```

## Configuration File

**Location**: `appsettings.json`

```json
{
  "RedisCache": {
    "ConnectionString": "localhost:6379",
    "DatabaseIndex": 0,
    "KeyPrefix": "tripconnect:",
    "DefaultAbsoluteExpirationSeconds": 3600,
    "DefaultSlidingExpirationSeconds": 1800
  }
}
```

## Complete Example: User Service

```csharp
using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Cache;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;

    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
    }

    // GET with caching
    public async Task<User> GetUserAsync(int userId)
    {
        return await _cacheService.GetOrSetAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, userId),
            async () => await _userRepository.GetByIdAsync(userId),
            TimeSpan.FromHours(1)
        );
    }

    // CREATE with caching
    public async Task<User> CreateUserAsync(User user)
    {
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Cache new user
        await _cacheService.SetAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, user.Id),
            user,
            TimeSpan.FromHours(1),
            "user:created",
            "user:all"
        );

        return user;
    }

    // UPDATE with cache invalidation
    public async Task<User> UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Invalidate relevant caches
        await _cacheService.RemoveAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, user.Id)
        );
        
        await _cacheService.InvalidateByTagAsync("user:all");

        return user;
    }

    // DELETE with cache invalidation
    public async Task DeleteUserAsync(int userId)
    {
        await _userRepository.DeleteAsync(userId);
        await _userRepository.SaveChangesAsync();

        // Invalidate user cache
        await _cacheService.RemoveAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, userId)
        );
        
        await _cacheService.InvalidateByTagAsync("user:all");
    }
}
```

## Controller Injection

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICacheService _cacheService;

    public UserController(
        IUserService userService,
        ICacheService cacheService)
    {
        _userService = userService;
        _cacheService = cacheService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }

    [HttpPost("clear-cache")]
    public async Task<IActionResult> ClearCache([FromQuery] string pattern = null)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            await _cacheService.ClearAllAsync();
        }
        else
        {
            await _cacheService.RemoveByPatternAsync(pattern);
        }

        return Ok("Cache cleared");
    }

    [HttpGet("cache-stats")]
    public async Task<IActionResult> GetCacheStats()
    {
        var stats = await _cacheService.GetStatisticsAsync();
        return Ok(stats);
    }
}
```

## Important Notes

1. **Key Prefix**: Automatically added (e.g., "tripconnect:user:id:1")
2. **Expiration**: Default is 1 hour (3600 seconds)
3. **Serialization**: Automatic JSON serialization/deserialization
4. **Thread-Safe**: Use Redis singleton connection
5. **Async Only**: All operations are async

## Debugging Redis

```bash
# Connect to Redis
redis-cli

# See all keys
KEYS tripconnect:*

# Get a value
GET "tripconnect:user:id:1"

# Monitor live commands
MONITOR

# Clear all data
FLUSHDB
```

## Checklist for Implementation

When implementing cache in a service:

- [ ] Use `GetOrSetAsync` for reads
- [ ] Invalidate cache on write operations
- [ ] Use `CacheKeyConstants` for key names
- [ ] Set appropriate TTL values
- [ ] Add tags for related entries
- [ ] Handle cache failures gracefully
- [ ] Document cache invalidation strategy
- [ ] Add unit tests for cache operations
- [ ] Monitor cache statistics regularly
- [ ] Review and adjust TTL values based on usage
