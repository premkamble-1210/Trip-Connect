# **APPLICATION_LAYER - Detailed Documentation**

## **Overview**

The **APPLICATION_LAYER** implements the business logic and data transfer layer for TripConnect. It acts as the bridge between the **API_LAYER** (controllers) and the **INFRASTRUCTURE_LAYER** (repositories). It handles:

- **Business Logic** - Data validation, calculations, workflows
- **Data Transfer Objects (DTOs)** - Input/output data contracts
- **Service Interfaces** - Business operation contracts
- **AutoMapper Profiles** - Entity-DTO transformations
- **Dependency Injection** - Service registration

**Location:** `TripConnect_Backend/TripConnect/APPLICATION_LAYER/`

---

## **Directory Structure**

```
APPLICATION_LAYER/
├── Services/                           # Business logic layer
│   ├── Interfaces/                     # Service contracts
│   │   ├── IUserService.cs             # User operations
│   │   ├── ITripService.cs             # Trip operations
│   │   ├── IJoinRequestService.cs      # Join request operations
│   │   ├── IExpenseService.cs          # Expense operations
│   │   ├── IChatService.cs             # Chat operations
│   │   └── IRatingService.cs           # Rating operations
│   └── Implementations/                # Service implementations
│       ├── UserService.cs              # User logic (66 methods)
│       ├── TripService.cs              # Trip logic
│       ├── JoinRequestService.cs       # Join request logic
│       ├── ExpenseService.cs           # Expense logic
│       ├── ChatService.cs              # Chat logic
│       └── RatingService.cs            # Rating logic
│
├── DTOs/                               # Data Transfer Objects
│   ├── User/                           # User DTOs
│   │   ├── CreateUserDto.cs            # Register input
│   │   ├── LoginUserDto.cs             # Login input
│   │   ├── UpdateUserDto.cs            # Update input
│   │   ├── UserResponseDto.cs          # User output
│   │   └── AuthResponseDto.cs          # Auth response
│   ├── Trip/                           # Trip DTOs
│   │   ├── CreateTripDto.cs
│   │   ├── UpdateTripDto.cs
│   │   └── TripResponseDto.cs
│   ├── JoinRequest/                    # Join request DTOs
│   │   ├── SendJoinRequestDto.cs
│   │   └── JoinRequestResponseDto.cs
│   ├── Expense/                        # Expense DTOs
│   │   ├── CreateExpenseDto.cs
│   │   ├── ExpenseResponseDto.cs
│   │   └── ExpenseSummaryDto.cs
│   ├── Rating/                         # Rating DTOs
│   │   ├── CreateRatingDto.cs
│   │   ├── UpdateRatingDto.cs
│   │   └── RatingResponseDto.cs
│   └── Chat/                           # Chat DTOs
│       ├── SendMessageDto.cs
│       └── ChatMessageResponseDto.cs
│
├── Mappers/                            # AutoMapper Profiles
│   ├── UserProfile.cs                  # User entity mappings
│   ├── TripProfile.cs                  # Trip entity mappings
│   ├── TripRequestProfile.cs           # TripRequest entity mappings
│   ├── ExpenseProfile.cs               # Expense entity mappings
│   ├── ChatMessageProfile.cs           # ChatMessage entity mappings
│   └── TripRatingProfile.cs            # TripRating entity mappings
│
├── DependencyInjection.cs              # Service registration
├── APPLICATION_LAYER.csproj            # Project file
└── [bin/, obj/]                        # Build output
```

---

## **🔧 Service Architecture**

### **Service Pattern**

Each service follows a consistent pattern:

```csharp
public class ServiceName : IServiceName
{
    private readonly IUnitOfWork _unitOfWork;      // Data access
    private readonly ILogger _logger;              // Logging
    private readonly IMapper _mapper;              // DTO mapping

    // Constructor injection
    public ServiceName(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    // Business logic methods
    public async Task<ResponseDto> OperationAsync(RequestDto request)
    {
        try
        {
            _logger.Information("Operation started");
            // Validation
            // Business logic
            // Database operations
            // Return result
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning($"Business logic error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error");
            throw;
        }
    }
}
```

---

## **👤 User Service**

**File:** `Services/Interfaces/IUserService.cs` & `Services/Implementations/UserService.cs`

### **Authentication Methods**

```csharp
// Registration
Task<AuthResponseDto> RegisterAsync(CreateUserDto dto)
    → Validates email/username uniqueness
    → Hashes password with salt
    → Creates new user account
    → Returns authentication token & user data

// Login
Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
    → Finds user by username
    → Validates password hash
    → Returns authentication token
    → Returns 401 if credentials invalid
```

### **User Management Methods**

```csharp
// Retrieval
Task<UserResponseDto> GetUserByIdAsync(int userId)
    → Returns user profile info
Task<UserResponseDto> GetUserByEmailAsync(string email)
    → Looks up user by email
Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    → Returns all users (admin operation)

// Updates
Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserDto dto)
    → Updates name, phone, etc.
    → Validates input

// Verification
Task<bool> VerifyPhoneAsync(int userId)
    → Marks phone as verified
Task<bool> VerifyIdAsync(int userId)
    → Marks ID as verified

// Checks
Task<double> GetUserRatingAsync(int userId)
    → Calculates average rating
Task<bool> EmailExistsAsync(string email)
    → Check if email registered
Task<bool> UsernameExistsAsync(string username)
    → Check if username taken
```

### **Security Features**

1. **Password Hashing:**
   - Uses SHA-256 with salt
   - Hashed password stored, never plain text
   - Salt generated per user

2. **Authentication:**
   - Returns AuthResponseDto with token
   - Used by controllers for authorization

3. **Input Validation:**
   - Email format validation
   - Username uniqueness check
   - Phone format validation

---

## **✈️ Trip Service**

**File:** `Services/Interfaces/ITripService.cs` & `Services/Implementations/TripService.cs`

### **Trip Management Methods**

```csharp
// Create
Task<TripResponseDto> CreateTripAsync(CreateTripDto dto)
    → Creates new trip
    → Sets status to Planned
    → Assigns host user

// Retrieval
Task<TripResponseDto> GetTripByIdAsync(int tripId)
    → Returns full trip details
Task<IEnumerable<TripResponseDto>> GetAllTripsAsync(int pageNumber, int pageSize)
    → Paginated trip listing
Task<IEnumerable<TripResponseDto>> GetTripsByStatusAsync(TripStatus status)
    → Filter by status (Planned, Ongoing, Completed, Cancelled)

// Search & Filter
Task<IEnumerable<TripResponseDto>> GetUpcomingTripsAsync()
    → Returns future trips (StartDate > Now)
    → Sorted by start date
Task<IEnumerable<TripResponseDto>> SearchTripsByLocationAsync(string location)
    → Case-insensitive location search
Task<IEnumerable<TripResponseDto>> SearchTripsAsync(location, startDate, maxBudget, travelType)
    → Multi-criteria search

// User Context
Task<IEnumerable<TripResponseDto>> GetTripsCreatedByUserAsync(int userId)
    → Returns trips created by user
Task<IEnumerable<TripResponseDto>> GetUserTripsAsync(int userId)
    → Returns user's joined trips

// Updates
Task<TripResponseDto> UpdateTripAsync(int tripId, UpdateTripDto dto, int userId)
    → Only host can update
    → Validates ownership
    → Updates details
Task<bool> CancelTripAsync(int tripId, int userId)
    → Sets status to Cancelled
    → Only host can cancel
```

### **Trip Status Workflow**

```
Planned → Ongoing → Completed
         ↘ Cancelled (anytime)
```

---

## **📬 Join Request Service**

**File:** `Services/Interfaces/IJoinRequestService.cs` & `Services/Implementations/JoinRequestService.cs`

### **Join Request Methods**

```csharp
// Request Management
Task<JoinRequestResponseDto> SendJoinRequestAsync(SendJoinRequestDto dto)
    → Creates pending request
    → Checks duplicate requests
    → Sets RequestedAt timestamp

// Retrieval
Task<JoinRequestResponseDto> GetJoinRequestByIdAsync(int requestId)
    → Returns request details
Task<IEnumerable<JoinRequestResponseDto>> GetPendingRequestsByTripAsync(int tripId)
    → Returns pending requests for trip
    → Only for trip host
Task<IEnumerable<JoinRequestResponseDto>> GetRequestsByUserAsync(int userId)
    → Returns user's join requests
Task<IEnumerable<JoinRequestResponseDto>> GetAllRequestsByTripAsync(int tripId)
    → Returns all requests (all statuses)

// Decision Handling
Task<bool> AcceptJoinRequestAsync(int requestId, int hostId)
    → Changes status to Accepted
    → Adds user to TripMembers
    → Only host can accept
Task<bool> RejectJoinRequestAsync(int requestId, int hostId)
    → Changes status to Rejected
    → Sets RespondedAt timestamp
    → Only host can reject
Task<bool> CancelJoinRequestAsync(int requestId, int userId)
    → Changes status to Cancelled
    → User can cancel own request

// Checks
Task<bool> HasUserRequestedAsync(int userId, int tripId)
    → Checks if user already requested
    → Prevents duplicate requests
```

### **Request Status Workflow**

```
Pending → Accepted (user joins trip)
      → Rejected (user cannot join)
      → Cancelled (user withdraws)
```

---

## **💰 Expense Service**

**File:** `Services/Interfaces/IExpenseService.cs` & `Services/Implementations/ExpenseService.cs`

### **Expense Management Methods**

```csharp
// Create
Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto, int userId)
    → Creates expense
    → User is PaidBy
    → Creates expense splits

// Retrieval
Task<ExpenseResponseDto> GetExpenseByIdAsync(int expenseId)
    → Returns expense details
Task<IEnumerable<ExpenseResponseDto>> GetExpensesByTripAsync(int tripId)
    → Returns all trip expenses
Task<IEnumerable<ExpenseResponseDto>> GetExpensesPaidByUserAsync(int userId)
    → Returns expenses paid by user

// Calculations
Task<decimal> GetTotalExpensesByTripAsync(int tripId)
    → Sums all expenses in trip
Task<decimal> GetTotalOwedByUserAsync(int userId, int tripId)
    → Calculates user's debt in trip
Task<IEnumerable<ExpenseSplitResponseDto>> GetUnsettledExpensesByUserAsync(int userId, int tripId)
    → Returns unpaid splits

// Settlement
Task<bool> SettleExpenseAsync(int splitId)
    → Marks split as settled/paid
    → Updates IsSettled flag
    → Reduces user's debt

// Summary
Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int tripId)
    → Returns total expenses
    → Returns all member balances
    → Shows who owes whom

// Deletion
Task<bool> DeleteExpenseAsync(int expenseId, int userId)
    → Only expense creator can delete
    → Removes expense and splits
```

### **Expense Flow**

```
1. User creates expense (PaidBy = their ID)
2. System creates splits for all trip members
3. Each member gets ExpenseSplit record
4. Members settle their portion (IsSettled = true)
5. Summary shows overall trip balance
```

---

## **💬 Chat Service**

**File:** `Services/Interfaces/IChatService.cs` & `Services/Implementations/ChatService.cs`

### **Message Methods**

```csharp
// Send
Task<ChatMessageResponseDto> SendMessageAsync(SendMessageDto dto, int userId)
    → Creates message
    → SenderId = userId
    → Sets CreatedAt timestamp

// Retrieval
Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripAsync(int tripId)
    → Returns all trip chat messages
Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripPaginatedAsync(int tripId, int page, int size)
    → Paginated message retrieval
    → Newer messages first (descending)
Task<IEnumerable<ChatMessageResponseDto>> GetLatestMessagesAsync(int tripId, int count)
    → Returns last N messages
    → Useful for chat UI initial load

// User Context
Task<IEnumerable<ChatMessageResponseDto>> GetMessagesBySenderAsync(int userId)
    → Returns messages sent by user

// Search & Count
Task<IEnumerable<ChatMessageResponseDto>> SearchMessagesAsync(int tripId, string searchText)
    → Case-insensitive message search
Task<int> GetMessageCountAsync(int tripId)
    → Returns total messages in trip

// Deletion
Task<bool> DeleteMessageAsync(int messageId, int userId)
    → Only message author can delete
    → Soft delete recommended
```

---

## **⭐ Rating Service**

**File:** `Services/Interfaces/IRatingService.cs` & `Services/Implementations/RatingService.cs`

### **Rating Methods**

```csharp
// Create
Task<RatingResponseDto> CreateRatingAsync(CreateRatingDto dto)
    → Creates rating (1.0-5.0)
    → Validates rating range
    → Prevents duplicate ratings

// Retrieval
Task<RatingResponseDto> GetRatingByIdAsync(int ratingId)
    → Returns rating details
Task<IEnumerable<RatingResponseDto>> GetRatingsForUserAsync(int userId)
    → Returns ratings received by user
Task<IEnumerable<RatingResponseDto>> GetRatingsGivenByUserAsync(int userId)
    → Returns ratings given by user
Task<IEnumerable<RatingResponseDto>> GetRatingsForTripAsync(int tripId)
    → Returns all ratings for a trip

// Calculations
Task<double> GetAverageRatingAsync(int userId)
    → Calculates average rating
    → Uses all ratings received
Task<int> GetRatingsCountAsync(int userId)
    → Returns total ratings received

// Checks
Task<bool> HasUserRatedAsync(int ratedBy, int ratedUserId, int tripId)
    → Checks if user already rated someone
    → Prevents duplicate ratings

// Updates
Task<RatingResponseDto> UpdateRatingAsync(int ratingId, UpdateRatingDto dto, int userId)
    → Updates rating value & review
    → Only rater can update
    → Recalculates average

// Deletion
Task<bool> DeleteRatingAsync(int ratingId, int userId)
    → Only rater can delete
    → Recalculates average
```

### **Rating Validation**

- **Range:** 1.0 to 5.0 (decimal)
- **Uniqueness:** One rating per (ratedBy, ratedUserId, trip) combination
- **Context:** Ratings are trip-specific

---

## **📦 Data Transfer Objects (DTOs)**

### **DTO Purpose**

- **Input Validation** - Controllers validate DTOs
- **Data Security** - Don't expose sensitive fields
- **API Contract** - Define request/response structure
- **Mapping** - AutoMapper transforms entities ↔ DTOs

### **User DTOs**

```csharp
// Input DTOs
CreateUserDto
{
    string Name              // Required
    string Email            // Required, unique
    string Username         // Required, unique
    string Phone            // Required
    string Password         // Required, hashed
}

LoginUserDto
{
    string Username         // Required
    string Password         // Required
}

UpdateUserDto
{
    string Name             // Optional
    string Phone            // Optional
}

// Output DTOs
UserResponseDto
{
    int Id
    string Name
    string Email
    string Username
    string Phone
    double Rating
    bool PhoneVerified
    bool IdVerified
    DateTime CreatedAt
}

AuthResponseDto
{
    bool Success
    string Message
    UserResponseDto User
    string Token            // JWT token
}
```

### **Trip DTOs**

```csharp
CreateTripDto
{
    string Title            // Required
    string Description      // Required
    string Location         // Required
    decimal Budget          // Required
    DateTime StartDate
    DateTime EndDate
    int Seats
    string TravelType       // (Trekking, Beach, etc.)
}

UpdateTripDto
{
    string Title
    string Description
    decimal Budget
    int Seats
    string TravelType
}

TripResponseDto
{
    int Id
    string Title
    string Description
    string Location
    decimal Budget
    DateTime StartDate
    DateTime EndDate
    int Seats
    string TravelType
    TripStatus Status       // Enum
    int HostId
    DateTime CreatedAt
}
```

### **Expense DTOs**

```csharp
CreateExpenseDto
{
    int TripId
    decimal Amount
    string Description      // Category
}

ExpenseResponseDto
{
    int Id
    int TripId
    int PaidBy
    decimal Amount
    string Description
    DateTime CreatedAt
    IEnumerable<ExpenseSplitDto> Splits
}

ExpenseSummaryDto
{
    int TripId
    decimal TotalExpenses
    IEnumerable<UserBalanceDto> UserBalances
    /* UserBalanceDto:
       {
           int UserId,
           decimal Paid,           // Amount paid by user
           decimal Owed,           // Amount owed by user
           decimal Balance         // Paid - Owed (+ = owed to, - = owed by)
       }
    */
}
```

### **Chat & Rating DTOs**

```csharp
SendMessageDto
{
    int TripId
    string Message          // Required
}

ChatMessageResponseDto
{
    int Id
    int TripId
    int SenderId
    string Message
    DateTime CreatedAt
}

CreateRatingDto
{
    int TripId
    int RatedUserId
    double Rating           // 1.0-5.0
    string Review           // Optional
}

RatingResponseDto
{
    int Id
    int TripId
    int RatedBy
    int RatedUserId
    double Rating
    string Review
    DateTime CreatedAt
}
```

---

## **🗺️ AutoMapper Profiles**

### **Mapping Configuration Pattern**

```csharp
public class EntityProfile : Profile
{
    public EntityProfile()
    {
        // Entity → ResponseDto
        CreateMap<Entity, ResponseDto>()
            .ForMember(dest => dest.Property1, 
                opt => opt.MapFrom(src => src.Property1))
            // ... more mappings

        // CreateDto → Entity
        CreateMap<CreateDto, Entity>()
            .ForMember(dest => dest.CreatedAt, 
                opt => opt.MapFrom(src => DateTime.UtcNow))

        // UpdateDto → Entity (conditional)
        CreateMap<UpdateDto, Entity>()
            .ForMember(dest => dest.Property, 
                opt => opt.Condition(src => !string.IsNullOrEmpty(src.Property)))
    }
}
```

### **6 Mapper Profiles**

1. **UserProfile** - User ↔ CreateUserDto ↔ UpdateUserDto ↔ UserResponseDto
2. **TripProfile** - Trip ↔ CreateTripDto ↔ UpdateTripDto ↔ TripResponseDto
3. **TripRequestProfile** - TripRequest → JoinRequestResponseDto
4. **ExpenseProfile** - Expense → ExpenseResponseDto & ExpenseSplit mappings
5. **ChatMessageProfile** - ChatMessage ↔ SendMessageDto ↔ ChatMessageResponseDto
6. **TripRatingProfile** - TripRating ↔ CreateRatingDto ↔ UpdateRatingDto ↔ RatingResponseDto

---

## **🔌 Dependency Injection Configuration**

### **AddApplicationLayer Extension Method**

**Located in:** `DependencyInjection.cs`

```csharp
public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
{
    // 1. Configure and register Serilog logger
    var logger = LoggingConfiguration.ConfigureLogger();
    Log.Logger = logger;
    services.AddSingleton<Serilog.ILogger>(logger);

    // 2. Register all 6 service implementations
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITripService, TripService>();
    services.AddScoped<IJoinRequestService, JoinRequestService>();
    services.AddScoped<IExpenseService, ExpenseService>();
    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<IRatingService, RatingService>();

    // 3. Configure AutoMapper with 6 profiles
    services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<UserProfile>();
        cfg.AddProfile<TripProfile>();
        cfg.AddProfile<TripRequestProfile>();
        cfg.AddProfile<ExpenseProfile>();
        cfg.AddProfile<ChatMessageProfile>();
        cfg.AddProfile<TripRatingProfile>();
    });

    return services;
}
```

**Lifetime:** Scoped (one instance per HTTP request)

---

## **🔐 Business Logic Patterns**

### **1. Validation Pattern**

```csharp
public async Task<ResponseDto> OperationAsync(RequestDto request)
{
    // Input validation
    if (request.SomeValue < 0)
        throw new InvalidOperationException("Value must be positive");

    // Database validation
    var entity = await _unitOfWork.Repo.GetByIdAsync(request.Id);
    if (entity == null)
        throw new InvalidOperationException("Entity not found");

    // Business rule validation
    if (entity.Status == Status.Completed)
        throw new InvalidOperationException("Cannot modify completed item");

    // Proceed with operation
}
```

### **2. Authorization Pattern**

```csharp
public async Task<ResponseDto> OperationAsync(int resourceId, int userId)
{
    var resource = await _unitOfWork.Repo.GetByIdAsync(resourceId);
    
    // Ownership check
    if (resource.OwnerId != userId)
        throw new InvalidOperationException("Unauthorized - not owner");

    // Or role check
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (!IsAdmin(user))
        throw new InvalidOperationException("Unauthorized - admin only");

    // Proceed
}
```

### **3. Transaction Pattern**

```csharp
public async Task<ResponseDto> ComplexOperationAsync(RequestDto request)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        // Multiple operations
        await _unitOfWork.Entity1.AddAsync(entity1);
        await _unitOfWork.Entity2.AddAsync(entity2);
        
        // Atomic save
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();

        _logger.Information("Operation committed successfully");
        return new ResponseDto { Success = true };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.Error(ex, "Operation rolled back");
        throw;
    }
}
```

---

## **📊 Service Method Statistics**

| Service | Interface Methods | Implementation Methods |
|---------|------------------|----------------------|
| UserService | 11 | 66+ (with helpers) |
| TripService | 11 | 50+ |
| JoinRequestService | 9 | 40+ |
| ExpenseService | 10 | 45+ |
| ChatService | 8 | 35+ |
| RatingService | 9 | 40+ |
| **Total** | **58** | **275+** |

---

## **🔄 Data Flow Example: User Registration**

```
1. API_LAYER
   POST /api/users/register
   → Controller receives CreateUserDto
   → ModelState validation
   
2. APPLICATION_LAYER (UserService)
   RegisterAsync(CreateUserDto)
   → Validate email/username uniqueness
   → Hash password
   → Create User entity
   → Call unitOfWork.Users.AddAsync()
   → Call unitOfWork.SaveChangesAsync()
   → Mapper: User → UserResponseDto
   → Return AuthResponseDto with token
   
3. INFRASTRUCTURE_LAYER
   UserRepository.AddAsync()
   → Add to DbContext
   → SaveChangesAsync()
   → SQL: INSERT INTO Users...
   
4. DATABASE
   TripConnect database
   → Stores user record
   → Returns ID (auto-increment)
   
5. Back through layers
   → Response returned to controller
   → 201 Created status
   → User object in response body
```

---

## **⚡ Performance Optimizations**

1. **Lazy Loading in UnitOfWork** - Services loaded only when needed
2. **Async/Await** - Non-blocking I/O operations
3. **Connection Pooling** - Reused database connections
4. **AutoMapper Caching** - Compiled mappings cached
5. **Query Optimization** - EF Core translates to efficient SQL
6. **Logging Levels** - Warning level for Framework noise

---

## **🛡️ Security Features**

1. **Password Hashing** - SHA-256 with salt
2. **Input Validation** - All DTOs validated
3. **Authorization Checks** - Ownership verification
4. **SQL Injection Prevention** - ORM parameterized queries
5. **Error Masking** - Generic messages to clients
6. **Logging** - Audit trail of operations

---

## **Key Characteristics**

✅ **Separation of Concerns** - Logic separated from presentation
✅ **Testability** - Interfaces enable mocking
✅ **Maintainability** - Clear patterns and conventions
✅ **Scalability** - Service-based architecture
✅ **Consistency** - Uniform error handling
✅ **Logging** - Comprehensive audit trail
✅ **Async Operations** - Non-blocking throughout
✅ **Data Validation** - Multi-layer validation

---

## **Integration Points**

1. **Receives from:** API_LAYER (controllers)
2. **Depends on:** INFRASTRUCTURE_LAYER (repositories)
3. **Uses:** DOMAIN_LAYER (entities, enums)
4. **Provides to:** API_LAYER (response DTOs)

---

This is the **APPLICATION_LAYER** - the intelligent business logic engine of TripConnect! ✅
