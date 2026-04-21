# JWT Token-Based Authentication Implementation Plan
**TripConnect API Layer**

---

## **Executive Summary**

This plan outlines the implementation of JWT (JSON Web Token) based authentication for the TripConnect API, replacing the current approach of passing `userId` as query parameters. The implementation will follow .NET Core best practices with a **stateless, token-based authentication system**.

---

## **JWT Authentication Flow - Complete Explanation**

### **What is JWT (JSON Web Token)?**

A JWT is a digitally signed token that contains user information (claims) encoded in JSON. It's stateless, meaning the server doesn't need to store session data.

**Structure:** `Header.Payload.Signature`
- **Header:** Algorithm (HS256) and token type (JWT)
- **Payload:** User claims (userId, email, roles, etc.)
- **Signature:** HMAC-SHA256(base64(header).base64(payload), secret_key)

**Example:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

---

### **Access Token vs Refresh Token**

#### **Access Token**
- **Lifetime:** Short-lived (60 minutes)
- **Purpose:** Authenticate API requests
- **Storage:** Sent in Authorization header
- **Contains:** User ID, email, claims
- **Validation:** Server validates signature & expiration

**When Access Token is Used:**
```
Client Request: GET /api/trip/123
  Header: Authorization: Bearer <ACCESS_TOKEN>
  ↓
Server validates token signature & expiration
  ✓ Valid → Grant access to resource
  ✗ Expired → Return 401 Unauthorized
  ✗ Invalid → Return 401 Unauthorized
```

#### **Refresh Token**
- **Lifetime:** Long-lived (7 days)
- **Purpose:** Get a new access token when current one expires
- **Storage:** Stored in database (NOT sent in requests, only on login)
- **Contains:** Random cryptographic string
- **Validation:** Server checks DB for match and expiration

**When Refresh Token is Used:**
```
Client has expired access token
  ↓
Client sends: POST /api/user/refresh-token
  Body: { "refreshToken": "<REFRESH_TOKEN>" }
  ↓
Server validates refresh token:
  - Exists in database
  - Not blacklisted
  - Not expired
  ↓
  ✓ Valid → Generate new access token, return to client
  ✗ Invalid → Return 401, require login again
```

---

### **Complete Authentication Flow Across All Layers**

#### **1. REGISTRATION/LOGIN FLOW**

```
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                          │
├─────────────────────────────────────────────────────────────────┤
│ User clicks "Login"                                             │
│ ↓                                                               │
│ Component sends: POST /api/user/login                           │
│   { username: "prem", password: "pass123" }                     │
└──────────────────┬──────────────────────────────────────────────┘
                   │ HTTP Request
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                    API_LAYER (Controllers)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserController.Login(LoginUserDto)                              │
│   ✓ No [Authorize] needed here (unauthenticated endpoint)       │
│   ✓ Calls IUserService.LoginAsync(dto)                          │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│               APPLICATION_LAYER (Services)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserService.LoginAsync():                                       │
│   1. Get user by username                                       │
│   2. Verify password (HMAC-SHA512)                              │
│   3. Generate access token:                                     │
│      - Call ITokenService.GenerateAccessToken(user)            │
│      - Returns: JwtTokenResponseDto                             │
│   4. Generate refresh token:                                    │
│      - Call ITokenService.GenerateRefreshToken()               │
│      - Returns: Random 32-byte Base64 string                    │
│   5. Save refresh token to database:                            │
│      - user.RefreshToken = refreshToken                         │
│      - user.RefreshTokenExpiryTime = now + 7 days              │
│      - user.LastLoginAt = now                                   │
│      - Call UpdateAsync(user) + SaveChangesAsync()             │
│   6. Return: AuthResponseDto {                                  │
│        success: true,                                           │
│        token: { accessToken, refreshToken, expiresIn, ... }     │
│        user: { id, email, name, ... }                           │
│      }                                                           │
└──────────────────┬──────────────────────────────────────────────┘
                   │ TokenService.GenerateAccessToken(user):
                   │   - Creates JWT claims: userId, email, name
                   │   - Reads JwtSettings from appsettings.json
                   │   - Uses HMAC-SHA256 secret key
                   │   - Sets expiration = now + 60 minutes
                   │   - Signs and encodes token
                   │   - Returns signed JWT string
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE_LAYER (Database)                    │
├─────────────────────────────────────────────────────────────────┤
│ SQL Server Update:                                              │
│   UPDATE Users                                                  │
│   SET RefreshToken = 'D3Dw5D...',                               │
│       RefreshTokenExpiryTime = '2026-04-28 10:30:00',          │
│       LastLoginAt = '2026-04-21 10:30:00'                      │
│   WHERE Id = 3                                                  │
└──────────────────┬──────────────────────────────────────────────┘
                   │ Response goes back up the stack
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                         │
├─────────────────────────────────────────────────────────────────┤
│ AuthService receives AuthResponseDto:                           │
│   {                                                             │
│     "success": true,                                            │
│     "token": {                                                  │
│       "accessToken": "eyJhbGc...",  ← Short-lived (60 min)    │
│       "refreshToken": "D3Dw5D...",  ← Long-lived (7 days)     │
│       "expiresIn": 3600,                                        │
│     },                                                          │
│     "user": { "id": 3, "email": "prem@..." }                   │
│   }                                                             │
│                                                                 │
│ Store in localStorage:                                          │
│   localStorage.setItem('tc_token', accessToken)                │
│   localStorage.setItem('tc_refresh_token', refreshToken)       │
│   localStorage.setItem('tc_userId', '3')                       │
│                                                                 │
│ Redirect to /profile                                            │
└─────────────────────────────────────────────────────────────────┘
```

---

#### **2. AUTHENTICATED REQUEST FLOW**

```
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                         │
├─────────────────────────────────────────────────────────────────┤
│ User navigates to Profile                                       │
│ ↓                                                               │
│ ProfileComponent calls:                                         │
│   this.profileService.getUserById(userId)                      │
│     GET /api/user/{userId}                                      │
│                                                                 │
│ HTTP Interceptor intercepts request:                            │
│   const token = localStorage.getItem('tc_token')               │
│   request.setHeaders({                                          │
│     'Authorization': 'Bearer eyJhbGc...'                        │
│   })                                                            │
└──────────────────┬──────────────────────────────────────────────┘
                   │ HTTP Request with Authorization header
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                    API_LAYER (Middleware)                       │
├─────────────────────────────────────────────────────────────────┤
│ JwtBearer Middleware (configured in Program.cs):                │
│   1. Extract token from Authorization header                    │
│      Authorization: Bearer eyJhbGc...                           │
│      → Extract: eyJhbGc...                                      │
│                                                                 │
│   2. Validate token signature:                                  │
│      - Decode header & payload                                  │
│      - Recalculate signature using secret key                   │
│      - Compare with provided signature                          │
│        ✓ Match → Token is authentic                             │
│        ✗ No Match → Reject (401)                                │
│                                                                 │
│   3. Validate claims & expiration:                              │
│      - Check expiration time (exp claim)                        │
│      - Check issuer (iss claim)                                 │
│      - Check audience (aud claim)                               │
│        ✓ All valid → Continue                                   │
│        ✗ Expired → Return 401 Unauthorized                      │
│                                                                 │
│   4. Set HttpContext.User:                                      │
│      - Extract claims from token payload                        │
│      - Create ClaimsPrincipal object                            │
│      - Attach to HttpContext.User                               │
│      - Controllers can access via User.FindFirst(...)          │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                    API_LAYER (Controllers)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserController.GetUserById(id):                                 │
│   [Authorize] ← Validates User is authenticated                 │
│                                                                 │
│   // Extract userId from JWT claims:                            │
│   var userId = int.Parse(                                       │
│     User.FindFirst(ClaimTypes.NameIdentifier)?.Value           │
│   );                                                            │
│   // userId = 3 (from JWT payload)                              │
│                                                                 │
│   // Call service                                               │
│   var user = await _userService.GetUserByIdAsync(userId);      │
│   return Ok(user);                                              │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│               APPLICATION_LAYER (Services)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserService.GetUserByIdAsync(3):                                │
│   1. Call IUserRepository.GetByIdAsync(3)                       │
│   2. Map User entity to UserResponseDto                         │
│   3. Return UserResponseDto                                     │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE_LAYER (Database)                    │
├─────────────────────────────────────────────────────────────────┤
│ SQL Query:                                                      │
│   SELECT * FROM Users WHERE Id = 3                             │
│                                                                 │
│ Returns User entity from database                               │
└──────────────────┬──────────────────────────────────────────────┘
                   │ Response travels back up
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                         │
├─────────────────────────────────────────────────────────────────┤
│ ProfileComponent receives:                                      │
│   {                                                             │
│     "id": 3,                                                    │
│     "name": "Prem Kamble",                                      │
│     "email": "prem@example.com",                                │
│     ...                                                         │
│   }                                                             │
│                                                                 │
│ Displays user profile in UI                                     │
└─────────────────────────────────────────────────────────────────┘
```

---

#### **3. TOKEN REFRESH FLOW**

```
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                         │
├─────────────────────────────────────────────────────────────────┤
│ After 60 minutes, access token expires                          │
│ User makes next API request                                     │
│ Interceptor detects 401 Unauthorized                            │
│                                                                 │
│ ErrorInterceptor or AuthService:                                │
│   1. Retrieve refresh token from localStorage                   │
│      refreshToken = localStorage.getItem('tc_refresh_token')   │
│                                                                 │
│   2. Call refresh endpoint:                                     │
│      POST /api/user/refresh-token                               │
│      { "refreshToken": "D3Dw5D..." }                            │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                    API_LAYER (Controllers)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserController.RefreshToken(RefreshTokenRequestDto):            │
│   ✓ No [Authorize] needed (unauthenticated endpoint)            │
│   Calls IRefreshTokenService.RefreshAccessTokenAsync(token)    │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│               APPLICATION_LAYER (Services)                      │
├─────────────────────────────────────────────────────────────────┤
│ RefreshTokenService.RefreshAccessTokenAsync(refreshToken):      │
│   1. Check token blacklist (revoked tokens):                    │
│      if (blacklist.Contains(refreshToken))                      │
│        → Return 401 Unauthorized                                │
│                                                                 │
│   2. Find user with matching refresh token:                     │
│      user = await _users.GetByRefreshTokenAsync(refreshToken)  │
│      if (user == null)                                          │
│        → Return 401 Unauthorized                                │
│                                                                 │
│   3. Validate refresh token expiration:                         │
│      if (user.RefreshTokenExpiryTime < DateTime.UtcNow)        │
│        → Return 401 Unauthorized (token expired)                │
│                                                                 │
│   4. Generate NEW access token:                                 │
│      newAccessToken = _tokenService.GenerateAccessToken(user) │
│      → Same token format, but with new exp claim                │
│      → exp = now + 60 minutes                                   │
│                                                                 │
│   5. Return new access token:                                   │
│      {                                                          │
│        "accessToken": "eyJhbGc...",  ← NEW token                │
│        "tokenType": "Bearer",                                   │
│        "expiresIn": 3600                                        │
│      }                                                          │
└──────────────────┬──────────────────────────────────────────────┘
                   │ (Refresh token stays same, user.RefreshToken
                   │  is NOT changed)
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                         │
├─────────────────────────────────────────────────────────────────┤
│ AuthService receives new access token:                          │
│   {                                                             │
│     "accessToken": "eyJhbGc...",  ← NEW                         │
│     "expiresIn": 3600                                           │
│   }                                                             │
│                                                                 │
│ Update localStorage:                                            │
│   localStorage.setItem('tc_token', newAccessToken)             │
│   // tc_refresh_token remains unchanged                         │
│                                                                 │
│ Retry original API request with new token:                      │
│   Authorization: Bearer <NEW_ACCESS_TOKEN>                      │
│   ✓ Request succeeds                                            │
└─────────────────────────────────────────────────────────────────┘
```

---

#### **4. LOGOUT & TOKEN REVOCATION FLOW**

```
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (Angular)                         │
├─────────────────────────────────────────────────────────────────┤
│ User clicks "Logout"                                            │
│ ↓                                                               │
│ AuthService.logout():                                           │
│   1. Call server logout endpoint:                               │
│      POST /api/user/logout                                      │
│      Header: Authorization: Bearer <ACCESS_TOKEN>               │
│                                                                 │
│   2. Clear local storage:                                       │
│      localStorage.removeItem('tc_token')                        │
│      localStorage.removeItem('tc_refresh_token')               │
│      localStorage.removeItem('tc_userId')                       │
│                                                                 │
│   3. Redirect to login page                                     │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│                    API_LAYER (Controllers)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserController.Logout():                                        │
│   [Authorize] ← Validates token is valid                        │
│                                                                 │
│   // Extract userId from JWT claims:                            │
│   var userId = int.Parse(                                       │
│     User.FindFirst(ClaimTypes.NameIdentifier)?.Value           │
│   );                                                            │
│                                                                 │
│   // Call service to revoke token                               │
│   await _userService.LogoutAsync(userId);                      │
│   return Ok(new { success = true });                            │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│               APPLICATION_LAYER (Services)                      │
├─────────────────────────────────────────────────────────────────┤
│ UserService.LogoutAsync(userId):                                │
│   1. Get user from database                                     │
│      user = await _unitOfWork.Users.GetByIdAsync(userId)       │
│                                                                 │
│   2. Revoke refresh token:                                      │
│      user.RefreshToken = null                                   │
│      user.RefreshTokenExpiryTime = DateTime.MinValue            │
│      user.IsTokenBlacklisted = true  ← Mark as revoked          │
│                                                                 │
│   3. Save to database:                                          │
│      await _unitOfWork.Users.UpdateAsync(user)                 │
│      await _unitOfWork.SaveChangesAsync()                      │
│                                                                 │
│   4. Add to in-memory blacklist (for performance):              │
│      blacklist.Add(user.RefreshToken)                           │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE_LAYER (Database)                    │
├─────────────────────────────────────────────────────────────────┤
│ SQL Update:                                                     │
│   UPDATE Users                                                  │
│   SET RefreshToken = NULL,                                      │
│       RefreshTokenExpiryTime = '1900-01-01',                   │
│       IsTokenBlacklisted = 1                                    │
│   WHERE Id = 3                                                  │
│                                                                 │
│ Refresh token is permanently revoked                            │
└─────────────────────────────────────────────────────────────────┘

Result:
  - User cannot refresh token anymore
  - Any request with old token gets 401 Unauthorized
  - Must login again to get new tokens
```

---

### **Layer-by-Layer Responsibility Breakdown**

| Layer | Responsibility | Key Files |
|-------|---|---|
| **Frontend (Angular)** | Store tokens in localStorage, Add Authorization header to requests, Handle 401 errors | `auth.service.ts`, `auth.interceptor.ts`, `login.ts` |
| **API Controller** | Extract user ID from JWT claims, Apply [Authorize] attribute | `UserController.cs`, `TripController.cs`, etc |
| **Application Service** | Generate tokens, Validate passwords, Manage user database operations | `TokenService.cs`, `UserService.cs`, `RefreshTokenService.cs` |
| **Infrastructure (Database)** | Store users with refresh tokens, Update token status on logout | `User` entity, migrations |
| **Middleware** | Validate JWT signature, Extract claims, Set HttpContext.User | `Program.cs` (JwtBearer config) |

---



### **Existing Authentication Approach**
- **Method:** userId passed as query parameters in requests
- **Issue:** Stateless but not secure; vulnerable to tampering
- **Endpoints Affected:** 
  - POST /api/user/register (returns Token in AuthResponseDto)
  - POST /api/user/login (returns Token in AuthResponseDto)
  - All other endpoints requiring userId

### **Current Architecture**
- **API Layer:** Controllers with dependency injection (IService, ILogger)
- **Application Layer:** DependencyInjection.cs (adds services)
- **Infrastructure Layer:** DependencyInjection.cs (adds repositories & db)
- **Middleware:** ErrorHandlingMiddleware for exception handling
- **CORS:** Configured for localhost:4200 (Angular) and localhost:3000

---

## **Implementation Plan Overview**

### **Phase 1: Setup & Configuration**

#### **1.1 Add Required NuGet Packages**
**Location:** `API_LAYER.csproj`

Packages to add:
```
- System.IdentityModel.Tokens.Jwt (v7.0.0+)
- Microsoft.IdentityModel.Tokens (v7.0.0+)
- Microsoft.AspNetCore.Authentication.JwtBearer (v8.0.0+)
```

**Reason:** 
- `System.IdentityModel.Tokens.Jwt` → JWT creation and validation
- `Microsoft.IdentityModel.Tokens` → Token handlers and security keys
- `Microsoft.AspNetCore.Authentication.JwtBearer` → ASP.NET Core JWT middleware

---

#### **1.2 Update appsettings.json**
**Location:** `API_LAYER/appsettings.json`

Add JWT Configuration section:
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-32-characters-long!!",
    "Issuer": "TripConnectAPI",
    "Audience": "TripConnectApp",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Settings Details:**
- `SecretKey` → Must be at least 32 characters (256-bit for HS256)
- `Issuer` → Who creates the token
- `Audience` → Who can use the token
- `ExpirationMinutes` → Access token validity (recommended: 60 mins)
- `RefreshTokenExpirationDays` → Refresh token validity (recommended: 7 days)

---

#### **1.3 Update appsettings.Development.json**
**Location:** `API_LAYER/appsettings.Development.json`

Add development-specific JWT configuration:
```json
{
  "JwtSettings": {
    "SecretKey": "development-secret-key-min-32-characters-!!!",
    "ExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 30
  }
}
```

---

### **Phase 2: Models & DTOs**

#### **2.1 Create JWT Configuration Model**
**File:** `APPLICATION_LAYER/Models/JwtSettings.cs`

```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
```

---

#### **2.2 Update/Create JWT Response DTOs**
**File:** `APPLICATION_LAYER/DTOs/Auth/JwtTokenResponseDto.cs`

```csharp
public class JwtTokenResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; } // "Bearer"
    public int ExpiresIn { get; set; } // seconds
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

---

#### **2.3 Update AuthResponseDto**
**File:** `APPLICATION_LAYER/DTOs/User/AuthResponseDto.cs`

Modify to include JWT details:
```csharp
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public UserResponseDto User { get; set; }
    public JwtTokenResponseDto Token { get; set; } // Include JWT details
}
```

---

#### **2.4 Create Token Request DTOs**
**File:** `APPLICATION_LAYER/DTOs/Auth/RefreshTokenRequestDto.cs`

```csharp
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }
}
```

---

### **Phase 3: JWT Service Layer**

#### **3.1 Create JWT Token Service Interface**
**File:** `APPLICATION_LAYER/Services/ITokenService.cs`

```csharp
public interface ITokenService
{
    JwtTokenResponseDto GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    bool ValidateRefreshToken(string token, User user);
}
```

---

#### **3.2 Create JWT Token Service Implementation**
**File:** `APPLICATION_LAYER/Services/TokenService.cs`

**Responsibilities:**
- Generate JWT access tokens with user claims
- Generate refresh tokens (random strings stored in DB)
- Validate and decode expired tokens
- Calculate token expiration times

**Key Methods:**
```csharp
public JwtTokenResponseDto GenerateAccessToken(User user)
{
    // 1. Create claims from user data
    // 2. Create signing credentials from secret key
    // 3. Create JWT token with claims, issuer, audience
    // 4. Return token details with expiration
}

public string GenerateRefreshToken()
{
    // Generate cryptographically secure random string
    // Typically: Base64 encoded 32+ random bytes
}

public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
{
    // Validate token signature with expired clock skew
    // Return claims principal for refresh token flow
}
```

---

### **Phase 4: Database & Domain Updates**

#### **4.1 Update User Entity**
**File:** `DOMAIN_LAYER/Entity/User.cs`

Add properties for refresh token management:
```csharp
public class User
{
    // ... existing properties ...
    
    // Refresh Token Management
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public DateTime LastLoginAt { get; set; }
    public bool IsTokenBlacklisted { get; set; }
}
```

---

#### **4.2 Create Database Migration**
**File:** `INFRASTRUCTURE_LAYER/Migrations/[timestamp]_AddJwtTokenPropertiesToUser.cs`

**Changes:**
- Add RefreshToken column (nvarchar(max), nullable)
- Add RefreshTokenExpiryTime column (datetime2)
- Add LastLoginAt column (datetime2, nullable)
- Add IsTokenBlacklisted column (bit, default false)

---

### **Phase 5: Authentication Middleware**

#### **5.1 Create Custom JWT Authentication Middleware** (Alternative to Bearer)
**File:** `API_LAYER/Middleware/JwtAuthenticationMiddleware.cs`

**Purpose:**
- Extract JWT from Authorization header (Bearer scheme)
- Validate token signature
- Set HttpContext.User with claims
- Handle missing/invalid tokens

---

#### **5.2 Update Program.cs**
**Location:** `API_LAYER/Program.cs`

**Add to Service Configuration:**
```csharp
// Add JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
    
    // Custom error handling for JWT failures
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context => { /* ... */ },
        OnTokenValidated = context => { /* ... */ },
        OnChallenge = context => { /* ... */ }
    };
});

// Add Authorization
builder.Services.AddAuthorization();
```

**Update Middleware Pipeline:**
```csharp
// Before app.UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();
```

---

### **Phase 6: Service Layer Updates**

#### **6.1 Update UserService**
**File:** `APPLICATION_LAYER/Services/UserService.cs`

**Changes:**
- Inject ITokenService
- In `RegisterAsync()`: Generate tokens and store refresh token
- In `LoginAsync()`: Generate tokens and update last login
- Add `RefreshTokenAsync()` method
- Add `LogoutAsync()` method for token blacklisting

**Updated Register Method:**
```csharp
public async Task<AuthResponseDto> RegisterAsync(CreateUserDto dto)
{
    // 1. Validate email/username not exists
    // 2. Hash password
    // 3. Create user entity
    // 4. Save to database
    // 5. Generate JWT access & refresh tokens
    // 6. Store refresh token in User entity
    // 7. Return AuthResponseDto with tokens
}
```

---

#### **6.2 Create Token Refresh Service**
**File:** `APPLICATION_LAYER/Services/IRefreshTokenService.cs`

```csharp
public interface IRefreshTokenService
{
    Task<JwtTokenResponseDto> RefreshAccessTokenAsync(string refreshToken);
    Task RevokeTokenAsync(int userId);
    Task<bool> IsTokenValidAsync(string token, int userId);
}
```

---

### **Phase 7: Controller Updates**

#### **7.1 Update UserController**
**File:** `API_LAYER/Controllers/UserController.cs`

**Changes:**
- **Register Endpoint:** Return JWT in response
- **Login Endpoint:** Return JWT in response, update LastLoginAt
- **Add New Endpoint:** POST `/api/user/refresh-token` → Refresh access token
- **Add New Endpoint:** POST `/api/user/logout` → Revoke refresh token
- **Remove:** userId from query parameters (use from claims instead)

**Example Updated Login:**
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
{
    var authResponse = await _userService.LoginAsync(dto);
    return Ok(authResponse); // Includes JWT tokens
}

[HttpPost("refresh-token")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
{
    var tokenResponse = await _refreshTokenService.RefreshAccessTokenAsync(dto.RefreshToken);
    return Ok(tokenResponse);
}

[HttpPost("logout")]
[Authorize] // Requires valid JWT
public async Task<IActionResult> Logout()
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    await _userService.LogoutAsync(userId);
    return Ok(new { success = true, message = "Logged out successfully" });
}
```

---

#### **7.2 Update All Other Controllers**
**Affected Controllers:**
- TripController
- ExpenseController
- ChatController
- RatingController
- JoinRequestController

**Changes:**
- Add `[Authorize]` attribute to protected endpoints
- Replace query parameter `userId` with JWT claim extraction:
  ```csharp
  var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
  ```
- Update route documentation in API_LAYER.http file

**Example:**
```csharp
[HttpPost]
[Authorize] // ← Add this
public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto dto)
// Remove: [FromQuery] int userId
{
    // Extract userId from JWT claims
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    var result = await _tripService.CreateAsync(dto, userId);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

---

### **Phase 8: Error Handling Updates**

#### **8.1 Update ErrorHandlingMiddleware**
**File:** `API_LAYER/Middleware/ErrorHandlingMiddleware.cs`

**Add Handling For:**
- TokenExpiredException → 401 Unauthorized with "Token expired" message
- SecurityTokenInvalidSignatureException → 401 Unauthorized
- SecurityTokenValidationException → 401 Unauthorized
- InvalidOperationException (No token) → 401 Unauthorized

---

#### **8.2 Add Authorization Error Response**
**Handle 403 Forbidden** for insufficient permissions:
```json
{
    "success": false,
    "message": "Access denied. Insufficient permissions.",
    "errorCode": "FORBIDDEN"
}
```

---

### **Phase 9: API Documentation Updates**

#### **9.1 Update API_LAYER_DOCUMENTATION.md**
**Changes:**
- Document JWT authentication flow
- Update endpoint documentation:
  - Remove `userId` from query parameters
  - Add `Authorization: Bearer <token>` header requirement
- Add Authentication Endpoints section
- Add Token Refresh flow example

**Example Updated Endpoint:**
```
POST /api/trip
    Authorization: Bearer <JWT_TOKEN>  ← Add this
    Input: CreateTripDto { ... }
    Output: TripResponseDto
    Status: 201 Created | 401 Unauthorized | 403 Forbidden
```

---

#### **9.2 Update API_LAYER.http**
**File:** `API_LAYER/API_LAYER.http`

**Changes:**
- Add JWT token variable: `@token = <Bearer Token>`
- Update all requests with Authorization header:
  ```
  Authorization: Bearer {{token}}
  ```
- Add sample requests for:
  - Register
  - Login
  - Refresh token
  - Logout

---

### **Phase 10: Security Implementations**

#### **10.1 Token Blacklisting Strategy**
**Approach:** Simple in-memory set (production: Redis/cache)
```csharp
public interface ITokenBlacklistService
{
    void AddToBlacklist(string token);
    bool IsBlacklisted(string token);
}
```

**Use in:**
- Logout endpoint
- Token refresh validation

---

#### **10.2 Password Hashing**
**Verify** existing implementation:
- Ensure BCrypt or similar is used
- Minimum 10+ hash rounds
- Update in UserService if needed

---

#### **10.3 HTTPS Enforcement**
**Verify in Program.cs:**
```csharp
app.UseHttpsRedirection(); // ✓ Already configured
```

---

#### **10.4 CORS Security Review**
**Current Configuration:** Allows specific origins ✓
```csharp
.WithOrigins(
    "http://localhost:4200",
    "https://localhost:4200"
)
.AllowCredentials(); // ✓ Allows credentials in CORS requests
```

---

### **Phase 11: Testing & Validation**

#### **11.1 Unit Tests**
**Test Files to Create:**
- `Tests/Services/TokenServiceTests.cs`
- `Tests/Services/UserServiceTests.cs`
- `Tests/Middleware/JwtAuthenticationTests.cs`

**Test Cases:**
- Valid token generation
- Token expiration validation
- Invalid token handling
- Refresh token flow
- Logout/revocation

---

#### **11.2 Integration Tests**
**Test Files to Create:**
- `Tests/Controllers/UserControllerAuthTests.cs`
- `Tests/Controllers/TripControllerAuthTests.cs`

**Test Cases:**
- Register → Login → Access Protected Endpoint
- Token Refresh Flow
- Expired Token Rejection
- Missing Authorization Header

---

#### **11.3 Manual Testing**
**Tools:** Postman, Thunder Client, or REST Client in VS Code

**Test Flow:**
1. **Register:** GET token
2. **Login:** GET token
3. **Access Trip:** Use token in header → Success
4. **Refresh:** POST refresh token → GET new token
5. **Logout:** POST logout → Next request fails (401)
6. **Expired Token:** Wait or manipulate → Should fail

---

### **Phase 12: Frontend Integration (Angular)**

#### **12.1 HTTP Interceptor Updates**
**File:** `TripConnect_Frontend/src/app/interceptors/auth.interceptor.ts`

**Changes:**
- Add JWT token to request headers:
  ```typescript
  const token = this.authService.getToken();
  request = request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
  ```
- Handle 401 responses → Redirect to login

---

#### **12.2 Auth Service Updates**
**File:** `TripConnect_Frontend/src/app/services/auth.service.ts`

**Methods to Add/Update:**
- `storeToken(token)` → Save JWT to localStorage
- `getToken()` → Retrieve JWT
- `removeToken()` → Clear JWT
- `isTokenExpired()` → Check expiration
- `refreshToken()` → Call refresh endpoint

---

---

## **Implementation Sequence (Recommended Order)**

1. **Phase 1:** Setup & Configuration (NuGet, appsettings)
2. **Phase 2:** Models & DTOs
3. **Phase 3:** JWT Service Layer
4. **Phase 4:** Database Updates & Migrations
5. **Phase 5:** Authentication Middleware & Program.cs
6. **Phase 6:** Service Layer Updates
7. **Phase 7:** Controller Updates
8. **Phase 8:** Error Handling
9. **Phase 9:** API Documentation
10. **Phase 10:** Security Implementations
11. **Phase 11:** Testing & Validation
12. **Phase 12:** Frontend Integration

---

## **Key Security Considerations**

| Aspect | Consideration | Status |
|--------|---------------|--------|
| **Secret Key** | Min 256-bit (32 chars) | ✓ To implement |
| **Token Signing** | Use HS256 (HMAC-SHA256) | ✓ To implement |
| **Token Expiration** | Short-lived access (60 min) | ✓ To implement |
| **Refresh Tokens** | Longer-lived (7 days), stored in DB | ✓ To implement |
| **HTTPS Only** | All token transmission over HTTPS | ✓ Already configured |
| **HttpOnly Cookies** | Optional: Store tokens in HttpOnly cookies | ○ Optional |
| **Token Revocation** | Blacklist on logout | ✓ To implement |
| **Clock Skew** | Set to zero for strict validation | ✓ To implement |
| **Password Hashing** | Verify BCrypt usage | ✓ To verify |
| **CORS Headers** | Validate allowed origins | ✓ Already configured |

---

## **Affected Endpoints Summary**

| Endpoint | Current Method | Changes |
|----------|----------------|---------|
| POST /api/user/register | Manual token creation | Add JWT generation |
| POST /api/user/login | Manual token creation | Add JWT generation |
| POST /api/user/logout | N/A (New) | Add logout with revocation |
| POST /api/user/refresh-token | N/A (New) | Add refresh token endpoint |
| POST /api/trip | userId parameter | Remove param, use JWT claim |
| GET /api/trip/{id} | No auth | Add [Authorize] |
| POST /api/trip | Auth + userId param | Combine auth |
| ... (60 total endpoints) | Mixed auth | Add [Authorize] consistently |

---

## **Deliverables**

### **Code Files to Create/Modify:**
1. ✓ `API_LAYER.csproj` (NuGet packages)
2. ✓ `appsettings.json` & `appsettings.Development.json` (JWT config)
3. ✓ `APPLICATION_LAYER/Models/JwtSettings.cs` (New)
4. ✓ `APPLICATION_LAYER/DTOs/Auth/JwtTokenResponseDto.cs` (New)
5. ✓ `APPLICATION_LAYER/Services/ITokenService.cs` (New)
6. ✓ `APPLICATION_LAYER/Services/TokenService.cs` (New)
7. ✓ `APPLICATION_LAYER/Services/IRefreshTokenService.cs` (New)
8. ✓ `APPLICATION_LAYER/Services/RefreshTokenService.cs` (New)
9. ✓ `DOMAIN_LAYER/Entity/User.cs` (Modified)
10. ✓ `INFRASTRUCTURE_LAYER/Migrations/[timestamp]_AddJwtTokenPropertiesToUser.cs` (New)
11. ✓ `API_LAYER/Program.cs` (Modified)
12. ✓ `API_LAYER/Middleware/ErrorHandlingMiddleware.cs` (Modified)
13. ✓ `API_LAYER/Controllers/UserController.cs` (Modified)
14. ✓ `API_LAYER/Controllers/TripController.cs` (Modified - add [Authorize])
15. ✓ `API_LAYER/Controllers/ExpenseController.cs` (Modified)
16. ✓ `API_LAYER/Controllers/ChatController.cs` (Modified)
17. ✓ `API_LAYER/Controllers/RatingController.cs` (Modified)
18. ✓ `API_LAYER/Controllers/JoinRequestController.cs` (Modified)
19. ✓ `API_LAYER/API_LAYER_DOCUMENTATION.md` (Updated)
20. ✓ `API_LAYER/API_LAYER.http` (Updated)

### **Testing:**
- Unit tests for TokenService & UserService
- Integration tests for auth flow
- Manual testing with REST client

### **Documentation:**
- Updated API_LAYER_DOCUMENTATION.md
- JWT flow diagram
- Security guidelines

---

## **Rollback Strategy**

If issues arise:
1. Revert Program.cs authentication setup
2. Comment out [Authorize] attributes
3. Restore original userId parameter extraction
4. Roll back database migration

---

## **Success Criteria**

- [ ] Register endpoint returns valid JWT
- [ ] Login endpoint returns valid JWT
- [ ] All protected endpoints require [Authorize]
- [ ] Token validation works correctly
- [ ] Refresh token endpoint works
- [ ] Logout revokes tokens
- [ ] Expired tokens are rejected
- [ ] Invalid tokens are rejected
- [ ] Frontend successfully uses JWT in requests
- [ ] All tests pass

---

**Plan Created:** 2026-04-21  
**Status:** Ready for Implementation  
**Estimated Duration:** 3-4 days (1 developer)
