# Redis Cache Integration Guide for Developers

## Quick Start - Inject and Use

### Basic Controller Implementation

```csharp
using INFRASTRUCTURE_LAYER.Cache;
using DOMAIN_LAYER.Enum;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICacheService _cacheService;
    private readonly ICacheStatisticsService _statistics;

    public UserController(
        IUserService userService,
        ICacheService cacheService,
        ICacheStatisticsService statistics)
    {
        _userService = userService;
        _cacheService = cacheService;
        _statistics = statistics;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }

    // Cache Management Endpoints
    [HttpGet("cache/statistics")]
    public async Task<IActionResult> GetCacheStatistics()
    {
        var stats = await _statistics.GetDetailedStatisticsAsync();
        return Ok(stats);
    }

    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache([FromQuery] string pattern = null)
    {
        if (string.IsNullOrEmpty(pattern))
            await _cacheService.ClearAllAsync();
        else
            await _cacheService.RemoveByPatternAsync(pattern);

        return Ok(new { message = "Cache cleared successfully" });
    }

    [HttpGet("cache/warmup")]
    public async Task<IActionResult> WarmupCache()
    {
        var warmingService = HttpContext.RequestServices.GetRequiredService<ICacheWarmingService>();
        await warmingService.WarmupCacheAsync();
        var status = await warmingService.GetStatusAsync();
        return Ok(status);
    }
}
```

---

## Service Layer Implementation

### User Service with Caching

```csharp
using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Cache;

public interface IUserService
{
    Task<User> GetUserAsync(int userId);
    Task<User> CreateUserAsync(CreateUserDto dto);
    Task<User> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task DeleteUserAsync(int userId);
    Task<IEnumerable<User>> GetUsersByEmailAsync(string email);
}

public class UserService : IUserService
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

    // ✅ GET with caching
    public async Task<User> GetUserAsync(int userId)
    {
        return await _cacheService.GetOrSetAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, userId),
            async () => await _userRepository.GetByIdAsync(userId),
            TimeSpan.FromHours(1)
        );
    }

    // ✅ CREATE with cache tags
    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User { /* ... mapping ... */ };
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Cache new user with tags
        await _cacheService.SetAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, user.Id),
            user,
            TimeSpan.FromHours(1),
            "user:created",
            "user:all"
        );

        return user;
    }

    // ✅ UPDATE with cache invalidation
    public async Task<User> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        // Map and update properties
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Invalidate relevant caches
        await _cacheService.RemoveAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, userId)
        );
        
        // Invalidate all user-related caches
        await _cacheService.InvalidateByTagAsync("user:all");

        return user;
    }

    // ✅ DELETE with cache cleanup
    public async Task DeleteUserAsync(int userId)
    {
        await _userRepository.DeleteAsync(userId);
        await _userRepository.SaveChangesAsync();

        // Remove from cache
        await _cacheService.RemoveAsync(
            string.Format(CacheKeyConstants.USER_BY_ID, userId)
        );
        
        // Invalidate all user caches
        await _cacheService.InvalidateByTagAsync("user:all");
    }

    // ✅ Complex query with caching
    public async Task<IEnumerable<User>> GetUsersByEmailAsync(string email)
    {
        return await _cacheService.GetOrSetAsync(
            string.Format(CacheKeyConstants.USER_BY_EMAIL, email),
            async () =>
            {
                // Execute complex query
                var users = await _userRepository.GetAllAsync();
                return users.Where(u => u.Email.Contains(email));
            },
            TimeSpan.FromHours(2)
        );
    }
}
```

---

## Trip Service Example with Tags

```csharp
public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly ICacheService _cacheService;

    public TripService(
        ITripRepository tripRepository,
        ICacheService cacheService)
    {
        _tripRepository = tripRepository;
        _cacheService = cacheService;
    }

    public async Task<Trip> GetTripAsync(int tripId, int userId)
    {
        return await _cacheService.GetOrSetAsync(
            string.Format(CacheKeyConstants.TRIP_BY_ID, tripId),
            async () => await _tripRepository.GetByIdAsync(tripId),
            TimeSpan.FromHours(1),
            // Tags for invalidation
            $"trip:{tripId}",
            $"user:{userId}:trips",
            "trip:all"
        );
    }

    public async Task<Trip> CreateTripAsync(CreateTripDto dto, int userId)
    {
        var trip = new Trip { /* mapping */ };
        await _tripRepository.AddAsync(trip);
        await _tripRepository.SaveChangesAsync();

        // Cache with tags
        await _cacheService.SetAsync(
            string.Format(CacheKeyConstants.TRIP_BY_ID, trip.Id),
            trip,
            TimeSpan.FromHours(1),
            $"trip:{trip.Id}",
            $"user:{userId}:trips",
            "trip:all"
        );

        return trip;
    }

    public async Task<Trip> UpdateTripAsync(int tripId, UpdateTripDto dto, int userId)
    {
        var trip = await _tripRepository.GetByIdAsync(tripId);
        // Update properties
        
        await _tripRepository.UpdateAsync(trip);
        await _tripRepository.SaveChangesAsync();

        // Invalidate by tag pattern
        await _cacheService.InvalidateByTagsAsync(
            $"trip:{tripId}",
            $"user:{userId}:trips"
        );

        return trip;
    }

    public async Task DeleteTripAsync(int tripId, int userId)
    {
        await _tripRepository.DeleteAsync(tripId);
        await _tripRepository.SaveChangesAsync();

        // Invalidate all related caches
        await _cacheService.InvalidateByTagsAsync(
            $"trip:{tripId}",
            $"user:{userId}:trips",
            "trip:all"
        );
    }
}
```

---

## Expense Service with Cache Relationships

```csharp
public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICacheService _cacheService;

    public async Task<Expense> CreateExpenseAsync(CreateExpenseDto dto, int tripId)
    {
        var expense = new Expense { /* mapping */ };
        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        // Cache with trip relationship tag
        await _cacheService.SetAsync(
            string.Format(CacheKeyConstants.EXPENSE_BY_ID, expense.Id),
            expense,
            TimeSpan.FromMinutes(30),
            $"trip:{tripId}:expenses",
            $"expense:summary:{tripId}",
            "expense:all"
        );

        // Invalidate trip summary cache (expenses changed)
        await _cacheService.RemoveByPatternAsync($"expense:summary:{tripId}");

        return expense;
    }

    public async Task DeleteExpenseAsync(int expenseId, int tripId)
    {
        await _expenseRepository.DeleteAsync(expenseId);
        await _expenseRepository.SaveChangesAsync();

        // Invalidate all related caches
        await _cacheService.InvalidateByTagsAsync(
            $"trip:{tripId}:expenses",
            $"expense:summary:{tripId}"
        );
    }
}
```

---

## Advanced Patterns

### Conditional Caching

```csharp
public async Task<Report> GetTripReportAsync(int tripId, bool skipCache = false)
{
    if (skipCache)
    {
        return await GenerateReportAsync(tripId);
    }

    return await _cacheService.GetOrSetAsync(
        $"report:trip:{tripId}",
        async () => await GenerateReportAsync(tripId),
        TimeSpan.FromHours(2)
    );
}
```

### Cache with Policy

```csharp
public async Task<User> GetUserWithPolicyAsync(int userId)
{
    return await _cacheService.GetOrSetAsync(
        $"user:id:{userId}",
        async () => await _userRepository.GetByIdAsync(userId),
        null, // Will use policy expiration
        // Tags for invalidation
        "user:all"
    );
}
```

### Batch Operations

```csharp
public async Task<Dictionary<int, User>> GetUsersAsync(params int[] userIds)
{
    var result = new Dictionary<int, User>();

    foreach (var userId in userIds)
    {
        var user = await _cacheService.GetOrSetAsync(
            $"user:id:{userId}",
            async () => await _userRepository.GetByIdAsync(userId),
            TimeSpan.FromHours(1)
        );

        if (user != null)
            result[userId] = user;
    }

    return result;
}
```

---

## Testing with Cache

### Unit Test Example

```csharp
[TestClass]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockRepository;
    private Mock<ICacheService> _mockCache;
    private UserService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockCache = new Mock<ICacheService>();
        _service = new UserService(_mockRepository.Object, _mockCache.Object);
    }

    [TestMethod]
    public async Task GetUser_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var userId = 1;
        var cachedUser = new User { Id = userId, Email = "test@example.com" };

        _mockCache
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User>>>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(cachedUser);

        // Act
        var result = await _service.GetUserAsync(userId);

        // Assert
        Assert.AreEqual(cachedUser, result);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Never);
    }

    [TestMethod]
    public async Task UpdateUser_InvalidatesCache()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId };
        var updateDto = new UpdateUserDto { Email = "newemail@example.com" };

        _mockRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _service.UpdateUserAsync(userId, updateDto);

        // Assert
        _mockCache.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Once);
    }
}
```

---

## Dependency Injection Registration

All dependencies are automatically registered in `DependencyInjection.cs`. In your `Program.cs`:

```csharp
// Already included in AddInfrastructure
services.AddInfrastructure(configuration);

// Creates scoped instances of:
// - ICacheService
// - ICacheStatisticsService
// - ICacheWarmingService
// - All cache repositories

// Registers hosted service:
// - CacheCleanupBackgroundService
```

---

## Common Cache Keys

```csharp
// Use these predefined constants
string.Format(CacheKeyConstants.USER_BY_ID, userId)
string.Format(CacheKeyConstants.TRIP_BY_ID, tripId)
string.Format(CacheKeyConstants.EXPENSE_BY_TRIP, tripId)
string.Format(CacheKeyConstants.TRIP_MEMBERS, tripId)
// ... and many more in CacheKeyConstants
```

---

## Error Handling

```csharp
try
{
    var user = await _cacheService.GetAsync<User>("user:123");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Cache operation failed");
    // Fallback to database
    user = await _userRepository.GetByIdAsync(123);
}
```

---

## Performance Tips

1. ✅ Use `GetOrSetAsync` for read-heavy operations
2. ✅ Set appropriate TTL based on data volatility
3. ✅ Use tags for related cache entries
4. ✅ Invalidate cache on every data modification
5. ✅ Monitor cache statistics regularly
6. ✅ Pre-warm cache with commonly accessed data
7. ✅ Use cache patterns (`trip:123:*`) for bulk operations

---

## References

- [REDIS_CACHE_IMPLEMENTATION.md](../INFRASTRUCTURE_LAYER/REDIS_CACHE_IMPLEMENTATION.md) - Complete guide
- [REDIS_CACHE_QUICK_REFERENCE.md](../REDIS_CACHE_QUICK_REFERENCE.md) - Quick lookup
- [IMPLEMENTATION_COMPLETE.md](../IMPLEMENTATION_COMPLETE.md) - Status overview
