# INFRASTRUCTURE_LAYER — Detailed Documentation

> Analyzed from source code on May 8, 2026.  
> This document is derived from the current implementation, not the older markdown guides in this layer.

---

## Table of Contents

1. Overview
2. Responsibilities
3. Project Structure
4. Dependencies and Packages
5. Dependency Injection Registration
6. Data Access Architecture
7. Entity Framework Core Configuration
8. Repository Implementations
9. Redis Cache Architecture
10. Background Services
11. ImageKit Integration
12. Logging Architecture
13. Configuration Model
14. Runtime Flow Examples
15. Design Notes

---

## 1. Overview

The `INFRASTRUCTURE_LAYER` is the outer implementation layer of the backend. It contains all technology-specific code required to make the application actually run against real systems.

This layer implements:

- SQL Server persistence through Entity Framework Core
- concrete repository implementations for all domain contracts
- Redis-based distributed caching
- cache metadata tracking and invalidation support
- background cache cleanup
- external image storage through ImageKit
- file-based logging through Serilog
- dependency injection registration for infrastructure services

This layer depends on the Domain Layer and supplies the implementations consumed by the Application Layer and API Layer.

---

## 2. Responsibilities

| Area | Responsibility |
|---|---|
| Persistence | Map domain entities to relational tables and execute queries through EF Core |
| Repositories | Implement all repository interfaces defined in `DOMAIN_LAYER` |
| Transactions | Coordinate repository access through `UnitOfWork` |
| Cache | Provide Redis storage, tag invalidation, cache stats, and cache warmup |
| External Services | Integrate with ImageKit for uploads and deletions |
| Logging | Configure rolling file logging with Serilog |
| Hosting | Register cleanup background services and singleton Redis connection |

---

## 3. Project Structure

```text
INFRASTRUCTURE_LAYER/
├── INFRASTRUCTURE_LAYER.csproj
├── DependencyInjection.cs
│
├── Data/
│   ├── TripConnectDbContext.cs
│   └── TripConnectDbContextFactory.cs
│
├── Repository/
│   ├── Repository.cs
│   ├── UnitOfWork.cs
│   ├── UserRepository.cs
│   ├── TripRepository.cs
│   ├── TripRequestRepository.cs
│   ├── TripMemberRepository.cs
│   ├── ExpenseRepository.cs
│   ├── ExpenseSplitRepository.cs
│   ├── ChatRepository.cs
│   ├── TripRatingRepository.cs
│   └── Cache/
│       ├── CacheRepository.cs
│       ├── CacheEntryRepository.cs
│       ├── CacheInvalidationRepository.cs
│       └── CachePolicyRepository.cs
│
├── Cache/
│   ├── RedisCacheConfiguration.cs
│   ├── ICacheService.cs
│   ├── CacheService.cs
│   ├── ICacheStatisticsService.cs
│   ├── CacheStatisticsService.cs
│   ├── ICacheWarmingService.cs
│   └── CacheWarmingService.cs
│
├── Services/
│   ├── IImageKitService.cs
│   ├── ImageKitService.cs
│   └── CacheCleanupBackgroundService.cs
│
├── Logging/
│   └── LoggingConfiguration.cs
│
└── Migrations/
		└── EF Core migration files
```

---

## 4. Dependencies and Packages

`INFRASTRUCTURE_LAYER.csproj` targets `.NET 10` and references the following packages:

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | `10.0.7` | ORM base package |
| `Microsoft.EntityFrameworkCore.SqlServer` | `10.0.7` | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` | `10.0.7` | Migrations and design-time tooling |
| `Microsoft.Extensions.Configuration` | `10.0.7` | Config access |
| `Microsoft.Extensions.Configuration.Binder` | `10.0.7` | Bind config sections to strongly typed models |
| `Microsoft.Extensions.DependencyInjection` | `10.0.7` | DI registration |
| `Microsoft.Extensions.Hosting` | `10.0.7` | Hosted services / background services |
| `StackExchange.Redis` | `2.12.14` | Redis client |
| `Serilog` | `4.3.1` | Structured logging |
| `Serilog.Sinks.File` | `7.0.0` | File sink for rolling logs |
| `Imagekit` | `4.0.1` | Package reference present, though the implementation uses direct REST calls via `HttpClient` |

Project dependency:

- `DOMAIN_LAYER`

---

## 5. Dependency Injection Registration

Infrastructure registration is centralized in `DependencyInjection.cs` via:

```csharp
services.AddInfrastructure(configuration);
```

### Registered Components

| Registration | Lifetime | Notes |
|---|---|---|
| `TripConnectDbContext` | Scoped | SQL Server-backed DbContext |
| `DbContext` | Scoped | Resolved as `TripConnectDbContext` |
| `IRepository<T>` -> `Repository<T>` | Scoped | Generic repository |
| `IUserRepository` -> `UserRepository` | Scoped | User persistence |
| `ITripRepository` -> `TripRepository` | Scoped | Trip persistence |
| `ITripRequestRepository` -> `TripRequestRepository` | Scoped | Join request persistence |
| `ITripMemberRepository` -> `TripMemberRepository` | Scoped | Trip membership persistence |
| `IExpenseRepository` -> `ExpenseRepository` | Scoped | Expense persistence |
| `IExpenseSplitRepository` -> `ExpenseSplitRepository` | Scoped | Expense split persistence |
| `IChatRepository` -> `ChatRepository` | Scoped | Chat persistence |
| `ITripRatingRepository` -> `TripRatingRepository` | Scoped | Rating persistence |
| `IUnitOfWork` -> `UnitOfWork` | Scoped | Repository coordination |
| `RedisCacheConfiguration` | Singleton | Bound from `RedisCache` config section |
| `IConnectionMultiplexer` | Singleton | Shared Redis connection |
| `ICacheRepository` -> `CacheRepository` | Scoped | Low-level Redis operations |
| `ICachePolicyRepository` -> `CachePolicyRepository` | Scoped | Cache policy persistence |
| `ICacheEntryRepository` -> `CacheEntryRepository` | Scoped | Cache metadata persistence |
| `ICacheInvalidationRepository` -> `CacheInvalidationRepository` | Scoped | Tag and dependency invalidation |
| `ICacheService` -> `CacheService` | Scoped | High-level cache API |
| `ICacheStatisticsService` -> `CacheStatisticsService` | Scoped | Metrics aggregation |
| `ICacheWarmingService` -> `CacheWarmingService` | Scoped | Cache preloading |
| `CacheCleanupBackgroundService` | Singleton | Hosted cleanup service |
| `IHostedService` -> `CacheCleanupBackgroundService` | Singleton | Registered as background worker |
| `HttpClient` | Singleton | Shared by `ImageKitService` |
| `IImageKitService` -> `ImageKitService` | Scoped | External image storage integration |

### Startup Validation

During `AddRedisCacheServices`, the code validates that:

- `RedisCache:ConnectionString` exists
- if missing, startup throws `InvalidOperationException`

This makes Redis configuration fail fast instead of failing later at runtime.

---

## 6. Data Access Architecture

The persistence model follows a repository pattern over EF Core.

### Main Components

| Component | Role |
|---|---|
| `TripConnectDbContext` | EF Core mapping and database access root |
| `Repository<T>` | Generic CRUD implementation |
| Specific repositories | Query methods specialized per aggregate |
| `UnitOfWork` | Central access point for all repositories and transaction boundaries |

### Pattern Used

```text
Application Service
		-> IUnitOfWork / IRepository Interfaces (from Domain)
		-> Infrastructure Repository Implementations
		-> EF Core DbContext
		-> SQL Server
```

The generic repository handles the shared CRUD operations. Each specialized repository adds query methods that encode business-specific retrieval patterns.

---

## 7. Entity Framework Core Configuration

### `TripConnectDbContext`

`TripConnectDbContext` defines the following `DbSet`s:

- `Users`
- `Trips`
- `TripRequests`
- `TripMembers`
- `Expenses`
- `ExpenseSplits`
- `ChatMessages`
- `TripRatings`
- `TripDays`
- `CachePolicies`
- `CacheEntries`

### Database Constraints and Indexes

The model configuration is done in `OnModelCreating` using Fluent API.

#### User

- primary key on `Id`
- unique index on `Email`
- unique index on `Username`
- `User -> CreatedTrips` uses `DeleteBehavior.Restrict`
- `User -> TripRequests` uses `Cascade`
- `User -> TripMembers` uses `Cascade`
- `User -> PaidExpenses` uses `Restrict`
- `User -> ExpenseSplits` uses `Cascade`
- `User -> ChatMessages` uses `Restrict`
- `User -> RatingsGiven` uses `Restrict`
- `User -> RatingsReceived` uses `Restrict`

#### Trip

- primary key on `Id`
- `Trip -> TripRequests` uses `Cascade`
- `Trip -> TripMembers` uses `Cascade`
- `Trip -> Expenses` uses `Cascade`
- `Trip -> ChatMessages` uses `Cascade`
- `Trip -> Ratings` uses `Cascade`
- `Trip -> TripDays` uses `Cascade`

#### TripRequest

- primary key on `Id`
- unique composite index on `(TripId, UserId)` to prevent duplicate requests

#### TripMember

- primary key on `Id`
- unique composite index on `(TripId, UserId)` to prevent duplicate memberships

#### Expense

- primary key on `Id`
- one-to-many to `ExpenseSplits` with cascade delete

#### TripRating

- primary key on `Id`
- unique composite index on `(TripId, RatedBy, RatedUserId)` to prevent duplicate ratings

#### CachePolicy

- primary key on `Id`
- unique index on `PolicyName`
- `CreatedAt` default value is `GETUTCDATE()`

#### CacheEntry

- primary key on `Id`
- unique index on `CacheKey`
- index on `ExpiresAt`
- index on `DataType`
- `CachedAt` default value is `GETUTCDATE()`
- optional FK to `CachePolicy` with `DeleteBehavior.SetNull`

### Design-Time Factory

`TripConnectDbContextFactory` exists for EF Core tooling support.

It creates a context using:

```text
Server=(localdb)\MSSQLLocalDB;Database=TripConnect;Trusted_Connection=True;MultipleActiveResultSets=true
```

This allows commands like migrations and database updates to work outside the running API host.

---

## 8. Repository Implementations

### 8.1 Generic Repository

`Repository<T>` provides the base implementation of `IRepository<T>`.

Implemented methods:

- `GetByIdAsync(int id)` via `DbSet.FindAsync`
- `GetAllAsync()` via `ToListAsync`
- `AddAsync(T entity)`
- `UpdateAsync(T entity)`
- `DeleteAsync(int id)`
- `SaveChangesAsync()`
- `ExistsAsync(int id)`

This repository is persistence-agnostic at the interface level but EF-specific in implementation.

### 8.2 UserRepository

Key behaviors:

- fetch by `Email`, `Phone`, `Username`
- check uniqueness with `AnyAsync`
- fetch by `RefreshToken`
- fetch by `EmailVerificationToken`
- update verification status and rating in-place
- update password hash and salt
- return verified users where both `PhoneVerified` and `IdVerified` are true

### 8.3 TripRepository

Key behaviors:

- overrides `GetByIdAsync` to eager-load `TripDays`
- filters by `TripStatus`, `Location`, `HostId`, `Budget`, `TravelType`
- `GetUpcomingTripsAsync()` returns `Planned` trips with `StartDate > DateTime.Now`
- `SearchTripsAsync(...)` composes a dynamic EF query based on optional parameters
- `GetTripWithDetailsAsync()` eager-loads:
	- `TripMembers`
	- `TripRequests`
	- `Expenses`
	- `Ratings`
	- `TripDays`

### 8.4 TripRequestRepository

Key behaviors:

- query requests by trip, user, or status
- retrieve only pending requests for a trip
- check if a user already requested to join a trip
- update request status and stamp `RespondedAt = DateTime.Now`

### 8.5 TripMemberRepository

Key behaviors:

- fetch members for a trip or memberships for a user
- filter members by role
- return only active members for trip operations
- update membership status
- `GetMemberCountAsync()` counts only members with `Status == Active`

This active-member count is critical to expense splitting logic.

### 8.6 ExpenseRepository

Key behaviors:

- query expenses by trip or paying user
- aggregate total trip cost with `SumAsync`
- aggregate what one user paid within a trip
- eager-load `ExpenseSplits` via `GetExpenseWithSplitsAsync`
- `DeleteExpenseWithSplitsAsync()` relies on cascade delete configured in EF Core

### 8.7 ExpenseSplitRepository

Key behaviors:

- fetch splits by expense
- eager-load `Expense` when computing owed values per trip
- compute unsettled totals per user and trip
- mark a split as settled by toggling `IsSettled`
- return all unsettled splits in a trip

### 8.8 ChatRepository

Key behaviors:

- query trip messages ordered by `CreatedAt`
- paginate using `Skip` and `Take`
- return latest N messages by descending sort, then reverse to restore chronological order
- search messages using `Message.Contains(searchText)`
- delete all messages for a trip and persist immediately

### 8.9 TripRatingRepository

Key behaviors:

- query ratings by trip, rater, or rated user
- compute average rating in memory after EF query
- prevent duplicate ratings through both repository checks and DB unique index
- return only ratings with non-empty reviews

### 8.10 UnitOfWork

`UnitOfWork` lazily constructs repositories over the shared scoped `DbContext`.

Repository properties exposed:

- `Users`
- `Trips`
- `TripRequests`
- `TripMembers`
- `Expenses`
- `ExpenseSplits`
- `ChatMessages`
- `TripRatings`

Additional infrastructure capabilities beyond the domain interface:

- `HasActiveTransaction`
- `BeginTransactionAsync()`
- `CommitTransactionAsync()`
- `RollbackTransactionAsync()`

This means the concrete implementation supports transaction management even though the domain contract focuses mainly on repository access and saving changes.

---

## 9. Redis Cache Architecture

The cache subsystem is split into two layers:

1. low-level Redis access and metadata repositories
2. high-level cache services used by the application

### 9.1 Redis Configuration

`RedisCacheConfiguration` holds:

| Property | Default | Purpose |
|---|---|---|
| `ConnectionString` | none | Redis endpoint and credentials |
| `DatabaseIndex` | `0` | Redis logical database |
| `KeyPrefix` | `tripconnect:` | Global namespace prefix |
| `DefaultAbsoluteExpirationSeconds` | `3600` | 1 hour default TTL |
| `DefaultSlidingExpirationSeconds` | `1800` | 30 minute sliding TTL |
| `ConnectTimeoutMilliseconds` | `5000` | Redis connection timeout |
| `EnableAutoCleanup` | `true` | Enables hosted cache cleanup |
| `CleanupIntervalMinutes` | `60` | Cleanup timer interval |
| `EnableCompression` | `false` | Present in config model, not implemented in current cache writes |

`GetConfigurationOptions()` parses the connection string and sets:

- `ConnectTimeout`
- `DefaultDatabase`
- `AbortOnConnectFail = false`

### 9.2 RedisConnectionFactory

Provides a singleton-style `ConnectionMultiplexer` with double-checked locking.

Responsibilities:

- create one shared connection
- reuse active connection if already connected
- expose `CloseConnection()` for manual disposal

### 9.3 CacheRepository

This is the low-level Redis repository implementing `ICacheRepository`.

Key behaviors:

- serialize all values via `System.Text.Json`
- deserialize on reads
- if JSON deserialization fails, delete the corrupted Redis key
- support single-key and multi-key retrieval patterns
- pattern-based deletes use Redis server key scans
- `ClearAllAsync()` flushes the current Redis database

Important implementation details:

- `GetAsync<T>()` returns `default` when a key does not exist
- `SetAsync<T>()` deletes the key if `value == null`
- `RemoveByPatternAsync()` iterates `server.Keys(pattern: pattern)` and deletes keys individually

### 9.4 CacheEntryRepository

Tracks metadata for cache entries in SQL Server.

Key behaviors:

- fetch by `CacheKey`
- query expired entries using `ExpiresAt <= DateTime.UtcNow`
- compute aggregate stats including:
	- total entries
	- expired entries
	- active entries
	- total access count
	- average age in hours
	- most accessed key
	- total cache size in bytes
- update per-entry access metadata using `CacheEntry.UpdateAccessInfo()`

This repository gives the system observability over cache behavior, separate from Redis itself.

### 9.5 CacheInvalidationRepository

Handles tag-based invalidation and invalidation dependencies using Redis sets.

Internal Redis key prefixes:

- `tag:` for mapping a tag to cache keys
- `key_tags:` for mapping a cache key to its tags
- `invalidation:` for dependency rules

Key behaviors:

- invalidate by key pattern
- invalidate by one or many tags
- add one or many tags to a cache key
- query all tags for a key
- query all keys for a tag
- register invalidation dependencies between key patterns

All tag and dependency bookkeeping keys are given a 365-day expiration.

### 9.6 CachePolicyRepository

Stores reusable cache policies in SQL Server.

Key behaviors:

- get an active policy by name
- list active policies ordered by name
- list active policies by `CachePolicyType`
- enable/disable policies and stamp `ModifiedAt = DateTime.UtcNow`

### 9.7 CacheService

This is the high-level cache facade used by the application.

Key capabilities:

- auto-prefix cache keys with `tripconnect:`
- use default TTL if no explicit expiration is supplied
- resolve policy-based expiration by name
- attach tags on write
- cache around a factory method with `GetOrSetAsync`
- track in-memory hit and miss counts
- expose summary statistics
- swallow cache failures on set/get-or-set and log warnings instead of failing requests

This is an important design choice: cache is treated as an optimization, not a hard dependency for core request success.

### 9.8 CacheStatisticsService

Aggregates information from both `ICacheEntryRepository` and `ICacheService`.

Produced metrics include:

- total entries
- active entries
- expired entries
- total hits and misses
- hit rate
- total cache size
- average entry age
- per-data-type entry counts
- memory pressure percentage
- top accessed keys

Memory pressure is estimated against an assumed 500 MB ceiling.

### 9.9 CacheWarmingService

Preloads commonly accessed cache entries.

Warmup strategy in current code:

- users: up to top 100 users from `IUserRepository.GetAllAsync()`
- trips: up to top 50 trips from `ITripRepository.GetAllAsync()`
- cache policies: inserts three predefined policies into Redis

Tags used during warmup include:

- `user:warmup`
- `user:all`
- `trip:warmup`
- `trip:all`

Status tracking includes:

- whether warming is active
- start and completion timestamps
- duration in milliseconds
- entries warmed
- accumulated errors
- strategy label

---

## 10. Background Services

### CacheCleanupBackgroundService

`CacheCleanupBackgroundService` derives from `BackgroundService` and performs periodic cleanup of expired cache data.

Behavior:

- if `EnableAutoCleanup` is false, the worker exits immediately
- on start, it creates a `Timer`
- first run occurs after `CleanupIntervalMinutes`
- each cycle:
	- resolves scoped services from the root provider
	- fetches expired cache entries from SQL via `ICacheEntryRepository`
	- deletes corresponding Redis keys via `ICacheRepository`
	- deletes expired metadata rows from SQL
- logs activity via `Debug.WriteLine`

Lifecycle management:

- disposes the timer in `StopAsync`
- disposes the timer in `Dispose()` as a safety net

This service keeps Redis and cache metadata tables in sync over time.

---

## 11. ImageKit Integration

### Service Overview

`ImageKitService` implements `IImageKitService` but uses direct REST calls through `HttpClient` rather than the package API.

Endpoints used:

- upload: `https://upload.imagekit.io/api/v1/files/upload`
- delete: `https://api.imagekit.io/v1/files/{fileId}`

### Required Configuration

The constructor requires these config values:

- `ImageKit:PublicKey`
- `ImageKit:PrivateKey`
- `ImageKit:UrlEndpoint`

If any are missing, construction throws `InvalidOperationException`.

Optional upload config:

- `ImageUpload:MaxFileSizeInMB`
- `ImageUpload:AllowedFormats`

Defaults used when not configured:

- max file size: `5 MB`
- allowed formats: `jpg`, `jpeg`, `png`, `webp`

### Upload Flow

`UploadImageAsync(Stream imageStream, string fileName, string folder = "uploads")` performs:

1. validate file is non-empty
2. validate size is under configured maximum
3. validate extension against allowed formats
4. sanitize file name using `Path.GetFileName` and regex replacement
5. read the stream into memory as bytes
6. create multipart form data
7. authenticate with Basic Auth using `privateKey:`
8. send upload request to ImageKit
9. parse JSON response into `ImageUploadResponse`

Returned fields include:

- `FileId`
- `Url`
- `FilePath`
- `FileName`
- `FileSize`
- `Width`
- `Height`
- `UploadedAt`
- `Success`
- `ErrorMessage`

### Delete Flow

`DeleteImageAsync(string fileId)`:

- validates non-empty `fileId`
- sends authenticated `DELETE`
- returns `true` only for successful HTTP responses
- logs failures and returns `false` without throwing to callers

### URL Generation

`GetImageUrlAsync(...)` builds transformation URLs without a network call.

Supported transformation parameters:

- width -> `w-<value>`
- height -> `h-<value>`
- quality -> `q-<value>` clamped to `1..100`

This means image resize/quality variants are generated as pure URLs rather than precomputed assets.

### Error Handling Strategy

The service catches and maps:

- configuration errors
- validation errors
- file I/O errors
- HTTP request failures
- unexpected exceptions

In upload operations, failures are converted into `ImageUploadResponse { Success = false, ErrorMessage = ... }`.

---

## 12. Logging Architecture

Logging is configured in `LoggingConfiguration` using Serilog.

### Configuration

- minimum log level: `Information`
- namespace overrides:
	- `Microsoft` -> `Warning`
	- `Microsoft.EntityFrameworkCore` -> `Warning`
	- `System` -> `Warning`

### Sink

File sink configuration:

- path: `logs/tripconnect-.txt`
- rolling interval: daily
- retention: 7 files
- output template:

```text
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

### Helper Methods

Static helpers are provided for:

- `GetLogger<T>()`
- `LogInfo(...)`
- `LogWarning(...)`
- `LogError(...)`
- `LogCritical(...)`

This gives a central place for file logging behavior while still allowing DI-based logging elsewhere.

---

## 13. Configuration Model

The infrastructure layer consumes these configuration sections across the backend:

### SQL Server

- `ConnectionStrings:DefaultConnection`

### RedisCache

- `ConnectionString`
- `DatabaseIndex`
- `KeyPrefix`
- `DefaultAbsoluteExpirationSeconds`
- `DefaultSlidingExpirationSeconds`
- `ConnectTimeoutMilliseconds`
- `EnableAutoCleanup`
- `CleanupIntervalMinutes`
- `EnableCompression`

### ImageKit

- `PublicKey`
- `PrivateKey`
- `UrlEndpoint`

### ImageUpload

- `MaxFileSizeInMB`
- `AllowedFormats`

---

## 14. Runtime Flow Examples

### Example 1: Trip lookup with cache-aside

```text
Application Service
	-> ICacheService.GetOrSetAsync("trip:id:7", factory)
	-> CacheService prefixes key to "tripconnect:trip:id:7"
	-> CacheRepository checks Redis
	-> cache miss
	-> factory executes repository query through TripRepository / DbContext
	-> result stored in Redis with TTL
	-> result returned to caller
```

### Example 2: Expired cache cleanup

```text
Hosted service timer fires
	-> CacheCleanupBackgroundService creates DI scope
	-> CacheEntryRepository.GetExpiredEntriesAsync()
	-> for each expired row, CacheRepository.RemoveAsync(cacheKey)
	-> CacheEntryRepository.DeleteExpiredEntriesAsync()
	-> Redis and SQL metadata remain aligned
```

### Example 3: Image upload

```text
API layer receives file stream
	-> ImageKitService validates file size and extension
	-> sanitizes file name
	-> posts multipart request to ImageKit REST API
	-> parses JSON response
	-> returns fileId + CDN URL + dimensions to application/API layer
```

---

## 15. Design Notes

### Cache is optional at request level

`CacheService` deliberately catches write/read-through cache failures and logs warnings instead of failing the main workflow. This keeps Redis as a performance optimization, not a mandatory dependency for basic correctness.

### Dual cache model: Redis + SQL metadata

The implementation stores actual values in Redis while keeping metadata in SQL through `CacheEntry` and `CachePolicy`. This supports observability, cleanup, policies, and analytics that Redis alone would not easily provide.

### Repositories stay thin

Most repository implementations are direct EF query wrappers. That keeps business rules in the Application Layer while letting Infrastructure focus on persistence mechanics.

### UnitOfWork is richer than the domain contract

The concrete `UnitOfWork` supports transactions, while the exposed domain interface is simpler. This gives implementation flexibility without forcing transaction concerns into all consumers.

### ImageKit package is not the active integration path

Even though the project references the `Imagekit` package, the actual code uses raw REST requests with `HttpClient`. Any future maintenance should treat the REST implementation as the source of truth unless that integration is intentionally refactored.

---

Document generated from source code analysis on May 8, 2026.
