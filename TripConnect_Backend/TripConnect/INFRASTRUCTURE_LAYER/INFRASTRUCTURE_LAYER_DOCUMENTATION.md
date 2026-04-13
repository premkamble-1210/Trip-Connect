# **INFRASTRUCTURE_LAYER - Detailed Documentation**

## **Overview**

The **INFRASTRUCTURE_LAYER** implements the data access patterns and infrastructure services for the TripConnect application. It bridges the gap between the **DOMAIN_LAYER** (interfaces) and the database, following the **Repository Pattern** and **Unit of Work Pattern**.

**Location:** `TripConnect_Backend/TripConnect/INFRASTRUCTURE_LAYER/`

---

## **Directory Structure**

```
INFRASTRUCTURE_LAYER/
├── Data/                           # Entity Framework Core configuration
│   ├── TripConnectDbContext.cs     # DbContext configuration
│   └── TripConnectDbContextFactory.cs  # Design-time factory for migrations
├── Repository/                     # Repository implementations
│   ├── Repository.cs               # Generic repository base class
│   ├── UserRepository.cs           # User-specific repository
│   ├── TripRepository.cs           # Trip-specific repository
│   ├── TripRequestRepository.cs    # Trip request repository
│   ├── TripMemberRepository.cs     # Trip member repository
│   ├── ExpenseRepository.cs        # Expense repository
│   ├── ExpenseSplitRepository.cs   # Expense split repository
│   ├── ChatRepository.cs           # Chat message repository
│   ├── TripRatingRepository.cs     # Trip rating repository
│   └── UnitOfWork.cs               # Unit of Work implementation
├── Logging/                        # Serilog logging configuration
│   └── LoggingConfiguration.cs     # Logging setup
├── Migrations/                     # Entity Framework migrations
│   └── [*.cs files]                # Auto-generated migration files
├── DependencyInjection.cs          # Service registration
├── INFRASTRUCTURE_LAYER.csproj     # Project file
└── [bin/, obj/]                    # Build output
```

---

## **🗄️ Data Access Layer**

### **TripConnectDbContext** - Entity Framework Core DbContext

**Purpose:** Manages database connections and entity mappings

**Key Features:**

1. **DbSets (8 entities mapped to tables):**
   - `DbSet<User>` - Users table
   - `DbSet<Trip>` - Trips table
   - `DbSet<TripRequest>` - Trip join requests table
   - `DbSet<TripMember>` - Trip members table
   - `DbSet<Expense>` - Expenses table
   - `DbSet<ExpenseSplit>` - Expense splits table
   - `DbSet<ChatMessage>` - Chat messages table
   - `DbSet<TripRating>` - User ratings table

2. **OnModelCreating Configuration:**
   - Primary keys definition
   - Unique constraints (Email, Username)
   - Foreign key relationships with cascading/restrict delete behaviors
   - Table indexing for performance

**Database Provider:**
- SQL Server via Entity Framework Core
- Connection string: Located in `appsettings.json`
- Configured in `DependencyInjection.cs`

---

### **TripConnectDbContextFactory** - Design-Time Context Factory

**Purpose:** Enables Entity Framework migrations at design-time

**Features:**
- Implements `IDesignTimeDbContextFactory<TripConnectDbContext>`
- Reads connection string from `appsettings.json`
- Allows running migrations without running the application
- Uses SQL Server connection string: `(localdb)\\MSSQLLocalDB`

**Usage:**
```csharp
// Migrations use this factory automatically
Add-Migration InitialCreate
Update-Database
```

---

## **📦 Repository Pattern Implementation**

### **Generic Repository<T>** - Base Class

**Purpose:** Provides common CRUD operations for all entities

**Implemented Methods:**

```csharp
// Read Operations
public Task<T> GetByIdAsync(int id)           // Get by primary key
public Task<IEnumerable<T>> GetAllAsync()     // Get all records

// Write Operations
public Task AddAsync(T entity)                // Add new entity
public Task UpdateAsync(T entity)             // Update existing entity
public Task DeleteAsync(int id)               // Delete by ID
public Task SaveChangesAsync()                // Save changes to DB

// Query Operations
public Task<bool> ExistsAsync(int id)         // Check if exists
```

**Implementation Details:**
- Uses `DbContext` for database operations
- Lazy-loads `DbSet<T>` for the entity type
- Fully asynchronous (all methods are `Task`-based)
- Virtual methods - can be overridden in derived classes

---

### **Specific Repository Implementations**

#### **1. UserRepository** 👤
**Base:** `Repository<User>`

**Specialized Methods:**
- `GetByEmailAsync(string email)` - Get user by email
- `GetByPhoneAsync(string phone)` - Get user by phone
- `GetByUsernameAsync(string username)` - Get user by username for auth
- `GetVerifiedUsersAsync()` - Get all verified users (phone + ID)
- `UpdateVerificationStatusAsync(userId, phoneVerified, idVerified)` - Update verification
- `UpdateRatingAsync(userId, rating)` - Update user rating
- `EmailExistsAsync(string email)` - Check if email exists
- `UsernameExistsAsync(string username)` - Check if username exists

---

#### **2. TripRepository** ✈️
**Base:** `Repository<Trip>`

**Specialized Methods:**
- `GetTripsByStatusAsync(TripStatus status)` - Get trips by status
- `GetTripsByLocationAsync(string location)` - Search by location
- `GetTripsByHostAsync(int hostId)` - Get trips by organizer
- `GetUpcomingTripsAsync()` - Get future trips (Planned status, StartDate > today)
- `GetTripsByBudgetAsync(decimal min, decimal max)` - Filter by budget range
- `GetTripsByTravelTypeAsync(string travelType)` - Filter by travel type
- `SearchTripsAsync(location, startDate, maxBudget, travelType)` - Multi-criteria search

---

#### **3. TripRequestRepository** 📬
**Base:** `Repository<TripRequest>`

**Specialized Methods:**
- `GetRequestsByTripAsync(int tripId)` - Get requests for a trip
- `GetRequestsByUserAsync(int userId)` - Get user's requests
- `GetPendingRequestsAsync(int tripId)` - Get pending requests only
- `GetRequestByStatusAsync(TripRequestStatus status)` - Filter by status
- `HasUserRequestedAsync(int userId, int tripId)` - Check if user already requested

---

#### **4. TripMemberRepository** 👥
**Base:** `Repository<TripMember>`

**Specialized Methods:**
- `GetMembersByTripAsync(int tripId)` - Get all members of a trip
- `GetMembershipsByUserAsync(int userId)` - Get user's trip memberships
- `GetActiveMembersAsync(int tripId)` - Get active members only
- `GetMemberByUserAndTripAsync(int userId, int tripId)` - Get specific membership
- `GetMembersCountAsync(int tripId)` - Count members in a trip

---

#### **5. ExpenseRepository** 💰
**Base:** `Repository<Expense>`

**Specialized Methods:**
- `GetExpensesByTripAsync(int tripId)` - Get trip expenses
- `GetExpensesPaidByUserAsync(int userId)` - Get expenses paid by user
- `GetTotalExpensesByTripAsync(int tripId)` - Calculate total trip expenses
- `GetExpensesBetweenDatesAsync(DateTime start, DateTime end)` - Date range filter
- `GetExpenseByCategoryAsync(string category)` - Filter by description/category

---

#### **6. ExpenseSplitRepository** 🧾
**Base:** `Repository<ExpenseSplit>`

**Specialized Methods:**
- `GetSplitsByExpenseAsync(int expenseId)` - Get splits for an expense
- `GetSplitsByUserAsync(int userId)` - Get user's expense splits
- `GetUnsettledSplitsAsync(int userId)` - Get unsettled splits (unpaid)
- `GetSettledSplitsAsync(int userId)` - Get settled splits (paid)
- `GetTotalOwedByUserAsync(int userId)` - Calculate total owed by user
- `MarkAsSettledAsync(int splitId)` - Mark split as paid

---

#### **7. ChatRepository** 💬
**Base:** `Repository<ChatMessage>`

**Specialized Methods:**
- `GetMessagesByTripAsync(int tripId)` - Get trip chat messages
- `GetMessagesBySenderAsync(int userId)` - Get messages from user
- `GetMessagesBetweenDatesAsync(DateTime start, DateTime end)` - Date range messages
- `SearchMessagesAsync(int tripId, string searchText)` - Search messages
- `GetMessageCountAsync(int tripId)` - Count messages in trip
- `GetLatestMessagesAsync(int tripId, int count)` - Get most recent N messages

---

#### **8. TripRatingRepository** ⭐
**Base:** `Repository<TripRating>`

**Specialized Methods:**
- `GetRatingsForUserAsync(int userId)` - Get ratings received by user
- `GetRatingsGivenByUserAsync(int userId)` - Get ratings given by user
- `GetRatingsByTripAsync(int tripId)` - Get ratings for a trip
- `GetAverageRatingAsync(int userId)` - Calculate average user rating
- `HasUserRatedAsync(int ratedBy, int ratedUserId, int tripId)` - Check if rated
- `GetRatingsCountAsync(int userId)` - Count ratings received

---

## **🔄 Unit of Work Pattern**

### **UnitOfWork Implementation**

**Purpose:** Manages all repositories and coordinates transactions

**Properties (Lazy-Loaded Repository Instances):**
```csharp
IUserRepository Users              // User operations
ITripRepository Trips              // Trip operations
ITripRequestRepository TripRequests // Join requests
ITripMemberRepository TripMembers   // Trip members
IExpenseRepository Expenses         // Expenses
IExpenseSplitRepository ExpenseSplits // Splits
IChatRepository ChatMessages        // Messages
ITripRatingRepository TripRatings   // Ratings
```

**Transaction Methods:**

```csharp
// Transactions
public Task<int> SaveChangesAsync()        // Commit all changes
public Task BeginTransactionAsync()        // Start transaction
public Task CommitTransactionAsync()       // Commit transaction
public Task RollbackTransactionAsync()     // Rollback transaction
public bool HasActiveTransaction { get; }  // Check active transaction
```

**Implementation Pattern:**
- Uses **Lazy Initialization** (`??=` operator) for repositories
- Each repository accessed only when needed
- Single DbContext instance shared across all repositories
- Implements `IDisposable` for resource cleanup

**Usage Example:**
```csharp
using (var unitOfWork = new UnitOfWork(dbContext))
{
    // Create user
    var user = new User { Email = "test@example.com", ... };
    await unitOfWork.Users.AddAsync(user);
    
    // Create trip
    var trip = new Trip { HostId = user.Id, ... };
    await unitOfWork.Trips.AddAsync(trip);
    
    // Save all changes atomically
    await unitOfWork.SaveChangesAsync();
}
```

---

## **📝 Dependency Injection Configuration**

### **AddInfrastructure Extension Method**

**Located in:** `DependencyInjection.cs`

**Registration Steps:**

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // 1. Configure Serilog logging
    Log.Logger = LoggingConfiguration.ConfigureLogger();

    // 2. Register DbContext with SQL Server
    services.AddDbContext<TripConnectDbContext>(options =>
        options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection")));

    // 3. Register generic DbContext
    services.AddScoped<DbContext>(provider => 
        provider.GetRequiredService<TripConnectDbContext>());

    // 4. Register generic Repository<T>
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    // 5. Register specific repositories (8 total)
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<ITripRepository, TripRepository>();
    // ... (6 more repositories)

    // 6. Register Unit of Work
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
}
```

**Lifetime Scope:**
- **Scoped** - One instance per HTTP request
- Ensures data consistency within a request
- Automatically disposed when request completes

---

## **📊 Logging Configuration**

### **Serilog Setup**

**Located in:** `Logging/LoggingConfiguration.cs`

**Features:**

1. **Log Levels:**
   - **Minimum:** Information level
   - **Microsoft/EF Core:** Warning level (reduce noise)
   - **System:** Warning level

2. **Output:**
   - **File:** `logs/tripconnect-[date].txt`
   - **Rolling Interval:** Daily (new file each day)
   - **Retention:** 7 days only
   - **Format:** `{Timestamp} [{Level}] {Message}\n{Exception}`

3. **Helper Methods:**
   ```csharp
   LoggingConfiguration.ConfigureLogger()     // Initialize logger
   LoggingConfiguration.GetLogger<T>()        // Get typed logger
   LoggingConfiguration.LogInfo(message)      // Log info
   LoggingConfiguration.LogWarning(message)   // Log warning
   LoggingConfiguration.LogError(ex, msg)     // Log error
   LoggingConfiguration.LogCritical(ex, msg)  // Log critical
   ```

4. **Integration:**
   - Used by all services and repositories
   - Injected via Serilog `ILogger` interface
   - Configured on application startup

---

## **🗄️ Entity Framework Migrations**

### **Database Migrations Directory**

**Located in:** `Migrations/`

**Generated Files:**

- `[timestamp]_InitialCreate.cs` - Initial schema creation
- `[timestamp]_InitialCreate.Designer.cs` - Migration metadata
- `TripConnectDbContextModelSnapshot.cs` - Current schema snapshot

**Migration Commands:**

```powershell
# Create new migration
Add-Migration MigrationName

# Apply migration to database
Update-Database

# Revert migration
Update-Database -Migration PreviousMigrationName

# List migrations
Get-Migration
```

**Automatic Features:**
- OnModelCreating configures all relationships
- Cascading/Restricted deletes configured per entity
- Unique constraints on Email and Username
- Indexes created for performance

---

## **🔗 Relationship Configuration**

### **One-to-Many Relationships**

```
User (1) ──────────→ (M) Trip (HostId)
User (1) ──────────→ (M) TripRequest
User (1) ──────────→ (M) TripMember
User (1) ──────────→ (M) Expense (PaidBy)
User (1) ──────────→ (M) ExpenseSplit
User (1) ──────────→ (M) ChatMessage
Trip (1) ──────────→ (M) TripRequest
Trip (1) ──────────→ (M) TripMember
Trip (1) ──────────→ (M) Expense
Trip (1) ──────────→ (M) ChatMessage
Trip (1) ──────────→ (M) TripRating
Expense (1) ───────→ (M) ExpenseSplit
```

### **Delete Behaviors**

- **Restrict:** Foreign key can't be deleted if child records exist
  - User → Trip (Host restriction)
  - User → Expense (PaidBy restriction)

- **Cascade:** Child records deleted when parent deleted
  - User → TripRequest
  - Trip → TripMember, Expense, ChatMessage

---

## **⚡ Performance Considerations**

1. **Lazy Loading:**
   - Repositories loaded only when accessed
   - Reduces initialization time

2. **Async/Await:**
   - Non-blocking database operations
   - Better resource utilization

3. **Indexing:**
   - Email and Username indexed (unique)
   - Foreign keys automatically indexed

4. **Query Optimization:**
   - LINQ queries translated to SQL by EF Core
   - `.ToListAsync()` executes query immediately
   - Can add `.Include()` for eager loading if needed

5. **Logging:**
   - Warning level for Microsoft/EF Core
   - Reduces log file size
   - Only logs critical app events

---

## **🔐 Data Validation**

**Enforced at Database Level:**
- Primary key uniqueness
- Foreign key constraints
- Email/Username uniqueness
- Non-nullable required fields

**Enforced at Application Level:**
- Service layer validation
- DTOs for input validation
- Null checks before operations

---

## **Key Characteristics**

✅ **Fully Asynchronous** - All operations are async
✅ **Repository Pattern** - Abstracted data access
✅ **Unit of Work** - Transaction coordination
✅ **Lazy Loading** - Repositories loaded on demand
✅ **Dependency Injection** - Loosely coupled services
✅ **Logging** - Serilog integration for audit trails
✅ **Migration Support** - EF Core code-first migrations
✅ **Transaction Support** - Explicit transaction control

---

## **Integration Points**

1. **Connects to:** DOMAIN_LAYER (interfaces)
2. **Used by:** APPLICATION_LAYER (services)
3. **Configuration:** API_LAYER (Program.cs)
4. **Database:** SQL Server via LocalDB or production server

---

This is the **INFRASTRUCTURE_LAYER** - the data persistence backbone of TripConnect! ✅
