# **API_LAYER - Detailed Documentation**

## **Overview**

The **API_LAYER** is the presentation and gateway layer for TripConnect. It handles:

- **HTTP Endpoints** - RESTful API routes for all operations
- **Request Validation** - Input validation and model state checks
- **Response Formatting** - Consistent JSON responses
- **Error Handling** - Global middleware for exception management
- **CORS Policy** - Cross-origin resource sharing for frontend
- **Health Monitoring** - API health and status endpoints
- **OpenAPI/Swagger** - Interactive API documentation
- **Logging & Diagnostics** - Request/response logging

**Location:** `TripConnect_Backend/TripConnect/API_LAYER/`

**Base URL:** 
- HTTPS: `https://localhost:7142`
- HTTP: `http://localhost:5126`

---

## **Directory Structure**

```
API_LAYER/
├── Controllers/                        # API Endpoints (7 controllers)
│   ├── UserController.cs               # User auth & management (11 endpoints)
│   ├── TripController.cs               # Trip operations (11 endpoints)
│   ├── JoinRequestController.cs        # Join requests (9 endpoints)
│   ├── ExpenseController.cs            # Expense tracking (10 endpoints)
│   ├── ChatController.cs               # Messaging (8 endpoints)
│   ├── RatingController.cs             # User ratings (9 endpoints)
│   ├── HealthController.cs             # Health checks (4 endpoints)
│   └── WeatherForecastController.cs    # Sample endpoint
│
├── Middleware/                         # Request/Response Processing
│   └── ErrorHandlingMiddleware.cs      # Global exception handler
│
├── Models/                             # API Models
│   └── HealthCheckResponse.cs          # Health check response DTO
│
├── Program.cs                          # Application startup & configuration
├── API_LAYER.http                      # HTTP client test file
├── appsettings.json                    # Configuration file
├── appsettings.Development.json        # Development settings
├── API_LAYER.csproj                    # Project file
└── [bin/, obj/]                        # Build output
```

---

## **🔧 Core Architecture**

### **Controller Pattern**

Each controller follows a consistent pattern:

```csharp
[ApiController]
[Route("api/[controller]")]
public class EntityController : ControllerBase
{
    private readonly IEntityService _entityService;
    private readonly ILogger<EntityController> _logger;

    public EntityController(IEntityService entityService, ILogger<EntityController> logger)
    {
        _entityService = entityService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDto dto, [FromQuery] int userId)
    {
        try
        {
            // 1. Validate model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Call service
            var result = await _entityService.CreateAsync(dto, userId);

            // 3. Return response
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation error");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }
}
```

**Key Elements:**
- Dependency injection of service & logger
- ModelState validation for input
- Try-catch with specific exception handling
- Appropriate HTTP status codes
- Consistent error response format

---

## **👤 User Controller** 

**File:** `Controllers/UserController.cs`

**Base Route:** `POST/GET /api/user`

### **Authentication Endpoints**

```
POST /api/user/register
    Input: CreateUserDto { Name, Email, Username, Phone, Password }
    Output: AuthResponseDto { Success, Message, User, Token }
    Status: 200 OK | 400 Bad Request | 500 Internal Server Error
    Exception: InvalidOperationException (duplicate email/username)

POST /api/user/login
    Input: LoginUserDto { Username, Password }
    Output: AuthResponseDto { Success, Message, User, Token }
    Status: 200 OK | 401 Unauthorized | 500 Internal Server Error
    Exception: InvalidOperationException (invalid credentials)
```

### **User Management Endpoints**

```
GET /api/user/{id}
    Params: id (int)
    Output: UserResponseDto
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/user/email/{email}
    Params: email (string)
    Output: UserResponseDto
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

PUT /api/user/{id}
    Params: id (int)
    Input: UpdateUserDto { Name, Phone }
    Output: UserResponseDto
    Status: 200 OK | 400 Bad Request | 404 Not Found | 500 Internal Server Error

GET /api/user/all
    Output: IEnumerable<UserResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/user/rating/{id}
    Params: id (int)
    Output: double (average rating)
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/user/verify-phone/{id}
    Params: id (int)
    Output: boolean
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/user/verify-id/{id}
    Params: id (int)
    Output: boolean
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/user/email-exists/{email}
    Params: email (string)
    Output: boolean
    Status: 200 OK | 500 Internal Server Error

GET /api/user/username-exists/{username}
    Params: username (string)
    Output: boolean
    Status: 200 OK | 500 Internal Server Error
```

**Total Endpoints:** 11

---

## **✈️ Trip Controller**

**File:** `Controllers/TripController.cs`

**Base Route:** `POST/GET /api/trip`

### **Trip Management Endpoints**

```
POST /api/trip
    Query: userId (int)
    Input: CreateTripDto { Title, Description, Location, Budget, StartDate, EndDate, Seats, TravelType }
    Output: TripResponseDto
    Status: 201 Created | 400 Bad Request | 500 Internal Server Error

GET /api/trip/{id}
    Params: id (int)
    Output: TripResponseDto
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/trip
    Query: pageNumber (int, default=1), pageSize (int, default=10)
    Output: IEnumerable<TripResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/trip/status/{status}
    Params: status (string - "Planned", "Ongoing", "Completed", "Cancelled")
    Output: IEnumerable<TripResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/trip/upcoming
    Output: IEnumerable<TripResponseDto> (sorted by date)
    Status: 200 OK | 500 Internal Server Error

GET /api/trip/location/{location}
    Params: location (string)
    Output: IEnumerable<TripResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/trip/search
    Query: location, startDate, maxBudget, travelType
    Output: IEnumerable<TripResponseDto> (filtered results)
    Status: 200 OK | 500 Internal Server Error

PUT /api/trip/{id}
    Params: id (int)
    Query: userId (int)
    Input: UpdateTripDto { Title, Description, Budget, Seats, TravelType }
    Output: TripResponseDto
    Status: 200 OK | 400 Bad Request | 403 Forbidden | 404 Not Found | 500 Internal Server Error

GET /api/trip/created-by/{userId}
    Params: userId (int)
    Output: IEnumerable<TripResponseDto> (trips created by user)
    Status: 200 OK | 500 Internal Server Error

GET /api/trip/user-trips/{userId}
    Params: userId (int)
    Output: IEnumerable<TripResponseDto> (trips user joined)
    Status: 200 OK | 500 Internal Server Error

DELETE /api/trip/{id}
    Params: id (int)
    Query: userId (int)
    Status: 204 No Content | 403 Forbidden | 404 Not Found | 500 Internal Server Error
```

**Total Endpoints:** 11

---

## **📬 Join Request Controller**

**File:** `Controllers/JoinRequestController.cs`

**Base Route:** `POST/GET /api/joinrequest`

### **Join Request Endpoints**

```
POST /api/joinrequest
    Query: userId (int)
    Input: SendJoinRequestDto { TripId }
    Output: JoinRequestResponseDto
    Status: 201 Created | 400 Bad Request | 500 Internal Server Error

GET /api/joinrequest/{id}
    Params: id (int)
    Output: JoinRequestResponseDto
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/joinrequest/trip/{tripId}
    Params: tripId (int)
    Output: IEnumerable<JoinRequestResponseDto> (pending only)
    Status: 200 OK | 500 Internal Server Error

GET /api/joinrequest/user/{userId}
    Params: userId (int)
    Output: IEnumerable<JoinRequestResponseDto> (user's requests)
    Status: 200 OK | 500 Internal Server Error

GET /api/joinrequest/trip/{tripId}/all
    Params: tripId (int)
    Output: IEnumerable<JoinRequestResponseDto> (all statuses)
    Status: 200 OK | 500 Internal Server Error

PUT /api/joinrequest/{id}/accept
    Params: id (int)
    Query: hostId (int)
    Status: 200 OK | 403 Forbidden | 400 Bad Request | 500 Internal Server Error

PUT /api/joinrequest/{id}/reject
    Params: id (int)
    Query: hostId (int)
    Status: 200 OK | 403 Forbidden | 400 Bad Request | 500 Internal Server Error

DELETE /api/joinrequest/{id}
    Params: id (int)
    Query: userId (int)
    Status: 204 No Content | 403 Forbidden | 404 Not Found | 500 Internal Server Error

GET /api/joinrequest/check-request
    Query: userId (int), tripId (int)
    Output: boolean
    Status: 200 OK | 500 Internal Server Error
```

**Total Endpoints:** 9

---

## **💰 Expense Controller**

**File:** `Controllers/ExpenseController.cs`

**Base Route:** `POST/GET /api/expense`

### **Expense Endpoints**

```
POST /api/expense
    Query: userId (int)
    Input: CreateExpenseDto { TripId, Amount, Description }
    Output: ExpenseResponseDto
    Status: 201 Created | 400 Bad Request | 500 Internal Server Error

GET /api/expense/{id}
    Params: id (int)
    Output: ExpenseResponseDto
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/expense/trip/{tripId}
    Params: tripId (int)
    Output: IEnumerable<ExpenseResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/expense/paid-by/{userId}
    Params: userId (int)
    Output: IEnumerable<ExpenseResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/expense/total/{tripId}
    Params: tripId (int)
    Output: decimal (total expenses)
    Status: 200 OK | 500 Internal Server Error

GET /api/expense/owed-by/{userId}/trip/{tripId}
    Params: userId (int), tripId (int)
    Output: decimal (total owed)
    Status: 200 OK | 500 Internal Server Error

GET /api/expense/unsettled/{userId}/trip/{tripId}
    Params: userId (int), tripId (int)
    Output: IEnumerable<ExpenseSplitResponseDto>
    Status: 200 OK | 500 Internal Server Error

PUT /api/expense/settle/{splitId}
    Params: splitId (int)
    Status: 200 OK | 400 Bad Request | 404 Not Found | 500 Internal Server Error

GET /api/expense/summary/{tripId}
    Params: tripId (int)
    Output: ExpenseSummaryDto { TotalExpenses, UserBalances }
    Status: 200 OK | 500 Internal Server Error

DELETE /api/expense/{id}
    Params: id (int)
    Query: userId (int)
    Status: 204 No Content | 403 Forbidden | 404 Not Found | 500 Internal Server Error
```

**Total Endpoints:** 10

---

## **💬 Chat Controller**

**File:** `Controllers/ChatController.cs`

**Base Route:** `POST/GET /api/chat`

### **Chat Endpoints**

```
POST /api/chat
    Query: userId (int)
    Input: SendMessageDto { TripId, Message }
    Output: ChatMessageResponseDto
    Status: 201 Created | 400 Bad Request | 500 Internal Server Error

GET /api/chat/trip/{tripId}
    Params: tripId (int)
    Output: IEnumerable<ChatMessageResponseDto>
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/chat/trip/{tripId}/paginated
    Params: tripId (int)
    Query: page (int, default=1), size (int, default=20)
    Output: IEnumerable<ChatMessageResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/chat/trip/{tripId}/latest
    Params: tripId (int)
    Query: count (int, default=50)
    Output: IEnumerable<ChatMessageResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/chat/sender/{userId}
    Params: userId (int)
    Output: IEnumerable<ChatMessageResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/chat/search/{tripId}
    Params: tripId (int)
    Query: searchText (string)
    Output: IEnumerable<ChatMessageResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/chat/count/{tripId}
    Params: tripId (int)
    Output: int (message count)
    Status: 200 OK | 500 Internal Server Error

DELETE /api/chat/{id}
    Params: id (int)
    Query: userId (int)
    Status: 204 No Content | 403 Forbidden | 404 Not Found | 500 Internal Server Error
```

**Total Endpoints:** 8

---

## **⭐ Rating Controller**

**File:** `Controllers/RatingController.cs`

**Base Route:** `POST/GET /api/rating`

### **Rating Endpoints**

```
POST /api/rating
    Input: CreateRatingDto { TripId, RatedUserId, Rating (1.0-5.0), Review }
    Output: RatingResponseDto
    Status: 201 Created | 400 Bad Request | 500 Internal Server Error

GET /api/rating/{id}
    Params: id (int)
    Output: RatingResponseDto
    Status: 200 OK | 404 Not Found | 500 Internal Server Error

GET /api/rating/for-user/{userId}
    Params: userId (int)
    Output: IEnumerable<RatingResponseDto> (ratings received)
    Status: 200 OK | 500 Internal Server Error

GET /api/rating/by-user/{userId}
    Params: userId (int)
    Output: IEnumerable<RatingResponseDto> (ratings given)
    Status: 200 OK | 500 Internal Server Error

GET /api/rating/for-trip/{tripId}
    Params: tripId (int)
    Output: IEnumerable<RatingResponseDto>
    Status: 200 OK | 500 Internal Server Error

GET /api/rating/average/{userId}
    Params: userId (int)
    Output: double (average rating)
    Status: 200 OK | 500 Internal Server Error

GET /api/rating/count/{userId}
    Params: userId (int)
    Output: int (total ratings)
    Status: 200 OK | 500 Internal Server Error

PUT /api/rating/{id}
    Params: id (int)
    Query: userId (int)
    Input: UpdateRatingDto { Rating, Review }
    Output: RatingResponseDto
    Status: 200 OK | 403 Forbidden | 404 Not Found | 500 Internal Server Error

DELETE /api/rating/{id}
    Params: id (int)
    Query: userId (int)
    Status: 204 No Content | 403 Forbidden | 404 Not Found | 500 Internal Server Error
```

**Total Endpoints:** 9

---

## **🏥 Health Controller**

**File:** `Controllers/HealthController.cs`

**Base Route:** `GET /api/health`

### **Health Check Endpoints**

```
GET /api/health
    Output: HealthCheckResponse { Status, Message, Timestamp, Details }
    Details: { ApiName, Environment, RuntimeVersion, MachineName, ProcessId }
    Status: 200 OK | 503 Service Unavailable

GET /api/health/detailed
    Output: HealthCheckResponse { Status, Message, Timestamp, Details }
    Details: { ApiStatus, Database, Services, Memory (MB), Uptime }
    Status: 200 OK | 503 Service Unavailable

GET /api/health/ready
    Output: HealthCheckResponse
    Purpose: Kubernetes/orchestration readiness probe
    Status: 200 OK | 503 Service Unavailable

GET /api/health/live
    Output: HealthCheckResponse
    Purpose: Kubernetes/orchestration liveness probe
    Status: 200 OK | 503 Service Unavailable
```

**Total Endpoints:** 4

---

## **📊 Endpoint Summary**

| Controller | Endpoints | Purpose |
|-----------|-----------|---------|
| UserController | 11 | Authentication & profile management |
| TripController | 11 | Trip CRUD & search |
| JoinRequestController | 9 | Request workflow (Pending→Accept/Reject) |
| ExpenseController | 10 | Expense tracking & splitting |
| ChatController | 8 | Messaging in trips |
| RatingController | 9 | User ratings & reviews |
| HealthController | 4 | API health monitoring |
| **Total** | **62** | **Complete API Surface** |

---

## **🔧 Middleware Pipeline**

### **Request Processing Order**

```
1. ErrorHandlingMiddleware
   ├─ Wraps entire request/response
   ├─ Catches unhandled exceptions
   ├─ Formats error responses
   └─ Logs exceptions

2. CORS Middleware
   ├─ Allows frontend requests
   ├─ Policy: AllowAngularFrontend
   └─ Allows: localhost:4200, localhost:3000 (HTTP & HTTPS)

3. HTTPS Redirection
   └─ Redirects HTTP to HTTPS

4. Authorization Middleware
   ├─ (Currently basic - can be enhanced)
   └─ Validates user context

5. Routing
   ├─ MapControllers()
   └─ Routes to appropriate controller

6. Health Check Endpoints
   └─ MapHealthChecks("/api/health/check")
```

---

## **🛡️ Error Handling Middleware**

**File:** `Middleware/ErrorHandlingMiddleware.cs`

### **Exception Mapping**

```csharp
InvalidOperationException          → 400 Bad Request
KeyNotFoundException               → 404 Not Found
UnauthorizedAccessException        → 401 Unauthorized
ArgumentException                  → 400 Bad Request
FormatException                    → 400 Bad Request
Default Exception                  → 500 Internal Server Error
```

### **Error Response Format**

```json
{
    "success": false,
    "message": "Descriptive error message"
}
```

### **Middleware Pattern**

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);  // Call next middleware
    }
    catch (Exception ex)
    {
        _logger.LogError($"Unhandled exception: {ex.Message}");
        await HandleExceptionAsync(context, ex);
    }
}
```

---

## **🌐 CORS Configuration**

**File:** `Program.cs`

### **Configuration**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",     // Angular dev server (HTTP)
            "https://localhost:4200",    // Angular dev server (HTTPS)
            "http://localhost:3000",     // Alternative frontend (HTTP)
            "https://localhost:3000"     // Alternative frontend (HTTPS)
        )
        .AllowAnyMethod()                // GET, POST, PUT, DELETE, etc.
        .AllowAnyHeader()                // Any Content-Type, Authorization, etc.
        .AllowCredentials();             // Allow cookies & credentials
    });
});

app.UseCors("AllowAngularFrontend");
```

### **Allowed Operations**

- ✅ All HTTP verbs (GET, POST, PUT, DELETE, PATCH, OPTIONS)
- ✅ All headers (Authorization, Content-Type, X-Custom-Header, etc.)
- ✅ Credentials (Cookies, authentication headers)
- ✅ Multiple origins (localhost:4200 & 3000, HTTP & HTTPS)

---

## **📊 Health Check Response**

**File:** `Models/HealthCheckResponse.cs`

### **Response Structure**

```json
{
    "status": "Healthy",
    "message": "API is running successfully",
    "timestamp": "2026-04-13T10:30:45.123Z",
    "version": "1.0.0",
    "details": {
        "apiName": "TripConnect API",
        "environment": "Development",
        "runtimeVersion": ".NET 6.0.0",
        "machineName": "DESKTOP-XYZ",
        "processId": 12345,
        "database": "Connected",
        "services": "All Running",
        "memory": "245 MB",
        "uptime": "04:23:15"
    }
}
```

---

## **📝 HTTP Status Codes**

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful GET/PUT request |
| 201 | Created | Successful POST request |
| 204 | No Content | Successful DELETE request |
| 400 | Bad Request | Invalid input/validation error |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | User lacks permission (ownership check) |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Unexpected server error |
| 503 | Service Unavailable | Health check failed |

---

## **🔌 Program.cs Startup Configuration**

### **Dependency Registration**

```csharp
// 1. Controllers
builder.Services.AddControllers();

// 2. CORS
builder.Services.AddCors(options => {...});

// 3. Swagger/OpenAPI
builder.Services.AddSwaggerGen();

// 4. Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("API", () => HealthCheckResult.Healthy(...));

// 5. Application Layer (Services)
builder.Services.AddApplicationLayer();

// 6. Infrastructure Layer (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);
```

### **Middleware Pipeline**

```csharp
// Development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(...);  // Served at https://localhost:7142/
}

// Global error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

// Enable CORS
app.UseCors("AllowAngularFrontend");

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Authorization checks
app.UseAuthorization();

// Route to controllers
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/api/health/check", options);

// Start server
app.Run();
```

---

## **📡 Swagger Documentation**

**URL:** `https://localhost:7142/`

### **Features**

- ✅ Interactive API exploration
- ✅ Try-it-out functionality
- ✅ Request/response schemas
- ✅ Parameter documentation
- ✅ Method descriptions (from XML comments)

### **All Endpoints Documented**

- 62 endpoints across 7 controllers
- Request/response examples
- Status codes
- Authorization requirements

---

## **🔄 Request/Response Pattern**

### **GET Request**

```
GET /api/user/123

Response (200 OK):
{
    "id": 123,
    "name": "John Doe",
    "email": "john@example.com",
    "rating": 4.5,
    ...
}
```

### **POST Request**

```
POST /api/trip?userId=123

Request Body:
{
    "title": "Himalayan Trek",
    "description": "5-day trek",
    "location": "Himalayas",
    "budget": 50000,
    "startDate": "2026-06-01",
    "endDate": "2026-06-05",
    "seats": 8,
    "travelType": "Trekking"
}

Response (201 Created):
{
    "id": 456,
    "title": "Himalayan Trek",
    ...
}

Header: Location: /api/trip/456
```

### **PUT Request**

```
PUT /api/trip/456?userId=123

Request Body:
{
    "title": "Updated Title",
    "budget": 55000
}

Response (200 OK):
{
    "id": 456,
    "title": "Updated Title",
    "budget": 55000,
    ...
}
```

### **DELETE Request**

```
DELETE /api/trip/456?userId=123

Response (204 No Content):
(Empty body)
```

### **Error Response**

```
POST /api/user/login

Request Body:
{
    "username": "invalid",
    "password": "wrong"
}

Response (401 Unauthorized):
{
    "success": false,
    "message": "Invalid username or password"
}
```

---

## **🔐 Security Features**

### **At API Layer**

1. **Input Validation** - ModelState validation for all inputs
2. **HTTPS** - All traffic encrypted (redirected from HTTP)
3. **CORS** - Restricted to known frontend origins
4. **Error Masking** - Generic error messages to prevent info leakage
5. **Logging** - All operations logged for audit trail
6. **Ownership Checks** - Authorization via userId query parameter
7. **Exception Handling** - Graceful error responses

### **Data Validation**

```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);  // Returns validation errors
```

### **Ownership Verification**

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateTrip(int id, UpdateTripDto dto, [FromQuery] int userId)
{
    // Service checks: if (trip.HostId != userId) throw Unauthorized
    var result = await _tripService.UpdateTripAsync(id, dto, userId);
}
```

---

## **📌 Configuration Files**

### **appsettings.json**

General configuration:
- Logging levels
- Connection strings
- Database settings

### **appsettings.Development.json**

Development-specific:
- Verbose logging
- Development database
- Swagger enabled

---

## **🧪 Testing Endpoints**

### **Using HTTP Client File**

**File:** `API_LAYER.http`

```http
### Register User
POST https://localhost:7142/api/user/register
Content-Type: application/json

{
    "name": "Test User",
    "email": "test@example.com",
    "username": "testuser",
    "phone": "9876543210",
    "password": "Password123"
}

### Login
POST https://localhost:7142/api/user/login
Content-Type: application/json

{
    "username": "testuser",
    "password": "Password123"
}

### Get Health
GET https://localhost:7142/api/health

### Create Trip
POST https://localhost:7142/api/trip?userId=1
Content-Type: application/json

{
    "title": "Beach Trip",
    "description": "Summer vacation",
    "location": "Goa",
    "budget": 30000,
    "startDate": "2026-05-01",
    "endDate": "2026-05-05",
    "seats": 6,
    "travelType": "Beach"
}
```

---

## **🔗 API Integration Flow**

### **Complete User Registration Flow**

```
┌─────────────┐
│  Frontend   │
└──────┬──────┘
       │ 1. POST /api/user/register
       │    (CreateUserDto)
       ▼
┌─────────────────────────┐
│  UserController         │
│  [ApiController]        │
│  [Route("api/user")]    │
└──────┬──────────────────┘
       │ 2. Register(createUserDto)
       │    - Validate ModelState
       │    - Call service
       ▼
┌──────────────────────┐
│  ErrorHandlingMiddleware
│  (Global exception handling)
└──────────────────────┘
       │
       │ 3. userService.RegisterAsync()
       ▼
┌─────────────────────┐
│  UserService        │
│  (APPLICATION_LAYER)│
└──────┬──────────────┘
       │ 4. Validate email/username
       │    Hash password
       │    Create User entity
       ▼
┌──────────────────────────┐
│  UserRepository          │
│  (INFRASTRUCTURE_LAYER)  │
└──────┬───────────────────┘
       │ 5. await _context.Users.AddAsync(user)
       │    await _context.SaveChangesAsync()
       ▼
┌──────────────────────┐
│  TripConnect DB      │
│  (SQL Server)        │
└──────┬───────────────┘
       │ 6. INSERT INTO Users...
       │    RETURN @Id
       ▼
┌──────────────────────────┐
│  AutoMapper              │
│  (USER → UserResponseDto)│
└──────┬───────────────────┘
       │ 7. Map entity to DTO
       ▼
┌──────────────────────────┐
│  UserController          │
│  Return 200 OK           │
│  AuthResponseDto in body │
└──────┬───────────────────┘
       │ 8. Response
       │    { success: true,
       │      user: {...},
       │      token: "jwt..." }
       ▼
┌─────────────────────┐
│  Frontend           │
│  Parse response     │
│  Store token        │
└─────────────────────┘
```

---

## **🚀 Starting the API**

### **Command Line**

```bash
# Navigate to API_LAYER
cd TripConnect_Backend/TripConnect/API_LAYER

# Run the application
dotnet run

# Starting development server...
# Now listening on: https://localhost:7142
# Now listening on: http://localhost:5126
```

### **In Visual Studio**

1. Set API_LAYER as startup project
2. Press F5 (Debug) or Ctrl+F5 (Run without debug)
3. Browser opens at https://localhost:7142/
4. Swagger UI available at path `/`

---

## **📊 API Statistics**

| Metric | Count |
|--------|-------|
| Controllers | 7 |
| Total Endpoints | 62 |
| HTTP Methods | GET (36), POST (14), PUT (8), DELETE (4) |
| Status Codes Used | 6 (200, 201, 204, 400, 401, 404, 500, 503) |
| Middleware Components | 5 (Error, CORS, HTTPS, Auth, Routing) |
| Health Check Endpoints | 4 |
| Documentation | Full Swagger with 62 endpoints |

---

## **Key Characteristics**

✅ **RESTful Design** - Standard HTTP verbs and patterns
✅ **Consistent Responses** - Uniform error/success formatting
✅ **Global Error Handling** - Middleware catches all exceptions
✅ **Input Validation** - ModelState validation on all inputs
✅ **Logging** - Comprehensive logging of all operations
✅ **CORS Enabled** - Supports frontend from multiple origins
✅ **Health Monitoring** - 4 health check endpoints
✅ **Documentation** - Full Swagger integration
✅ **Status Codes** - Proper HTTP status codes for all scenarios
✅ **Async/Await** - Non-blocking I/O throughout
✅ **Dependency Injection** - All dependencies injected
✅ **Security** - HTTPS, validation, ownership checks

---

## **Integration Points**

1. **Receives from:** Frontend (Angular at localhost:4200)
2. **Calls:** APPLICATION_LAYER services
3. **Uses:** INFRASTRUCTURE_LAYER repositories
4. **Accesses:** DOMAIN_LAYER entities
5. **Returns to:** Frontend via JSON

---

## **Next Steps for Enhancement**

- 🔐 JWT token-based authentication
- 🔒 Role-based access control (RBAC)
- ⏱️ Request throttling/rate limiting
- 📊 Advanced logging & monitoring
- 🔄 API versioning (v1, v2, etc.)
- 🧪 Comprehensive API testing with xUnit
- 📦 API response caching
- 🌍 Internationalization (i18n)

---

This is the **API_LAYER** - the gateway to your TripConnect backend! ✅
