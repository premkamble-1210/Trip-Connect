# APPLICATION LAYER DOCUMENTATION

## 1. Overview

The **Application Layer** is the business logic orchestration tier that implements use cases, coordinates domain entities, and mediates between the API layer and infrastructure layer. It contains all service implementations, data transfer objects (DTOs), and entity-to-DTO mappings via AutoMapper.

**Purpose:**
- Implement business logic workflows independent of framework/infrastructure details
- Transform domain entities to/from DTOs for API consumption
- Aggregate repository calls and cache operations
- Enforce validation rules and authorization checks
- Handle external service integrations (email, SMS, image uploads)
- Manage JWT token generation and refresh flows

**Key Principles:**
- Dependency Injection for all external service dependencies
- Dependency inversion: Services depend on Domain layer repository interfaces, not Infrastructure implementations
- Cache-aware operations: Services integrate with ICacheService for read/write caching
- Comprehensive logging via Serilog for debugging and monitoring
- Exception propagation with meaningful context to API layer

---

## 2. Project Structure

```
APPLICATION_LAYER/
├── APPLICATION_LAYER.csproj           # NuGet dependencies
├── DependencyInjection.cs             # Service registration
├── Models/                            # Configuration classes
│   ├── EmailSettings.cs               # SMTP configuration
│   ├── JwtSettings.cs                 # JWT token configuration
│   └── TwilioSettings.cs              # SMS service configuration
├── Services/
│   ├── Interfaces/                    # Service contracts
│   │   ├── IChatService.cs
│   │   ├── IEmailService.cs
│   │   ├── IExpenseService.cs
│   │   ├── IImageService.cs
│   │   ├── IJoinRequestService.cs
│   │   ├── IRatingService.cs
│   │   ├── IRefreshTokenService.cs
│   │   ├── ISmsService.cs
│   │   ├── ITokenService.cs
│   │   ├── ITripService.cs
│   │   └── IUserService.cs
│   └── Implementations/               # Service implementations
│       ├── ChatService.cs
│       ├── EmailService.cs
│       ├── ExpenseService.cs
│       ├── ImageService.cs
│       ├── JoinRequestService.cs
│       ├── RatingService.cs
│       ├── RefreshTokenService.cs
│       ├── SmsService.cs
│       ├── TokenService.cs
│       ├── TripService.cs
│       └── UserService.cs
├── DTOs/                              # Data Transfer Objects (organized by domain)
│   ├── Auth/
│   │   ├── JwtTokenResponseDto.cs     # Token details (access, refresh, expiry)
│   │   └── RefreshTokenRequestDto.cs  # Refresh token request
│   ├── User/
│   │   ├── LoginUserDto.cs            # Login credentials (username, password)
│   │   ├── CreateUserDto.cs           # Registration (name, email, username, phone, password)
│   │   ├── UpdateUserDto.cs           # Profile update (name, phone)
│   │   ├── UserResponseDto.cs         # User info (all fields except secrets)
│   │   ├── AuthResponseDto.cs         # Auth result (success, message, token, user)
│   │   └── VerifyPhoneOtpDto.cs       # Phone OTP verification (userId, otp)
│   ├── Trip/
│   │   ├── CreateTripDto.cs           # Trip creation (title, location, budget, dates, etc.)
│   │   ├── UpdateTripDto.cs           # Trip update
│   │   ├── TripResponseDto.cs         # Trip info with host/members/status
│   │   ├── TripDayDto.cs              # Itinerary item for input
│   │   └── TripDayResponseDto.cs      # Itinerary item for output
│   ├── TripMember/
│   │   └── TripMemberResponseDto.cs   # Member info (userId, userName, role, status)
│   ├── JoinRequest/
│   │   ├── SendJoinRequestDto.cs      # Request to join (tripId)
│   │   ├── JoinRequestResponseDto.cs  # Request details (id, userId, userName, status)
│   │   └── UpdateJoinRequestDto.cs    # Accept/reject update
│   ├── Expense/
│   │   ├── CreateExpenseDto.cs        # Create expense with splits (tripId, amount, splits[])
│   │   ├── ExpenseResponseDto.cs      # Expense details (id, tripId, paidBy, paidByName, amount, splits[])
│   │   └── ExpenseSplitResponseDto.cs # Split details (id, userId, userName, amountOwed, isSettled)
│   ├── Chat/
│   │   ├── SendMessageDto.cs          # Message input (tripId, message)
│   │   └── ChatMessageResponseDto.cs  # Message output (id, tripId, userId, userName, message, timestamp)
│   ├── Rating/
│   │   ├── CreateRatingDto.cs         # Rating input (tripId, ratedUserId, rating, review)
│   │   └── RatingResponseDto.cs       # Rating output (id, tripId, ratedBy, ratedByName, ratedUser, rating, review)
│   ├── ImageUploadRequestDto.cs       # Image upload request (file stream, fileName)
│   └── ImageUploadResponseDto.cs      # Image upload response (fileId, imageUrl, publicUrl, success, message)
└── Mappers/                           # AutoMapper profiles (Entity ↔ DTO)
    ├── ChatMessageProfile.cs          # ChatMessage → ChatMessageResponseDto
    ├── ExpenseProfile.cs              # Expense → ExpenseResponseDto
    ├── ImageMappingProfile.cs         # Image entity mappings
    ├── TripProfile.cs                 # Trip → TripResponseDto, CreateTripDto → Trip
    ├── TripRatingProfile.cs           # TripRating → RatingResponseDto
    ├── TripRequestProfile.cs          # TripRequest → JoinRequestResponseDto
    └── UserProfile.cs                 # User → UserResponseDto, CreateUserDto → User
```

---

## 3. Dependencies

**NuGet Packages:**
- **AutoMapper 16.1.1** — Entity-to-DTO mapping with configuration-based transformations
- **MailKit 4.16.0** — SMTP-based transactional email sending
- **Microsoft.Extensions.Options.ConfigurationExtensions 10.0.7** — IOptions<T> configuration binding
- **Twilio 7.6.0** — SMS sending via Twilio Verify service

**Project References:**
- **DOMAIN_LAYER** — Entities, enumerations, and repository interfaces
- **INFRASTRUCTURE_LAYER** — Repository implementations, cache service, logging, external service integrations

---

## 4. Service Interfaces & Implementations

### 4.1 IUserService / UserService

**Purpose:** User authentication, registration, profile management, and verification

**Interface Methods:**
```csharp
Task<AuthResponseDto> RegisterAsync(CreateUserDto dto)
Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
Task<UserResponseDto> GetUserByIdAsync(int userId)
Task<UserResponseDto> GetUserByEmailAsync(string email)
Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserDto dto)
Task<double> GetUserRatingAsync(int userId)
Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
Task<bool> VerifyPhoneAsync(int userId)
Task<bool> VerifyIdAsync(int userId)
Task<bool> UsernameExistsAsync(string username)
Task<bool> EmailExistsAsync(string email)
Task LogoutAsync(int userId)
Task RequestEmailVerificationAsync(int userId)
Task<bool> VerifyEmailAsync(string token)
Task RequestPhoneVerificationAsync(int userId)
Task<bool> VerifyPhoneOtpAsync(int userId, string otp)
```

**Implementation Logic:**

**RegisterAsync:**
1. Validate email not already registered
2. Validate username not taken
3. Generate password salt (cryptographically secure)
4. Hash password with salt (PBKDF2 pattern)
5. Generate 7-day refresh token
6. Create User entity with initial state (PhoneVerified=false, IdVerified=false, Rating=0.0)
7. Persist to database via IUnitOfWork.Users.AddAsync
8. Generate JWT access token via ITokenService
9. Return AuthResponseDto with token and user info

**LoginAsync:**
1. Fetch user by username via IUnitOfWork.Users.GetByUsernameAsync
2. Verify password against hash+salt
3. Generate new refresh token
4. Update user LastLoginAt, RefreshToken, RefreshTokenExpiryTime
5. Generate JWT access token
6. Return AuthResponseDto

**LogoutAsync:**
1. Clear RefreshToken and RefreshTokenExpiryTime
2. Set IsTokenBlacklisted=true
3. Invalidate cache entries by tag "user:{userId}"

**Caching Pattern:**
- GetUserByIdAsync: Cache key `user:byid:{userId}`, TTL 2 hours, tags ["user:{userId}", "user:all"]
- GetUserByEmailAsync: Cache key `user:byemail:{email}`, TTL 2 hours, tags ["user:email:{email}", "user:all"]
- Cache miss triggers repository fetch, cache write via ICacheService.SetAsync
- Cache hit returns mapped DTO directly

**Error Handling:**
- Email already registered → InvalidOperationException
- Username taken → InvalidOperationException
- Invalid credentials (login) → InvalidOperationException
- User not found → InvalidOperationException
- All exceptions logged at ERROR level with context

---

### 4.2 ITripService / TripService

**Purpose:** Trip creation, search, retrieval, member management, and lifecycle

**Interface Methods:**
```csharp
Task<TripResponseDto> CreateTripAsync(CreateTripDto dto, int userId)
Task<TripResponseDto> GetTripByIdAsync(int tripId)
Task<IEnumerable<TripResponseDto>> GetAllTripsAsync(int pageNumber = 1, int pageSize = 10)
Task<IEnumerable<TripResponseDto>> GetTripsByStatusAsync(string status)
Task<IEnumerable<TripResponseDto>> GetUpcomingTripsAsync()
Task<IEnumerable<TripResponseDto>> SearchTripsByLocationAsync(string location)
Task<IEnumerable<TripResponseDto>> SearchTripsAsync(string location, DateTime? startDate, decimal? maxBudget, string travelType)
Task<IEnumerable<TripResponseDto>> GetTripsCreatedByUserAsync(int userId)
Task<TripResponseDto> UpdateTripAsync(int tripId, UpdateTripDto dto, int userId)
Task<bool> CancelTripAsync(int tripId, int userId)
Task<IEnumerable<TripResponseDto>> GetUserTripsAsync(int userId)
Task<IEnumerable<TripMemberResponseDto>> GetTripMembersAsync(int tripId)
```

**Implementation Logic:**

**CreateTripAsync:**
1. Map CreateTripDto → Trip entity
2. Set HostId from userId
3. Set Status = TripStatus.Planned
4. Set CreatedAt = DateTime.UtcNow
5. Map TripDays from DTO list if present
6. Persist via IUnitOfWork.Trips.AddAsync
7. Invalidate cache tags "trip:all", "trip:user:{userId}"
8. Return mapped TripResponseDto

**GetTripByIdAsync:**
1. Cache key `trip:byid:{tripId}`, TTL 1 hour
2. Cache miss → fetch via IUnitOfWork.Trips.GetByIdAsync
3. SetAsync with tags ["trip:{tripId}", "trip:all"]
4. Return mapped DTO

**GetAllTripsAsync:**
1. Cache key `trip:all:page:{pageNumber}:size:{pageSize}`
2. Cache miss → fetch all trips, apply Skip/Take for pagination
3. SetAsync with tags ["trip:all", "trip:list"], TTL 1 hour
4. Return paginated mapped DTOs

**SearchTripsAsync:**
1. Validate location/startDate/maxBudget/travelType parameters
2. Cache key includes all parameters (location, startDate, maxBudget, travelType)
3. Cache miss → query via IUnitOfWork.Trips.SearchTripsAsync
4. Return mapped DTOs, TTL 2 hours

**GetUpcomingTripsAsync:**
1. Fetch trips with StartDate > DateTime.UtcNow
2. Cache key `trip:upcoming`, TTL 30 minutes (frequent changes)
3. Return mapped DTOs

**Caching Strategy:**
- Upcoming trips: 30 min TTL (high volatility)
- Search results: 2 hour TTL
- Single trip: 1 hour TTL
- All invalid on CreateTrip, UpdateTrip, CancelTrip

---

### 4.3 IExpenseService / ExpenseService

**Purpose:** Expense creation, tracking, splitting, and settlement calculations

**Interface Methods:**
```csharp
Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto, int userId)
Task<ExpenseResponseDto> GetExpenseByIdAsync(int expenseId)
Task<IEnumerable<ExpenseResponseDto>> GetExpensesByTripAsync(int tripId)
Task<IEnumerable<ExpenseResponseDto>> GetExpensesPaidByUserAsync(int userId)
Task<decimal> GetTotalExpensesByTripAsync(int tripId)
Task<decimal> GetTotalOwedByUserAsync(int userId, int tripId)
Task<IEnumerable<ExpenseResponseDto>> GetUnsettledExpensesByUserAsync(int userId, int tripId)
Task<bool> SettleExpenseAsync(int splitId)
Task<bool> DeleteExpenseAsync(int expenseId, int userId)
Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int tripId)
```

**Implementation Logic:**

**CreateExpenseAsync:**
1. Validate trip exists via IUnitOfWork.Trips.GetByIdAsync
2. Verify userId is trip member or host
3. Create Expense entity with PaidBy=userId, Amount, Description
4. For each split in CreateExpenseDto.Splits:
   - Create ExpenseSplit entity with UserId, AmountOwed
5. Persist via IUnitOfWork.Expenses.AddAsync + SaveChangesAsync
6. Invalidate cache tags "trip:{tripId}:expenses", "user:{userId}:expenses"
7. Return ExpenseResponseDto with PaidByName included

**GetExpenseByIdAsync:**
1. Cache key `expense:byid:{expenseId}`, TTL 1 hour
2. Cache miss → fetch via IUnitOfWork.Expenses.GetExpenseWithSplitsAsync (eager-loads splits)
3. Map to DTO, include PaidByUser lookup for PaidByName
4. Return DTO

**GetExpensesByTripAsync:**
1. Validate trip exists
2. Fetch all expenses for trip
3. For each expense, lookup PaidByUser and map to DTO
4. Return list of DTOs (no caching for per-trip list)

**GetTotalOwedByUserAsync:**
1. Query unsettled ExpenseSplits for userId in tripId
2. Sum AmountOwed where IsSettled=false
3. Return decimal total

**SettleExpenseAsync:**
1. Fetch ExpenseSplit by splitId
2. Set IsSettled=true
3. Update via IUnitOfWork.ExpenseSplits.UpdateAsync
4. Clear related cache entries

---

### 4.4 IJoinRequestService / JoinRequestService

**Purpose:** Trip join request workflow (send, accept, reject, cancel)

**Interface Methods:**
```csharp
Task<JoinRequestResponseDto> SendJoinRequestAsync(SendJoinRequestDto dto, int userId)
Task<JoinRequestResponseDto> GetJoinRequestByIdAsync(int requestId)
Task<IEnumerable<JoinRequestResponseDto>> GetPendingRequestsByTripAsync(int tripId)
Task<IEnumerable<JoinRequestResponseDto>> GetRequestsByUserAsync(int userId)
Task<bool> AcceptJoinRequestAsync(int requestId, int tripHostId)
Task<bool> RejectJoinRequestAsync(int requestId, int tripHostId)
Task<bool> CancelJoinRequestAsync(int requestId, int userId)
Task<bool> HasUserRequestedAsync(int userId, int tripId)
Task<IEnumerable<JoinRequestResponseDto>> GetAllRequestsByTripAsync(int tripId)
```

**Implementation Logic:**

**SendJoinRequestAsync:**
1. Validate trip exists
2. Check user not already member (via TripMember query)
3. Check no pending request exists (HasUserRequestedAsync)
4. Create TripRequest: Status=TripRequestStatus.Pending, CreatedAt=now
5. Persist and return mapped DTO

**AcceptJoinRequestAsync:**
1. Fetch TripRequest by requestId
2. Verify caller is trip host
3. Set Status=TripRequestStatus.Accepted, UpdatedAt=now
4. Create TripMember: UserId, TripId, Role=TripMemberRole.Member, Status=TripMemberStatus.Active
5. Persist both updates
6. Return true

**RejectJoinRequestAsync:**
1. Fetch TripRequest
2. Verify caller is trip host
3. Set Status=TripRequestStatus.Rejected, UpdatedAt=now
4. Persist
5. Return true

---

### 4.5 ITokenService / TokenService

**Purpose:** JWT access token generation and validation for authentication

**Interface Methods:**
```csharp
JwtTokenResponseDto GenerateAccessToken(User user)
string GenerateRefreshToken()
ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
```

**Implementation Logic:**

**GenerateAccessToken:**
1. Create SymmetricSecurityKey from JwtSettings.SecretKey (minimum 32 chars for HS256)
2. Create SigningCredentials with HmacSha256 algorithm
3. Create Claims array:
   - ClaimTypes.NameIdentifier → user.Id
   - ClaimTypes.Email → user.Email
   - ClaimTypes.Name → user.Username
   - "FullName" → user.Name
   - "Phone" → user.Phone
4. Calculate issuedAt = DateTime.UtcNow
5. Calculate expiresAt = issuedAt + JwtSettings.ExpirationMinutes (default 60 min)
6. Create JwtSecurityToken:
   - issuer: JwtSettings.Issuer
   - audience: JwtSettings.Audience
   - claims: [claims array]
   - notBefore: issuedAt
   - expires: expiresAt
   - signingCredentials: [signed with secret key]
7. Serialize with JwtSecurityTokenHandler.WriteToken
8. Return JwtTokenResponseDto with AccessToken, TokenType="Bearer", ExpiresIn (seconds), IssuedAt, ExpiresAt

**GenerateRefreshToken:**
1. Generate 32 random bytes via RandomNumberGenerator.Create()
2. Convert to Base64 string
3. Return as refresh token (typically stored in database with 7-day expiry)

**GetPrincipalFromExpiredToken:**
1. Create TokenValidationParameters with ValidateLifetime=false (allows expired tokens)
2. Validate token signature and claims (issuer/audience)
3. Validate algorithm is HmacSha256
4. Extract and return ClaimsPrincipal
5. Used by RefreshTokenService to refresh expired access tokens

---

### 4.6 IRefreshTokenService / RefreshTokenService

**Purpose:** Token refresh workflow (expired access token → new access token)

**Interface Methods:**
```csharp
Task<JwtTokenResponseDto> RefreshAccessTokenAsync(string refreshToken)
Task RevokeTokenAsync(int userId)
Task<bool> IsTokenValidAsync(string token, int userId)
```

**Implementation Logic:**

**RefreshAccessTokenAsync:**
1. Validate refreshToken is not empty
2. Fetch user by refreshToken via IUnitOfWork.Users.GetByRefreshTokenAsync
3. Check RefreshTokenExpiryTime > DateTime.UtcNow
4. Check IsTokenBlacklisted != true (revoked)
5. Generate new access token via ITokenService.GenerateAccessToken
6. Optionally rotate refresh token: generate new, update user, persist
7. Return new JwtTokenResponseDto

---

### 4.7 IEmailService / EmailService

**Purpose:** Transactional email sending via SMTP/MailKit

**Interface Methods:**
```csharp
Task SendEmailVerificationAsync(string toEmail, string toName, string verificationUrl)
```

**Implementation Logic:**

**SendEmailVerificationAsync:**
1. Bind EmailSettings from IOptions<EmailSettings>
2. Create MimeMessage:
   - From: EmailSettings.SenderEmail / SenderName
   - To: toEmail / toName
   - Subject: "Verify your TripConnect email address"
3. Build HTML body with:
   - Greeting with recipient name
   - Verification link (24-hour expiry note)
   - HTML button + fallback URL text
   - Safety note
4. Create SMTP client from EmailSettings:
   - Host: SmtpHost, Port: SmtpPort
   - SecureSocketOptions: SSLOnConnect (port 465) or StartTls (other ports)
5. Connect, authenticate with SenderEmail/SenderPassword
6. Send message
7. Disconnect
8. Log INFO on success, ERROR on failure
9. Rethrow exceptions to API layer

**Configuration (appsettings.json):**
```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "noreply@tripconnect.com",
  "SenderPassword": "app-password",
  "SenderName": "TripConnect Team"
}
```

---

### 4.8 ISmsService / SmsService

**Purpose:** SMS/OTP sending via Twilio Verify service

**Interface Methods:**
```csharp
Task SendVerificationAsync(string toPhoneNumber)
Task<bool> CheckVerificationAsync(string toPhoneNumber, string code)
```

**Implementation Logic:**

**SendVerificationAsync:**
1. Bind TwilioSettings from IOptions<TwilioSettings>
2. Create TwilioClient with AccountSid/AuthToken
3. Call VerificationResource.CreateAsync:
   - To: toPhoneNumber (international format)
   - Channel: "sms"
   - ServicSid: VerifyServiceSid
4. Log INFO on success
5. Exceptions logged at ERROR level and rethrown

**CheckVerificationAsync:**
1. Create TwilioClient
2. Call VerificationCheckResource.CreateAsync:
   - To: toPhoneNumber
   - Code: user-submitted OTP
   - ServiceSid: VerifyServiceSid
3. Return VerificationCheckResource.Status == "approved"

**Configuration:**
```json
"TwilioSettings": {
  "AccountSid": "...",
  "AuthToken": "...",
  "VerifyServiceSid": "..."
}
```

---

### 4.9 IImageService / ImageService

**Purpose:** Image upload/deletion via ImageKit REST API

**Interface Methods:**
```csharp
Task<ImageUploadResponseDto> UploadProfileImageAsync(Stream imageStream, string fileName, int userId)
Task<ImageUploadResponseDto> UploadTripImageAsync(Stream imageStream, string fileName, int tripId)
Task<ImageUploadResponseDto> UploadExpenseImageAsync(Stream imageStream, string fileName, int expenseId)
Task<bool> DeleteImageAsync(string fileId)
Task<string> GetFileIdFromUrlAsync(string imageUrl)
Task<string> GetUrlFromFileIdAsync(string fileId)
```

**Implementation Logic:**

**UploadProfileImageAsync/UploadTripImageAsync/UploadExpenseImageAsync:**
1. Validate stream not null/empty
2. Validate file extension (default: jpg, jpeg, png, webp)
3. Validate file size ≤ 5MB
4. Sanitize fileName (remove special chars)
5. Read stream to byte[]
6. POST to https://upload.imagekit.io/api/v1/files/upload with:
   - Authorization: Basic auth (base64 PrivateKey:)
   - multipart/form-data with fileName, bytes, folder (profile/trip/expense)
   - Optional: customCoordinates, tags
7. Parse response JSON for fileId, url, dimensions
8. Return ImageUploadResponseDto with FileId, ImageUrl, PublicUrl, Width, Height, FileSize, Success=true
9. Log success/failure, throw on critical errors

**DeleteImageAsync:**
1. DELETE to https://api.imagekit.io/v1/files/{fileId}
2. Authorization: Basic auth
3. Return true on success, false on 404/error (no exception)

**GetFileIdFromUrlAsync/GetUrlFromFileIdAsync:**
1. Extract fileId from URL path (utility for URL ↔ fileId conversion)
2. ImageKit public URL format: https://{UrlEndpoint}/tr:w-400,h-300/{fileId}

**Configuration:**
```json
"ImageKit": {
  "PublicKey": "...",
  "PrivateKey": "...",
  "UrlEndpoint": "https://ik.imagekit.io/..."
}
```

---

### 4.10 IChatService / ChatService

**Purpose:** Trip chat message creation, retrieval, and deletion

**Interface Methods:**
```csharp
Task<ChatMessageResponseDto> SendMessageAsync(SendMessageDto dto, int userId)
Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripAsync(int tripId)
Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripPaginatedAsync(int tripId, int pageNumber, int pageSize)
Task<IEnumerable<ChatMessageResponseDto>> GetLatestMessagesAsync(int tripId, int count)
Task<IEnumerable<ChatMessageResponseDto>> GetMessagesBySenderAsync(int userId)
Task<IEnumerable<ChatMessageResponseDto>> SearchMessagesAsync(int tripId, string searchText)
Task<int> GetMessageCountAsync(int tripId)
Task<bool> DeleteMessageAsync(int messageId, int userId)
```

**Implementation Logic:**

**SendMessageAsync:**
1. Validate trip exists and user is member
2. Create ChatMessage: TripId, SenderId=userId, Content=message, CreatedAt=now
3. Persist via IUnitOfWork.Chat.AddAsync
4. Invalidate cache tags "trip:{tripId}:chat"
5. Return mapped ChatMessageResponseDto

**GetMessagesByTripAsync:**
1. Fetch all ChatMessages for trip, ordered by CreatedAt DESC
2. Map each to DTO with SenderName lookup
3. Return list

**GetMessagesByTripPaginatedAsync:**
1. Fetch all messages, apply Skip/Take pagination
2. Return paginated DTOs

**GetLatestMessagesAsync:**
1. Fetch last `count` messages by CreatedAt DESC
2. Reverse to chronological order
3. Return DTOs

**SearchMessagesAsync:**
1. Search ChatMessages where Content contains searchText (case-insensitive)
2. Filter to specific tripId
3. Return matching DTOs

---

### 4.11 IRatingService / RatingService

**Purpose:** User rating creation, retrieval, and statistics

**Interface Methods:**
```csharp
Task<RatingResponseDto> CreateRatingAsync(CreateRatingDto dto, int userId)
Task<RatingResponseDto> GetRatingByIdAsync(int ratingId)
Task<IEnumerable<RatingResponseDto>> GetRatingsForUserAsync(int userId)
Task<double> GetAverageRatingAsync(int userId)
Task<IEnumerable<RatingResponseDto>> GetRatingsGivenByUserAsync(int userId)
Task<IEnumerable<RatingResponseDto>> GetRatingsForTripAsync(int tripId)
Task<bool> HasUserRatedAsync(int tripId, int ratedBy, int ratedUserId)
Task<RatingResponseDto> UpdateRatingAsync(int ratingId, CreateRatingDto dto, int userId)
Task<bool> DeleteRatingAsync(int ratingId, int userId)
Task<int> GetRatingsCountAsync(int userId)
```

**Implementation Logic:**

**CreateRatingAsync:**
1. Validate trip exists, user and ratedUserId are different
2. Check no duplicate rating exists (RatedBy=userId, RatedUserId unique pair)
3. Create TripRating: TripId, RatedBy=userId, RatedUserId, Rating (1.0-5.0), Review
4. Persist via IUnitOfWork.TripRatings.AddAsync
5. Update User.Rating field via averaging algorithm (in-memory or via repository)
6. Invalidate cache tags "user:{ratedUserId}:rating", "trip:{tripId}:ratings"
7. Return mapped RatingResponseDto

**GetAverageRatingAsync:**
1. Fetch all TripRatings where RatedUserId=userId
2. Calculate average(Rating) in-memory or via repository query
3. Return double

**GetRatingsForTripAsync:**
1. Fetch all TripRatings for tripId
2. For each rating, lookup RatedBy user and RatedUser
3. Map to DTOs with names
4. Return list

---

## 5. Data Transfer Objects (DTOs)

### 5.1 Authentication DTOs

**LoginUserDto:**
```csharp
public class LoginUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

**CreateUserDto:**
```csharp
public class CreateUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
}
```

**UserResponseDto:**
```csharp
public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Phone { get; set; }
    public double Rating { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IdVerified { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**AuthResponseDto:**
```csharp
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public JwtTokenResponseDto Token { get; set; }
    public UserResponseDto User { get; set; }
}
```

**JwtTokenResponseDto:**
```csharp
public class JwtTokenResponseDto
{
    public string AccessToken { get; set; }           // JWT bearer token
    public string RefreshToken { get; set; }          // Refresh token
    public string TokenType { get; set; }             // "Bearer"
    public int ExpiresIn { get; set; }                // Seconds until expiry (600 for 60 min)
    public DateTime IssuedAt { get; set; }            // Token creation time
    public DateTime ExpiresAt { get; set; }           // Token expiration time
}
```

**RefreshTokenRequestDto:**
```csharp
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }
}
```

---

### 5.2 Trip DTOs

**CreateTripDto:**
```csharp
public class CreateTripDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Seats { get; set; }
    public string TravelType { get; set; }           // e.g., "Adventure", "Leisure"
    public string? ImgUrl { get; set; }
    public List<TripDayDto>? TripDays { get; set; }
}
```

**TripResponseDto:**
```csharp
public class TripResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Seats { get; set; }
    public string TravelType { get; set; }
    public string ImgUrl { get; set; }
    public string Status { get; set; }               // "Planned", "Ongoing", "Completed", "Cancelled"
    public int HostId { get; set; }
    public string HostName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TripDayResponseDto> TripDays { get; set; } = new();
}
```

**TripDayDto / TripDayResponseDto:**
```csharp
public class TripDayDto
{
    public int DayNumber { get; set; }
    public string Description { get; set; }
    public string? Activity { get; set; }
    public DateTime Date { get; set; }
}
```

**UpdateTripDto:** Only title, description, location, budget updatable

---

### 5.3 Member & Request DTOs

**TripMemberResponseDto:**
```csharp
public class TripMemberResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }                 // "Host", "Member"
    public string Status { get; set; }               // "Active", "Left", "Removed"
    public DateTime JoinedAt { get; set; }
}
```

**SendJoinRequestDto:**
```csharp
public class SendJoinRequestDto
{
    public int TripId { get; set; }
}
```

**JoinRequestResponseDto:**
```csharp
public class JoinRequestResponseDto
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Status { get; set; }               // "Pending", "Accepted", "Rejected", "Cancelled"
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### 5.4 Expense & Settlement DTOs

**CreateExpenseDto:**
```csharp
public class CreateExpenseDto
{
    public int TripId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public List<ExpenseSplitItemDto> Splits { get; set; }  // Who to split with
}

public class ExpenseSplitItemDto
{
    public int UserId { get; set; }
    public decimal AmountOwed { get; set; }
}
```

**ExpenseResponseDto:**
```csharp
public class ExpenseResponseDto
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int PaidBy { get; set; }
    public string PaidByName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ExpenseSplitResponseDto> Splits { get; set; }
}

public class ExpenseSplitResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public decimal AmountOwed { get; set; }
    public bool IsSettled { get; set; }
}
```

**ExpenseSummaryDto:**
```csharp
public class ExpenseSummaryDto
{
    public decimal TotalExpenses { get; set; }
    public List<UserOwesDto> UserBalances { get; set; }
}

public class UserOwesDto
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public decimal AmountOwed { get; set; }
}
```

---

### 5.5 Chat & Rating DTOs

**SendMessageDto:**
```csharp
public class SendMessageDto
{
    public int TripId { get; set; }
    public string Message { get; set; }
}
```

**ChatMessageResponseDto:**
```csharp
public class ChatMessageResponseDto
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**CreateRatingDto:**
```csharp
public class CreateRatingDto
{
    public int TripId { get; set; }
    public int RatedUserId { get; set; }
    public double Rating { get; set; }               // 1.0 - 5.0
    public string Review { get; set; }
}
```

**RatingResponseDto:**
```csharp
public class RatingResponseDto
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int RatedBy { get; set; }
    public string RatedByName { get; set; }
    public int RatedUserId { get; set; }
    public string RatedUserName { get; set; }
    public double Rating { get; set; }
    public string Review { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### 5.6 Image DTOs

**ImageUploadResponseDto:**
```csharp
public class ImageUploadResponseDto
{
    public string FileId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string FileName { get; set; } = string.Empty;
}
```

---

## 6. AutoMapper Profiles

### 6.1 UserProfile

**Mappings:**
- `User → UserResponseDto`: Direct property mapping (excludes PasswordHash, PasswordSalt, RefreshToken)
- `CreateUserDto → User`: Maps input fields, sets defaults (PhoneVerified=false, IdVerified=false, Rating=0.0, CreatedAt=now)
- `UpdateUserDto → User`: Conditional mapping for Name, Phone (only if non-empty)

### 6.2 TripProfile

**Mappings:**
- `Trip → TripResponseDto`: Maps all properties, includes HostName lookup from User
- `CreateTripDto → Trip`: Maps to entity, consumer sets HostId and Status
- `TripDayDto → TripDay`: Maps itinerary items

### 6.3 TripRequestProfile

**Mappings:**
- `TripRequest → JoinRequestResponseDto`: Maps status, includes UserName lookup

### 6.4 ExpenseProfile

**Mappings:**
- `Expense → ExpenseResponseDto`: Maps core fields, consumer includes Splits and PaidByName
- `ExpenseSplit → ExpenseSplitResponseDto`: Maps UserId, AmountOwed, IsSettled

### 6.5 ChatMessageProfile

**Mappings:**
- `ChatMessage → ChatMessageResponseDto`: Maps message content, includes SenderName lookup

### 6.6 TripRatingProfile

**Mappings:**
- `TripRating → RatingResponseDto`: Maps rating/review, includes RatedByName and RatedUserName lookups

### 6.7 ImageMappingProfile

**Mappings:**
- Minimal entity mappings, mostly DTO transformation in service layer

---

## 7. Configuration Models

### 7.1 JwtSettings

```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }           // ≥32 chars for HS256
    public string Issuer { get; set; }              // e.g., "TripConnectAPI"
    public string Audience { get; set; }            // e.g., "TripConnectApp"
    public int ExpirationMinutes { get; set; }      // Access token lifetime (default 60)
    public int RefreshTokenExpirationDays { get; set; } // Refresh token lifetime (default 7)
}
```

**Binding:** `services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"))`

---

### 7.2 EmailSettings

```csharp
public class EmailSettings
{
    public string SmtpHost { get; set; }            // e.g., "smtp.gmail.com"
    public int SmtpPort { get; set; }               // 25, 465 (SSL), 587 (TLS)
    public string SenderEmail { get; set; }         // Authorized SMTP user
    public string SenderPassword { get; set; }      // SMTP password or app password
    public string SenderName { get; set; }          // Display name
}
```

---

### 7.3 TwilioSettings

```csharp
public class TwilioSettings
{
    public string AccountSid { get; set; }          // Twilio account identifier
    public string AuthToken { get; set; }           // Twilio auth token
    public string VerifyServiceSid { get; set; }    // Verify service for OTP
}
```

---

## 8. Dependency Injection Setup

**File:** `DependencyInjection.cs`

```csharp
public static IServiceCollection AddApplicationLayer(
    this IServiceCollection services, IConfiguration configuration)
{
    // Logging
    var logger = LoggingConfiguration.ConfigureLogger();
    Log.Logger = logger;
    services.AddSingleton<Serilog.ILogger>(logger);

    // Configuration Binding
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
    services.Configure<TwilioSettings>(configuration.GetSection("TwilioSettings"));

    // Service Registration (Scoped lifetime)
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITripService, TripService>();
    services.AddScoped<IJoinRequestService, JoinRequestService>();
    services.AddScoped<IExpenseService, ExpenseService>();
    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<IRatingService, RatingService>();
    services.AddScoped<IImageService, ImageService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<ISmsService, SmsService>();

    // Token Services (critical for auth)
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IRefreshTokenService, RefreshTokenService>();

    // AutoMapper (7 profiles)
    services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<UserProfile>();
        cfg.AddProfile<TripProfile>();
        cfg.AddProfile<TripRequestProfile>();
        cfg.AddProfile<ExpenseProfile>();
        cfg.AddProfile<ChatMessageProfile>();
        cfg.AddProfile<TripRatingProfile>();
        cfg.AddProfile<ImageMappingProfile>();
    });

    return services;
}
```

**Integration in API_LAYER Program.cs:**
```csharp
builder.Services.AddApplicationLayer(builder.Configuration);
```

---

## 9. Business Logic Patterns

### 9.1 Cache-Aside Pattern

Services implement cache-aside logic via ICacheService:

```csharp
// Pattern 1: GetOrSetAsync (check → set if miss)
var cacheKey = string.Format(CacheKeyConstants.USER_BY_ID, userId);
var user = await _cacheService.GetOrSetAsync(
    cacheKey,
    async () => await _unitOfWork.Users.GetByIdAsync(userId),
    TimeSpan.FromHours(2)
);

// Pattern 2: Explicit Get + Set (for tag management)
var cached = await _cacheService.GetAsync<User>(cacheKey);
if (cached == null)
{
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    await _cacheService.SetAsync(
        cacheKey,
        user,
        TimeSpan.FromHours(2),
        new[] { $"user:{userId}", "user:all" }
    );
}
```

**TTL Strategy:**
- User profile: 2 hours
- Trip details: 1 hour
- Upcoming trips: 30 minutes (volatile)
- Search results: 2 hours

---

### 9.2 Tag-Based Invalidation

Services invalidate entire categories on mutations:

```csharp
// On CreateTrip
await _cacheService.InvalidateByTagAsync("trip:all");
await _cacheService.InvalidateByTagAsync($"trip:user:{userId}");

// On UpdateExpense
await _cacheService.InvalidateByTagAsync($"trip:{tripId}:expenses");
```

**Tag Naming Convention:**
- `entity:identifier` (e.g., `user:123`, `trip:456`)
- `entity:all` (all records of entity)
- `entity:subtype` (e.g., `trip:upcoming`, `trip:list`)
- `user:subresource` (e.g., `user:123:expenses`, `trip:456:chat`)

---

### 9.3 Transactional Consistency

Services use IUnitOfWork for multi-step operations:

```csharp
// Single transaction for related entities
await _unitOfWork.Expenses.AddAsync(expense);
await _unitOfWork.ExpenseSplits.AddAsync(splits);
await _unitOfWork.SaveChangesAsync();  // Commits both to database

// Invalidate cache after transaction succeeds
await _cacheService.InvalidateByTagAsync($"trip:{tripId}:expenses");
```

---

### 9.4 Password Handling

UserService implements secure password management:

```csharp
// Registration
var passwordSalt = GenerateSalt();  // Random bytes
var passwordHash = HashPassword(dto.Password, passwordSalt);
user.PasswordHash = passwordHash;
user.PasswordSalt = passwordSalt;

// Login (verify without storing plaintext)
if (!VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
    throw new InvalidOperationException("Invalid credentials");
```

**Algorithm:** PBKDF2 (via Rfc2898DeriveBytes in infrastructure)

---

### 9.5 Token Rotation

RefreshTokenService implements secure token refresh:

```csharp
// On refresh request
var newAccessToken = _tokenService.GenerateAccessToken(user);
var newRefreshToken = _tokenService.GenerateRefreshToken();

// Optional: Rotate refresh token (revoke old, store new)
user.RefreshToken = newRefreshToken;
user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
await _unitOfWork.Users.UpdateAsync(user);
await _unitOfWork.SaveChangesAsync();

return newAccessToken;
```

---

## 10. Error Handling

**Exception Strategy:**
1. Services validate input and business rules
2. Throw specific exceptions with contextual messages
3. Infrastructure (IUnitOfWork) propagates database errors
4. API layer catches exceptions and maps to HTTP responses

**Common Exceptions:**
- `InvalidOperationException` — Business logic violation (user not found, duplicate request, etc.)
- `ArgumentException` — Invalid input parameters
- `SecurityTokenException` — Invalid JWT/refresh token
- `SmtpException` (MailKit) — Email service failures (logged, not always re-thrown)

**Logging Pattern:**
```csharp
try
{
    _logger.Information($"Creating trip for user: {userId}");
    var trip = _mapper.Map<Trip>(dto);
    await _unitOfWork.Trips.AddAsync(trip);
    await _unitOfWork.SaveChangesAsync();
    _logger.Information($"Trip created: {trip.Id}");
    return _mapper.Map<TripResponseDto>(trip);
}
catch (Exception ex)
{
    _logger.Error($"Error creating trip: {ex.Message}");
    throw;  // Re-throw for API layer handling
}
```

---

## 11. Caching Integration

**Cache Keys Namespace:**
Defined in `DOMAIN_LAYER.Repository.CacheKeyConstants`:
- `USER_BY_ID = "user:byid:{0}"` (userId)
- `TRIP_BY_ID = "trip:byid:{0}"` (tripId)
- `EXPENSE_BY_ID = "expense:byid:{0}"` (expenseId)
- etc.

**Cache Failures:**
- ICacheService.SetAsync swallows write failures (logs warning, doesn't fail request)
- If Redis unavailable, cache misses silently, database queries proceed
- Ensures graceful degradation

---

## 12. Design Notes

**Dependency Inversion:**
- Services depend on IUserRepository, not UserRepository
- All repositories injected via IUnitOfWork
- Enables testing with mock repositories

**Separation of Concerns:**
- Services handle orchestration and business logic
- DTOs encapsulate contract between API and application layers
- AutoMapper handles entity ↔ DTO transformations
- Configuration models isolate external service credentials

**Scalability Considerations:**
- Cache-aside pattern reduces database load
- Tag-based invalidation enables partial cache clears
- Pagination in list endpoints prevents memory overload
- TTLs tuned per data volatility (30 min for volatile, 2 hours for stable)

---

## 13. External Service Integration Summary

| Service | Purpose | Configuration | Failure Handling |
|---------|---------|---------------|------------------|
| ImageKit | Image uploads/deletes | ImageKit:PublicKey, PrivateKey, UrlEndpoint | Exceptions logged, returned in response |
| Twilio | SMS OTP | TwilioSettings:AccountSid, AuthToken, VerifyServiceSid | Task exceptions thrown to API |
| Gmail SMTP | Email verification | EmailSettings:SmtpHost/Port/Auth | Exceptions logged, may be non-critical |
| Redis | Caching | Infrastructure registered as singleton | Transparent fallback to DB |
| SQL Server | Persistence | EF Core DbContext | Transactional consistency |

---

## 14. Summary

The **Application Layer** provides a clean, well-separated business logic tier that:

1. **Orchestrates domain logic** via services using repository interfaces
2. **Transforms entities to DTOs** via AutoMapper for API consumption
3. **Manages authentication** through JWT token generation and refresh workflows
4. **Integrates external services** (email, SMS, image hosting) with error resilience
5. **Implements caching** via tag-based invalidation for performance
6. **Ensures transactional consistency** through UnitOfWork pattern
7. **Provides comprehensive logging** for debugging and monitoring
8. **Validates business rules** before persisting changes

All 11 services are scoped (per-request), enabling clean DI and testability. The 7 AutoMapper profiles provide consistent entity-to-DTO transformations. Configuration models bind external credentials securely. Error handling with meaningful exception messages ensures API layer can generate appropriate HTTP responses.

