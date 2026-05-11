# API LAYER DOCUMENTATION

## 1. Overview

The **API Layer** is the presentation tier that handles HTTP requests/responses, authentication, authorization, error handling, and request routing. It acts as the gateway between frontend clients and the business logic in the Application layer.

**Purpose:**
- Accept HTTP requests from frontend clients (Angular)
- Authenticate and authorize requests via JWT Bearer tokens
- Validate request payloads and extract user context from claims
- Delegate to Application Layer services for business logic
- Format and return responses with appropriate HTTP status codes
- Handle exceptions globally with consistent error responses
- Provide health check endpoints for monitoring

**Key Principles:**
- Stateless design (all user context in JWT token)
- Separation of concerns (controllers only route/validate, services execute logic)
- Comprehensive error handling via middleware
- CORS enabled for Angular frontend
- Swagger/OpenAPI documentation for API exploration
- Logging for debugging and audit trails

---

## 2. Project Structure

```
API_LAYER/
├── API_LAYER.csproj                   # NuGet dependencies
├── Program.cs                         # Startup configuration, middleware setup
├── appsettings.json                   # Configuration (JWT, SMTP, Twilio, Redis, ImageKit)
├── appsettings.Development.json       # Development overrides
├── API_LAYER.http                     # HTTP test requests
├── Controllers/                       # 9 route controllers (58 total endpoints)
│   ├── UserController.cs              # Auth (register, login, logout, refresh), profile
│   ├── TripController.cs              # Trip CRUD, search, filters, member management
│   ├── ExpenseController.cs           # Expense creation, tracking, settlement, calculations
│   ├── JoinRequestController.cs       # Trip join request workflow
│   ├── ChatController.cs              # Real-time chat messages
│   ├── RatingController.cs            # User ratings and reviews
│   ├── ImageController.cs             # Image upload/delete for profiles, trips, expenses
│   ├── HealthController.cs            # API health status (Kubernetes probes)
│   └── WeatherForecastController.cs   # Sample controller (unused)
├── Middleware/
│   └── ErrorHandlingMiddleware.cs     # Global exception handling, error response formatting
└── Models/
    └── HealthCheckResponse.cs         # Health check response model
```

---

## 3. Dependencies

**NuGet Packages:**
- **Microsoft.AspNetCore.OpenApi 10.0.5** — OpenAPI/Swagger integration
- **Swashbuckle.AspNetCore.Swagger 10.1.7** — Swagger UI and generation
- **Swashbuckle.AspNetCore.SwaggerGen 10.1.7** — OpenAPI schema generation
- **Swashbuckle.AspNetCore.SwaggerUI 10.1.7** — Interactive Swagger documentation
- **System.IdentityModel.Tokens.Jwt 8.0.1** — JWT token creation/validation
- **Microsoft.IdentityModel.Tokens 8.14.0** — Token validation parameters and security
- **Microsoft.AspNetCore.Authentication.JwtBearer 10.0.5** — JWT authentication scheme
- **Microsoft.EntityFrameworkCore.Design 10.0.5** — EF Core tooling (migrations)

**Project References:**
- **APPLICATION_LAYER** — Service interfaces and business logic
- **INFRASTRUCTURE_LAYER** — Data access, caching, external service integrations

---

## 4. Configuration & Startup (Program.cs)

**Middleware Pipeline:**
```
ErrorHandlingMiddleware
    ↓
CORS Middleware
    ↓
HTTPS Redirection
    ↓
Authentication Middleware
    ↓
Authorization Middleware
    ↓
Controllers/Endpoints
```

**Configuration Details:**

**4.1 JWT Authentication**
- Scheme: `JwtBearerDefaults.AuthenticationScheme` (HTTP Bearer)
- Algorithm: HS256 (HMAC-SHA256)
- Configuration:
  - `ValidateIssuerSigningKey`: true (verify secret key)
  - `IssuerSigningKey`: SymmetricSecurityKey(SecretKey)
  - `ValidateIssuer`: true (issuer must match "TripConnectAPI")
  - `ValidateAudience`: true (audience must match "TripConnectApp")
  - `ValidateLifetime`: true (token must not be expired)
  - `ClockSkew`: TimeSpan.Zero (no tolerance for clock differences)
- Custom Events:
  - `OnChallenge`: Returns 401 with message "Token has expired" (if SecurityTokenExpiredException) or "Authentication failed"
  - `OnForbidden`: Returns 403 with message "Access denied. Insufficient permissions"

**4.2 CORS Policy**
- Policy Name: "AllowAngularFrontend"
- Allowed Origins:
  - http://localhost:4200 (Angular dev server)
  - https://localhost:4200
  - http://localhost:3000 (alternative frontend)
  - https://localhost:3000
- AllowAnyMethod: true (GET, POST, PUT, DELETE, etc.)
- AllowAnyHeader: true (any request headers)
- AllowCredentials: true (cookies and Authorization headers)

**4.3 Service Registration**
- Controllers: `.AddControllers()`
- JWT Authentication: `.AddAuthentication().AddJwtBearer()`
- Authorization: `.AddAuthorization()`
- CORS: `.AddCors()`
- Swagger: `.AddSwaggerGen()`
- Health Checks: `.AddHealthChecks().AddCheck("API", ...)`
- Application Layer: `.AddApplicationLayer(configuration)`
- Infrastructure Layer: `.AddInfrastructure(configuration)`

**4.4 Swagger UI Configuration**
- Route: `/swagger/v1/swagger.json` (OpenAPI schema)
- UI: Root path `/` in development (http://localhost:5126/)
- Document Title: "TripConnect API v1"

**4.5 Health Check Endpoint**
- Route: `/api/health/check`
- Response Format: JSON with status and entries
- Runs built-in "API" check and custom component checks

---

## 5. Controllers & Endpoints

### 5.1 UserController (`/api/user`)

**Authentication Endpoints (Public):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/register` | Register new user | ❌ |
| POST | `/login` | Login and get JWT tokens | ❌ |
| POST | `/refresh-token` | Refresh expired access token | ❌ |

**Profile Endpoints (Protected):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/logout` | Logout and blacklist refresh token | ✅ |
| GET | `/{id}` | Get user by ID (cached) | ✅ |
| GET | `/email/{email}` | Get user by email (cached) | ✅ |
| PUT | `/{id}` | Update own profile (name, phone) | ✅ |
| GET | `/{id}/rating` | Get user rating score | ✅ |

**Verification Endpoints (Protected):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/request-email-verification` | Send email verification link | ✅ |
| POST | `/verify-email` | Confirm email verification | ✅ |
| POST | `/request-phone-verification` | Send OTP to phone via SMS | ✅ |
| POST | `/verify-phone-otp` | Confirm phone OTP | ✅ |
| GET | `/verify-phone/{id}` | Mark phone as verified | ✅ |
| GET | `/verify-id/{id}` | Mark ID as verified (admin) | ✅ |

**Utility Endpoints (Public):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/username-exists/{username}` | Check username availability | ❌ |
| GET | `/email-exists/{email}` | Check email availability | ❌ |
| GET | `/all` | Get all users (admin) | ✅ |

**Typical Request/Response:**

Register:
```http
POST /api/user/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "username": "johndoe",
  "phone": "+1234567890",
  "password": "SecurePass123!"
}

Response 200 OK:
{
  "success": true,
  "message": "Registration successful",
  "token": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "M7xK9pL...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "issuedAt": "2026-05-08T10:30:00Z",
    "expiresAt": "2026-05-08T11:30:00Z"
  },
  "user": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "username": "johndoe",
    "phone": "+1234567890",
    "rating": 0.0,
    "phoneVerified": false,
    "idVerified": false,
    "emailVerified": false,
    "createdAt": "2026-05-08T10:30:00Z"
  }
}
```

Login:
```http
POST /api/user/login
Content-Type: application/json

{
  "username": "johndoe",
  "password": "SecurePass123!"
}

Response 200 OK: [same structure as Register]
```

Refresh Token:
```http
POST /api/user/refresh-token
Content-Type: application/json

{
  "refreshToken": "M7xK9pL..."
}

Response 200 OK:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "newRefreshToken...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "issuedAt": "2026-05-08T11:30:00Z",
  "expiresAt": "2026-05-08T12:30:00Z"
}
```

---

### 5.2 TripController (`/api/trip`)

**Trip Management (Public/Protected):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/` | Create new trip | ✅ |
| GET | `/` | Get all trips with pagination | ❌ |
| GET | `/{id}` | Get trip by ID | ❌ |
| PUT | `/{id}` | Update trip details | ✅ |
| DELETE | `/{id}` | Cancel trip | ✅ |

**Trip Queries (Public):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/status/{status}` | Get trips by status (Planned/Ongoing/Completed/Cancelled) | ❌ |
| GET | `/upcoming` | Get upcoming trips (StartDate > now, cached 30 min) | ❌ |
| GET | `/search/location/{location}` | Search trips by location (cached 2 hrs) | ❌ |
| GET | `/search?location=...&startDate=...&maxBudget=...&travelType=...` | Multi-criteria search | ❌ |
| GET | `/user/{userId}` | Get trips created by user | ❌ |

**Trip Members (Protected):**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/{id}/members` | Get trip members and their roles | ✅ |

**Example Requests:**

Create Trip:
```http
POST /api/trip
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "title": "Summer Adventure",
  "description": "Beach and hiking trip",
  "location": "Goa, India",
  "budget": 50000,
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-06-08T00:00:00Z",
  "seats": 4,
  "travelType": "Adventure",
  "imgUrl": "https://...",
  "tripDays": [
    {
      "dayNumber": 1,
      "date": "2026-06-01T00:00:00Z",
      "description": "Arrival and beach",
      "activity": "Beach"
    }
  ]
}

Response 201 Created:
{
  "id": 1,
  "title": "Summer Adventure",
  "location": "Goa, India",
  "budget": 50000,
  "status": "Planned",
  "hostId": 1,
  "hostName": "John Doe",
  "createdAt": "2026-05-08T10:30:00Z",
  ...
}
```

Get All Trips (Paginated):
```http
GET /api/trip?pageNumber=1&pageSize=10
Response 200 OK: [Array of TripResponseDto]
```

Search Trips:
```http
GET /api/trip/search?location=Goa&startDate=2026-06-01&maxBudget=50000&travelType=Adventure
Response 200 OK: [Filtered trips, cached 2 hrs]
```

---

### 5.3 ExpenseController (`/api/expense`)

**Expense Management:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/` | Create expense with splits | ✅ |
| GET | `/{id}` | Get expense details | ❌ |
| DELETE | `/{id}` | Delete expense | ✅ |

**Expense Queries:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/trip/{tripId}` | Get all expenses for trip | ❌ |
| GET | `/user/{userId}/paid` | Get expenses paid by user | ❌ |
| GET | `/user/{userId}/owed?tripId=...` | Get total amount owed by user | ❌ |
| GET | `/user/{userId}/unsettled?tripId=...` | Get unsettled expenses for user | ❌ |
| GET | `/trip/{tripId}/total` | Get total expenses for trip | ❌ |

**Settlement:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/split/{splitId}/settle` | Mark expense split as settled | ✅ |

**Example Request:**

Create Expense:
```http
POST /api/expense?userId=1
Content-Type: application/json

{
  "tripId": 1,
  "amount": 5000,
  "description": "Hotel booking",
  "splits": [
    {
      "userId": 2,
      "amountOwed": 2500
    },
    {
      "userId": 3,
      "amountOwed": 2500
    }
  ]
}

Response 201 Created:
{
  "id": 1,
  "tripId": 1,
  "paidBy": 1,
  "paidByName": "John Doe",
  "amount": 5000,
  "description": "Hotel booking",
  "createdAt": "2026-05-08T10:30:00Z",
  "splits": [
    {
      "id": 1,
      "userId": 2,
      "userName": "Jane Smith",
      "amountOwed": 2500,
      "isSettled": false
    },
    {
      "id": 2,
      "userId": 3,
      "userName": "Bob Johnson",
      "amountOwed": 2500,
      "isSettled": false
    }
  ]
}
```

---

### 5.4 JoinRequestController (`/api/joinrequest`)

**Join Request Workflow:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/` | Send join request for trip | ✅ |
| GET | `/{id}` | Get request details | ❌ |
| POST | `/{id}/accept?tripHostId=...` | Accept request (host only) | ✅ |
| POST | `/{id}/reject?tripHostId=...` | Reject request (host only) | ✅ |
| POST | `/{id}/cancel?userId=...` | Cancel own request | ✅ |

**Queries:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/trip/{tripId}/pending` | Get pending requests for trip | ❌ |
| GET | `/trip/{tripId}` | Get all requests for trip | ❌ |
| GET | `/user/{userId}` | Get requests sent by user | ❌ |
| GET | `/check?userId=...&tripId=...` | Check if user already requested | ❌ |

**Example Request:**

Send Join Request:
```http
POST /api/joinrequest?userId=2
Content-Type: application/json

{
  "tripId": 1
}

Response 201 Created:
{
  "id": 1,
  "tripId": 1,
  "userId": 2,
  "userName": "Jane Smith",
  "status": "Pending",
  "createdAt": "2026-05-08T10:30:00Z",
  "updatedAt": null
}
```

Accept Request:
```http
POST /api/joinrequest/1/accept?tripHostId=1
Response 200 OK:
{
  "success": true,
  "message": "Join request accepted successfully"
}
```

---

### 5.5 ChatController (`/api/chat`)

**Chat Management:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/` | Send message to trip chat | ✅ |
| DELETE | `/{id}` | Delete message | ✅ |

**Message Queries:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/trip/{tripId}` | Get all messages for trip | ❌ |
| GET | `/trip/{tripId}/paginated?pageNumber=1&pageSize=20` | Get paginated messages | ❌ |
| GET | `/trip/{tripId}/latest?count=10` | Get latest N messages | ❌ |
| GET | `/user/{userId}` | Get messages sent by user | ❌ |
| GET | `/trip/{tripId}/search?searchText=...` | Search messages by content | ❌ |
| GET | `/trip/{tripId}/count` | Get message count for trip | ❌ |

**Example Request:**

Send Message:
```http
POST /api/chat?userId=1
Content-Type: application/json

{
  "tripId": 1,
  "message": "Looking forward to the trip!"
}

Response 201 Created:
{
  "id": 1,
  "tripId": 1,
  "senderId": 1,
  "senderName": "John Doe",
  "content": "Looking forward to the trip!",
  "createdAt": "2026-05-08T10:30:00Z"
}
```

Get Messages:
```http
GET /api/chat/trip/1
Response 200 OK: [Array of ChatMessageResponseDto]
```

---

### 5.6 RatingController (`/api/rating`)

**Rating Management:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/` | Create rating for user | ✅ |
| GET | `/{id}` | Get rating details | ❌ |
| PUT | `/{id}` | Update rating | ✅ |
| DELETE | `/{id}` | Delete rating | ✅ |

**Rating Queries:**

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/user/{userId}` | Get all ratings for user | ❌ |
| GET | `/user/{userId}/average` | Get average rating for user | ❌ |
| GET | `/user/{userId}/given` | Get ratings given by user | ❌ |
| GET | `/trip/{tripId}` | Get ratings for trip | ❌ |
| GET | `/check?userId=...&tripId=...&ratedUserId=...` | Check if user already rated | ❌ |
| GET | `/user/{userId}/count` | Get ratings count for user | ❌ |

**Example Request:**

Create Rating:
```http
POST /api/rating?userId=1
Content-Type: application/json

{
  "tripId": 1,
  "ratedUserId": 2,
  "rating": 4.5,
  "review": "Great travel companion!"
}

Response 201 Created:
{
  "id": 1,
  "tripId": 1,
  "ratedBy": 1,
  "ratedByName": "John Doe",
  "ratedUserId": 2,
  "ratedUserName": "Jane Smith",
  "rating": 4.5,
  "review": "Great travel companion!",
  "createdAt": "2026-05-08T10:30:00Z"
}
```

---

### 5.7 ImageController (`/api/image`)

**Protected Endpoints (Require JWT):**

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/upload-profile` | Upload profile picture |
| POST | `/upload-trip/{tripId}` | Upload trip image |
| POST | `/upload-expense/{expenseId}` | Upload expense receipt image |
| DELETE | `/delete/{fileId}` | Delete image by file ID |
| GET | `/get-file-id` | Extract file ID from URL |
| GET | `/get-url/{fileId}` | Get image URL from file ID |

**File Validation:**
- Max size: 5 MB
- Allowed formats: jpg, jpeg, png, webp
- Min dimensions: 100×100 px
- Max dimensions: 4000×4000 px

**Example Request:**

Upload Profile Image:
```http
POST /api/image/upload-profile
Authorization: Bearer eyJhbGc...
Content-Type: multipart/form-data

File: profile.jpg (binary)

Response 200 OK:
{
  "fileId": "ik_file_id_123",
  "imageUrl": "REDACTED/tr:w-400,h-300/profile.jpg",
  "publicUrl": "REDACTED/profile.jpg",
  "uploadedAt": "2026-05-08T10:30:00Z",
  "success": true,
  "message": "Image uploaded successfully",
  "fileSize": 204800,
  "width": 1920,
  "height": 1080,
  "fileName": "profile.jpg"
}
```

---

### 5.8 HealthController (`/api/health`)

**Health Check Endpoints (Public):**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Basic health status |
| GET | `/detailed` | Detailed component health |
| GET | `/ready` | Kubernetes readiness probe |
| GET | `/live` | Kubernetes liveness probe |

**Example Response:**

```http
GET /api/health
Response 200 OK:

{
  "status": "Healthy",
  "message": "API is running successfully",
  "details": {
    "apiName": "TripConnect API",
    "environment": "Development",
    "runtimeVersion": ".NET 10.0.5",
    "machineName": "DESKTOP-XYZ",
    "processId": 12345
  }
}
```

---

## 6. Authentication & Authorization

### 6.1 JWT Token Flow

**Registration/Login:**
1. Client sends credentials (username/password)
2. Server verifies credentials against password hash
3. Server generates access token (60 min) + refresh token (7 days)
4. Client stores tokens in localStorage (or sessionStorage)

**Authenticated Requests:**
1. Client includes `Authorization: Bearer <accessToken>` header
2. JwtBearerAuthenticationHandler validates:
   - Signature (matches secret key)
   - Issuer matches "TripConnectAPI"
   - Audience matches "TripConnectApp"
   - Token not expired
3. If valid, request proceeds with User claims populated

**Token Refresh:**
1. Client sends POST /refresh-token with refresh token
2. Server validates refresh token hasn't expired and isn't blacklisted
3. Server generates new access token + optionally new refresh token
4. Client updates stored access token

**Logout:**
1. Client sends POST /logout with valid access token
2. Server sets IsTokenBlacklisted=true on user
3. Server invalidates cache entries
4. Refresh token becomes invalid
5. Client removes stored tokens

### 6.2 Claims Extraction

Controllers extract user context from JWT claims:
```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (!int.TryParse(userIdClaim, out var userId))
    return Unauthorized("Invalid user claim");
```

**Available Claims:**
- `ClaimTypes.NameIdentifier` → user.Id (int)
- `ClaimTypes.Email` → user.Email
- `ClaimTypes.Name` → user.Username
- `"FullName"` → user.Name
- `"Phone"` → user.Phone

### 6.3 Authorization Attributes

```csharp
[Authorize]                    // Requires any valid JWT token
[Authorize(Roles = "Admin")]   // Requires specific role (if implemented)
[AllowAnonymous]               // Explicitly allows unauthenticated access
```

Most endpoints use `[Authorize]` to require authentication.

---

## 7. Error Handling Middleware

**File:** `ErrorHandlingMiddleware.cs`

**Purpose:** Global exception catching and standardized error response formatting

**Exception Mapping:**

| Exception Type | HTTP Status | Error Code | Message |
|---|---|---|---|
| `SecurityTokenExpiredException` | 401 | `TOKEN_EXPIRED` | "Token has expired" |
| `SecurityTokenInvalidSignatureException` | 401 | `INVALID_SIGNATURE` | "Invalid token signature" |
| `SecurityTokenValidationException` | 401 | `TOKEN_INVALID` | "Token validation failed" |
| `SecurityTokenException` | 401 | `TOKEN_ERROR` | "Security token error" |
| `InvalidOperationException` | 400 | `INVALID_OPERATION` | (exception message) |
| `KeyNotFoundException` | 404 | `NOT_FOUND` | "Resource not found" |
| `UnauthorizedAccessException` | 401 | `UNAUTHORIZED` | "Unauthorized access" |
| `ArgumentException` | 400 | `INVALID_ARGUMENT` | (exception message) |
| (Default/Unhandled) | 500 | `INTERNAL_ERROR` | "Internal server error" |

**Error Response Format:**
```json
{
  "success": false,
  "message": "Token has expired",
  "errorCode": "TOKEN_EXPIRED"
}
```

**Logging:**
- All exceptions logged at ERROR level with stack trace
- Warning level for expected errors (auth failures, not found, etc.)
- Request context preserved if response not already started

---

## 8. HTTP Status Codes

**2xx Success:**
- `200 OK` — Standard successful request
- `201 Created` — Resource created (CreateTrip, SendMessage, etc.)

**4xx Client Error:**
- `400 Bad Request` — Invalid input, business logic violation
- `401 Unauthorized` — Missing/invalid JWT token, expired token
- `403 Forbidden` — User lacks permission for resource (different user's profile)
- `404 Not Found` — Resource not found

**5xx Server Error:**
- `500 Internal Server Error` — Unhandled exception
- `503 Service Unavailable` — Health checks failing

---

## 9. Request Validation

**ModelState Validation:**
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);  // Returns field-level validation errors
```

**Custom Validation:**
- Controllers verify user owns resource (user can only update own profile)
- Controllers verify user is trip host (can accept/reject join requests)
- Services validate business rules (unique email, username, prevent duplicate ratings)

**DTO Attributes (Application Layer):**
- Data annotations like `[Required]`, `[StringLength]`, `[Range]` defined in DTOs

---

## 10. CORS Configuration

**Policy:** "AllowAngularFrontend"

**Why CORS matters:**
- Frontend at `http://localhost:4200` makes requests to API at `http://localhost:5126`
- Browser blocks cross-origin requests by default
- Server must explicitly allow frontend origin

**Headers Added by Browser:**
```
Origin: http://localhost:4200
Access-Control-Request-Method: POST
Access-Control-Request-Headers: content-type, authorization
```

**Server Response Headers:**
```
Access-Control-Allow-Origin: http://localhost:4200
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS, PATCH
Access-Control-Allow-Headers: *
Access-Control-Allow-Credentials: true
```

---

## 11. Logging Strategy

**Log Levels:**
- `Information` — Successful operations (login, trip created, health check)
- `Warning` — Expected errors (invalid credentials, not found, duplicate username)
- `Error` — Unexpected exceptions (database errors, service failures)

**Controller Logging Pattern:**
```csharp
try
{
    _logger.LogInformation($"Creating trip for user: {userId}");
    var result = await _tripService.CreateTripAsync(dto, userId);
    _logger.LogInformation($"Trip created: {result.Id}");
    return CreatedAtAction(...);
}
catch (InvalidOperationException ex)
{
    _logger.LogWarning(ex.Message);
    return BadRequest(...);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating trip");
    return StatusCode(500, ...);
}
```

**Middleware Logging:**
- All unhandled exceptions logged with full stack trace
- Useful for post-mortem debugging

---

## 12. Configuration (appsettings.json)

**Database:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TripConnect;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

**JWT:**
```json
"JwtSettings": {
  "SecretKey": "your-super-secret-key-min-32-characters-long!!!!!",
  "Issuer": "TripConnectAPI",
  "Audience": "TripConnectApp",
  "ExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

**Email (SMTP):**
```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 465,
  "SenderEmail": "trip.connect123@gmail.com",
  "SenderPassword": "REDACTED",
  "SenderName": "TripConnect"
}
```

**SMS (Twilio):**
```json
"TwilioSettings": {
  "AccountSid": "REDACTED",
  "AuthToken": "REDACTED",
  "VerifyServiceSid": "REDACTED"
}
```

**Redis Cache:**
```json
"RedisCache": {
  "ConnectionString": "redis-15777.crce286.ap-south-1-1.ec2.cloud.redislabs.com:15777,user=default,password=...",
  "DatabaseIndex": 0,
  "KeyPrefix": "tripconnect:",
  "DefaultAbsoluteExpirationSeconds": 3600,
  "DefaultSlidingExpirationSeconds": 1800,
  "ConnectTimeoutMilliseconds": 5000,
  "EnableAutoCleanup": true,
  "CleanupIntervalMinutes": 60
}
```

**Image Upload (ImageKit):**
```json
"ImageKit": {
  "PublicKey": "REDACTED",
  "PrivateKey": "REDACTED",
  "UrlEndpoint": "REDACTED"
},
"ImageUpload": {
  "MaxFileSizeInMB": 5,
  "AllowedFormats": ["jpg", "jpeg", "png", "webp"],
  "MinWidth": 100,
  "MinHeight": 100,
  "MaxWidth": 4000,
  "MaxHeight": 4000
}
```

---

## 13. API Summary Table

**Total Endpoints: 58**

| Controller | Public | Protected | Total |
|---|---|---|---|
| UserController | 6 | 9 | 15 |
| TripController | 8 | 4 | 12 |
| ExpenseController | 4 | 3 | 7 |
| JoinRequestController | 5 | 3 | 8 |
| ChatController | 5 | 1 | 6 |
| RatingController | 5 | 2 | 7 |
| ImageController | 0 | 6 | 6 |
| HealthController | 4 | 0 | 4 |
| **Total** | **37** | **28** | **65** |

---

## 14. Frontend Integration Patterns

### 14.1 HTTP Interceptor (Angular)

```typescript
// Add token to all requests
config.headers = config.headers.set(
  'Authorization',
  `Bearer ${this.tokenService.getAccessToken()}`
);

// Handle 401 responses
if (error.status === 401) {
  // Call refresh endpoint
  // Update stored token
  // Retry request
}
```

### 14.2 Typical Angular Service

```typescript
export class TripService {
  constructor(private http: HttpClient) {}

  createTrip(dto: CreateTripDto): Observable<TripResponseDto> {
    return this.http.post('/api/trip', dto);
  }

  getTrip(id: number): Observable<TripResponseDto> {
    return this.http.get(`/api/trip/${id}`);
  }

  searchTrips(location: string): Observable<TripResponseDto[]> {
    return this.http.get(`/api/trip/search/location/${location}`);
  }
}
```

### 14.3 Token Storage

```typescript
// Store after login
localStorage.setItem('access_token', response.token.accessToken);
localStorage.setItem('refresh_token', response.token.refreshToken);

// Retrieve for requests
const token = localStorage.getItem('access_token');

// Clear on logout
localStorage.removeItem('access_token');
localStorage.removeItem('refresh_token');
```

---

## 15. Common Response Patterns

**Success Response (Standard):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  ...
}
```

**Success Response (Action Confirmation):**
```json
{
  "success": true,
  "message": "Trip cancelled successfully"
}
```

**Success Response (Aggregated Data):**
```json
{
  "userId": 1,
  "tripId": 1,
  "totalOwed": 5000
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Email already registered",
  "errorCode": "INVALID_OPERATION"
}
```

---

## 16. Deployment Considerations

**Environment Variables (Production):**
- Database connection string (use Azure SQL or RDS)
- JWT secret key (min 32 chars, cryptographically random)
- CORS origins (only production frontend domains)
- Email credentials (use environment secrets, not appsettings)
- Twilio credentials (same)
- Redis connection string (use managed Redis service)
- ImageKit credentials (same)

**Security:**
- HTTPS only in production (enforce via middleware)
- CORS restricted to specific origins (not *)
- JWT secret rotated regularly
- Sensitive configuration via secrets management (not in git)
- API versioning for backward compatibility
- Rate limiting (optional, can be added)

**Scaling:**
- Stateless design allows horizontal scaling
- Each instance reads configuration from same source
- Database and Redis shared across instances
- Health checks used for load balancer routing

---

## 17. Summary

The **API Layer** provides a clean HTTP interface with:

1. **8 Controllers** (58+ endpoints) covering all business domains
2. **JWT Authentication** with secure token generation and refresh flow
3. **Comprehensive Error Handling** via middleware with standardized responses
4. **CORS Configuration** enabling Angular frontend communication
5. **Request Validation** at controller and service levels
6. **Health Monitoring** via Kubernetes-compatible probes
7. **Swagger Documentation** for API exploration
8. **Clean Architecture** with controllers delegating to Application Layer services
9. **Logging Strategy** for debugging and audit trails
10. **Production-Ready Configuration** with external service integration

All controllers follow consistent patterns for error handling, logging, and response formatting, making the API predictable and maintainable for frontend developers.

