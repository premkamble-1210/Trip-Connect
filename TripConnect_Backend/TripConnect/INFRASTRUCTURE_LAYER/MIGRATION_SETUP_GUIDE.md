# Redis Cache Migration Setup Guide

## Migration Creation Instructions

After implementing the Redis cache infrastructure, you need to create and apply a database migration to add the cache tables.

### Step 1: Create Migration

Run this command in the **INFRASTRUCTURE_LAYER** folder:

```powershell
cd TripConnect_Backend\TripConnect\INFRASTRUCTURE_LAYER

# Create the migration
Add-Migration AddCacheTablesToDatabase -Context TripConnectDbContext -OutputDir Migrations
```

### Step 2: Review Migration

The migration file will be created in `INFRASTRUCTURE_LAYER/Migrations/` with a name like:
- `[timestamp]_AddCacheTablesToDatabase.cs`

Review the migration to ensure it contains:
- `CachePolicies` table creation
- `CacheEntries` table creation
- Proper indexes on `PolicyName`, `CacheKey`, `ExpiresAt`, `DataType`
- Foreign key relationship between `CacheEntries` and `CachePolicies`

### Step 3: Apply Migration

Run this command to apply the migration:

```powershell
# Update the database
Update-Database -Context TripConnectDbContext
```

### Step 4: Verify Migration

Check that the migration was applied successfully:

```sql
-- Run in SQL Server
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('CachePolicies', 'CacheEntries');

-- Check indexes
SELECT * FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME IN ('CachePolicies', 'CacheEntries');
```

## Expected Tables

### CachePolicies Table
```sql
CREATE TABLE [dbo].[CachePolicies] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PolicyName] NVARCHAR(255) UNIQUE NOT NULL,
    [PolicyType] INT NOT NULL,
    [AbsoluteExpirationSeconds] INT NULL,
    [SlidingExpirationSeconds] INT NULL,
    [IsActive] BIT NOT NULL DEFAULT(1),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    [ModifiedAt] DATETIME2 NULL
);

CREATE INDEX IX_PolicyName ON [dbo].[CachePolicies]([PolicyName]);
```

### CacheEntries Table
```sql
CREATE TABLE [dbo].[CacheEntries] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CacheKey] NVARCHAR(MAX) UNIQUE NOT NULL,
    [CachedData] NVARCHAR(MAX) NOT NULL,
    [DataType] NVARCHAR(255) NOT NULL,
    [ExpiresAt] DATETIME2 NULL,
    [CachedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    [AccessCount] BIGINT NOT NULL DEFAULT(0),
    [LastAccessedAt] DATETIME2 NULL,
    [CachePolicyId] INT NULL,
    FOREIGN KEY ([CachePolicyId]) REFERENCES [dbo].[CachePolicies]([Id]) ON DELETE SET NULL
);

CREATE INDEX IX_CacheKey ON [dbo].[CacheEntries]([CacheKey]);
CREATE INDEX IX_ExpiresAt ON [dbo].[CacheEntries]([ExpiresAt]);
CREATE INDEX IX_DataType ON [dbo].[CacheEntries]([DataType]);
```

## Troubleshooting

### Migration Not Found
If you get "No migrations found", make sure:
1. You're in the API_LAYER directory (where Program.cs is)
2. The INFRASTRUCTURE_LAYER project is properly referenced
3. Your default project in Package Manager Console is set to INFRASTRUCTURE_LAYER

### Migration Failed
If migration fails, check:
1. Your database connection string is correct in `appsettings.json`
2. The database server is accessible
3. You have permissions to create tables
4. No naming conflicts with existing tables

## Files Modified/Created

### Modified Files
- ✅ `INFRASTRUCTURE_LAYER\Data\TripConnectDbContext.cs`
  - Added `using DOMAIN_LAYER.Entity.Cache;`
  - Added `DbSet<CachePolicy>` and `DbSet<CacheEntry>`
  - Added entity configuration in `OnModelCreating()`

- ✅ `INFRASTRUCTURE_LAYER\DependencyInjection.cs`
  - Added service registrations for cache statistics and warming
  - Added background service registration

### New Files Created
- ✅ `INFRASTRUCTURE_LAYER\Cache\ICacheStatisticsService.cs`
- ✅ `INFRASTRUCTURE_LAYER\Cache\CacheStatisticsService.cs`
- ✅ `INFRASTRUCTURE_LAYER\Cache\ICacheWarmingService.cs`
- ✅ `INFRASTRUCTURE_LAYER\Cache\CacheWarmingService.cs`
- ✅ `INFRASTRUCTURE_LAYER\Services\CacheCleanupBackgroundService.cs`

## Next Steps After Migration

1. **Initialize Cache Policies**
   ```csharp
   // In your application startup
   var cachePolicyRepository = serviceProvider.GetRequiredService<ICachePolicyRepository>();
   // Seed initial cache policies
   ```

2. **Start Cache Warmup Service**
   ```csharp
   // In your application startup
   var cacheWarmingService = serviceProvider.GetRequiredService<ICacheWarmingService>();
   await cacheWarmingService.WarmupCacheAsync();
   ```

3. **Implement Caching in Services**
   - Use `ICacheService` in your APPLICATION_LAYER services
   - Wrap database queries with `GetOrSetAsync()`
   - Invalidate cache on data modifications

4. **Monitor Cache Statistics**
   - Add endpoint to get cache statistics
   - Monitor hit rate and memory usage
   - Adjust TTL values based on usage patterns

## Quick Commands Reference

```powershell
# Package Manager Console

# Create migration
Add-Migration AddCacheTablesToDatabase -Context TripConnectDbContext -OutputDir Migrations

# Apply migration
Update-Database -Context TripConnectDbContext

# Remove last migration (if needed)
Remove-Migration -Context TripConnectDbContext

# Show migrations
Get-Migration -Context TripConnectDbContext

# Script migration (generates SQL)
Script-Migration -Context TripConnectDbContext
```

## Configuration Verification

Before running migration, verify:
1. ✅ Redis connection string in `appsettings.json`
2. ✅ SQL Server connection string in `appsettings.json`
3. ✅ Database exists and is accessible
4. ✅ `INFRASTRUCTURE_LAYER.csproj` has required NuGet packages:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `StackExchange.Redis`
