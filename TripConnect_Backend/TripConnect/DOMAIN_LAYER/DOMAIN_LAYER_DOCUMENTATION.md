# DOMAIN_LAYER — Detailed Documentation

> **Analyzed from source code on May 8, 2026.**  
> All content reflects the actual implemented code.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Layer Principles](#2-layer-principles)
3. [Project Structure](#3-project-structure)
4. [Entities](#4-entities)
   - [User](#41-user)
   - [Trip](#42-trip)
   - [TripDay](#43-tripday)
   - [TripMember](#44-tripmember)
   - [TripRequest](#45-triprequest)
   - [Expense](#46-expense)
   - [ExpenseSplit](#47-expensesplit)
   - [ChatMessage](#48-chatmessage)
   - [TripRating](#49-triprating)
   - [CacheEntry](#410-cacheentry)
   - [CachePolicy](#411-cachepolicy)
5. [Entity Relationships](#5-entity-relationships)
6. [Enumerations](#6-enumerations)
7. [Repository Interfaces](#7-repository-interfaces)
   - [IRepository\<T\>](#71-irepositoryt--generic-base)
   - [IUserRepository](#72-iuserrepository)
   - [ITripRepository](#73-itriprepository)
   - [ITripRequestRepository](#74-itriprequestrepository)
   - [ITripMemberRepository](#75-itripmemberrepository)
   - [IExpenseRepository](#76-iexpenserepository)
   - [IExpenseSplitRepository](#77-iexpensesplitrepository)
   - [IChatRepository](#78-ichatrepository)
   - [ITripRatingRepository](#79-itripratingrepository)
   - [IUnitOfWork](#710-iunitofwork)
   - [Cache Repository Interfaces](#711-cache-repository-interfaces)
8. [Cache Key Constants](#8-cache-key-constants)
9. [Design Decisions](#9-design-decisions)

---

## 1. Overview

The **DOMAIN_LAYER** is the innermost layer of TripConnect's Clean Architecture. It defines what the system *is* — the core business concepts — without any dependency on frameworks, databases, or external services.

**Project file:** `DOMAIN_LAYER/DOMAIN_LAYER.csproj`  
**Target framework:** .NET 10  
**External NuGet dependencies:** None (zero)  
**Namespace root:** `DOMAIN_LAYER`

Everything else in the system (Application, Infrastructure, API) depends on this layer. This layer depends on nothing.

---

## 2. Layer Principles

| Principle | How it is applied |
|---|---|
| **No external dependencies** | `DOMAIN_LAYER.csproj` has no `<PackageReference>` entries |
| **Persistence ignorance** | Entities are plain C# classes — no EF Core attributes, no data annotations |
| **Interface segregation** | Each aggregate root has its own dedicated repository interface |
| **Dependency inversion** | Infrastructure implements the interfaces defined here; Application consumes them |
| **Single responsibility** | Entities hold data + navigation properties only; no business logic |

---

## 3. Project Structure

```
DOMAIN_LAYER/
├── DOMAIN_LAYER.csproj
│
├── Entity/
│   ├── User/
│   │   └── User.cs
│   ├── Trip/
│   │   ├── Trip.cs
│   │   └── Tripday.cs
│   ├── TripMember/
│   │   └── TripMember.cs
│   ├── TripRequest/
│   │   └── TripRequest.cs
│   ├── Expense/
│   │   └── Expense.cs
│   ├── ExpenseSplit/
│   │   └── ExpenseSplit.cs
│   ├── ChatMessage/
│   │   └── ChatMessage.cs
│   ├── TripRating/
│   │   └── TripRating.cs
│   └── Cache/
│       ├── CacheEntry.cs
│       └── CachePolicy.cs
│
├── Enum/
│   ├── TripStatus.cs
│   ├── TripMemberRole.cs
│   ├── TripMemberStatus.cs
│   ├── TripRequestStatus.cs
│   ├── CachePolicyType.cs
│   └── CacheKeyConstants.cs
│
└── Repository/
    ├── IRepository.cs
    ├── IUserRepository.cs
    ├── ITripRepository.cs
    ├── ITripRequestRepository.cs
    ├── ITripMemberRepository.cs
    ├── IExpenseRepository.cs
    ├── IExpenseSplitRepository.cs
    ├── IChatRepository.cs
    ├── ITripRatingRepository.cs
    ├── IUnitOfWork.cs
    ├── ICacheRepository.cs
    ├── ICacheEntryRepository.cs
    ├── ICacheInvalidationRepository.cs
    └── ICachePolicyRepository.cs
```

---

## 4. Entities

### 4.1 User

**Namespace:** `DOMAIN_LAYER.Entity.User`  
**File:** `Entity/User/User.cs`

Represents a registered user on the TripConnect platform. Users can host trips, join trips, chat, log expenses, and rate other members.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `Name` | `string` | Full display name |
| `Email` | `string` | Unique email address |
| `Username` | `string` | Unique username used for login |
| `Phone` | `string` | Phone number |
| `Rating` | `double` | Reputation score, range 0.0 – 5.0, updated after each trip rating |
| `PhoneVerified` | `bool` | Whether phone OTP verification was completed |
| `IdVerified` | `bool` | Whether identity document was verified |
| `EmailVerified` | `bool` | Whether email link verification was completed (default: `false`) |
| `PasswordHash` | `string` | HMAC-SHA256 hashed password |
| `PasswordSalt` | `string` | Unique salt used during hashing |
| `CreatedAt` | `DateTime` | Account creation timestamp |
| `LastLoginAt` | `DateTime?` | Timestamp of most recent login (nullable) |
| `RefreshToken` | `string?` | Current JWT refresh token (nullable, replaced on each login) |
| `RefreshTokenExpiryTime` | `DateTime?` | Expiry of the refresh token (nullable) |
| `IsTokenBlacklisted` | `bool` | Whether the current token has been invalidated (default: `false`) |
| `EmailVerificationToken` | `string?` | One-time token sent during email verification (cleared after use) |
| `EmailVerificationTokenExpiry` | `DateTime?` | Token expiry — 24 hours from issue |
| `PhoneOtpCode` | `string?` | Hashed OTP code for phone verification (cleared after success) |
| `PhoneOtpExpiry` | `DateTime?` | OTP expiry — 10 minutes from generation |
| `PhoneOtpRequestCount` | `int` | Number of OTP send attempts; max 3; resets on success (default: `0`) |

#### Navigation Properties

| Property | Type | Relationship |
|---|---|---|
| `CreatedTrips` | `ICollection<Trip>` | Trips hosted by this user (1:M) |
| `TripRequests` | `ICollection<TripRequest>` | Join requests sent by this user (1:M) |
| *(TripMembers, Expenses, ChatMessages, TripRatings)* | Various | Accessed via respective entities' FK |

---

### 4.2 Trip

**Namespace:** `DOMAIN_LAYER.Entity.Trip`  
**File:** `Entity/Trip/Trip.cs`

Represents a trip posting. A trip has one host, optional members, an itinerary (days), expenses, chat messages, and ratings.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `Title` | `string` | Short trip title |
| `Description` | `string` | Detailed trip description |
| `Location` | `string` | Destination/location |
| `Budget` | `decimal` | Estimated trip budget |
| `StartDate` | `DateTime` | Trip start date |
| `EndDate` | `DateTime` | Trip end date |
| `Seats` | `int` | Number of available seats |
| `TravelType` | `string` | Category: Trekking, Beach, Biking, etc. |
| `ImgUrl` | `string` | Cover image URL (default: Unsplash placeholder) |
| `Status` | `TripStatus` | Current lifecycle state (see enum) |
| `HostId` | `int` | FK → `User.Id` of the organizer |
| `CreatedAt` | `DateTime` | Timestamp when trip was posted |

#### Navigation Properties

| Property | Type | Relationship |
|---|---|---|
| `Host` | `User` | Trip organizer (M:1) |
| `TripDays` | `ICollection<TripDay>` | Day-by-day itinerary (1:M) |
| `TripRequests` | `ICollection<TripRequest>` | Join requests for this trip (1:M) |
| `TripMembers` | `ICollection<TripMember>` | Members of this trip (1:M) |
| `Expenses` | `ICollection<Expense>` | Expenses logged for this trip (1:M) |
| `ChatMessages` | `ICollection<ChatMessage>` | Chat messages in this trip (1:M) |
| `Ratings` | `ICollection<TripRating>` | Ratings given within this trip (1:M) |

---

### 4.3 TripDay

**Namespace:** `DOMAIN_LAYER.Entity.Trip`  
**File:** `Entity/Trip/Tripday.cs`

Represents a single day in a trip's itinerary.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `TripId` | `int` | FK → `Trip.Id` |
| `Day` | `int` | Day number in the itinerary (1, 2, 3…) |
| `Location` | `string?` | Location for this day (nullable) |
| `Date` | `DateOnly` | Calendar date for this day |
| `Description` | `string?` | Activities / notes for the day (nullable) |
| `ImgUrl` | `string?` | Optional image for this day (nullable) |
| `Trip` | `Trip` | Navigation → parent Trip (M:1) |

---

### 4.4 TripMember

**Namespace:** `DOMAIN_LAYER.Entity.TripMember`  
**File:** `Entity/TripMember/TripMember.cs`

Junction entity connecting a `User` to a `Trip`. Tracks the member's role and current status.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `TripId` | `int` | FK → `Trip.Id` |
| `UserId` | `int` | FK → `User.Id` |
| `Role` | `TripMemberRole` | Host or Member |
| `Status` | `TripMemberStatus` | Active, Left, or Removed |
| `JoinedAt` | `DateTime` | Timestamp when the member joined |
| `User` | `User` | Navigation → User (M:1) |
| `Trip` | `Trip` | Navigation → Trip (M:1) |

---

### 4.5 TripRequest

**Namespace:** `DOMAIN_LAYER.Entity.TripRequest`  
**File:** `Entity/TripRequest/TripRequest.cs`

Represents a user's application to join a trip. Must be accepted by the host before the user becomes a `TripMember`.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `TripId` | `int` | FK → `Trip.Id` |
| `UserId` | `int` | FK → `User.Id` of the applicant |
| `Status` | `TripRequestStatus` | Pending / Accepted / Rejected / Cancelled |
| `RequestedAt` | `DateTime` | When the request was submitted |
| `RespondedAt` | `DateTime?` | When the host responded (nullable until responded) |
| `User` | `User` | Navigation → requesting User (M:1) |
| `Trip` | `Trip` | Navigation → target Trip (M:1) |

---

### 4.6 Expense

**Namespace:** `DOMAIN_LAYER.Entity.Expense`  
**File:** `Entity/Expense/Expense.cs`

Represents a single expense logged during a trip. The expense is paid by one user and split among all active trip members via `ExpenseSplit` records.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `TripId` | `int` | FK → `Trip.Id` |
| `PaidBy` | `int` | FK → `User.Id` of the payer |
| `Amount` | `decimal` | Total expense amount |
| `Description` | `string` | Category or note (Hotel, Fuel, Food, etc.) |
| `CreatedAt` | `DateTime` | When the expense was logged |
| `Trip` | `Trip` | Navigation → parent Trip (M:1) |
| `PaidByUser` | `User` | Navigation → payer User (M:1) |
| `ExpenseSplits` | `ICollection<ExpenseSplit>` | Per-member splits (1:M) |

---

### 4.7 ExpenseSplit

**Namespace:** `DOMAIN_LAYER.Entity.ExpenseSplit`  
**File:** `Entity/ExpenseSplit/ExpenseSplit.cs`

Represents one member's share of an expense. Created automatically when an expense is logged (Application Layer divides `Amount / ActiveMemberCount`).

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `ExpenseId` | `int` | FK → `Expense.Id` |
| `UserId` | `int` | FK → `User.Id` of the member who owes |
| `AmountOwed` | `decimal` | Amount this member owes |
| `IsSettled` | `bool` | Whether this share has been paid back |
| `Expense` | `Expense` | Navigation → parent Expense (M:1) |
| `User` | `User` | Navigation → owing User (M:1) |

---

### 4.8 ChatMessage

**Namespace:** `DOMAIN_LAYER.Entity.ChatMessage`  
**File:** `Entity/ChatMessage/ChatMessage.cs`

Represents a single message in a trip's group chat. All members of a trip can send and view messages.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `TripId` | `int` | FK → `Trip.Id` |
| `SenderId` | `int` | FK → `User.Id` of the sender |
| `Message` | `string` | Message body text |
| `CreatedAt` | `DateTime` | Message timestamp |
| `Trip` | `Trip` | Navigation → parent Trip (M:1) |
| `Sender` | `User` | Navigation → sender User (M:1) |

---

### 4.9 TripRating

**Namespace:** `DOMAIN_LAYER.Entity.TripRating`  
**File:** `Entity/TripRating/TripRating.cs`

Represents a user-to-user rating given after a shared trip. A rater can rate any other member who was in the same trip (including the host).

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `TripId` | `int` | FK → `Trip.Id` the rating is associated with |
| `RatedBy` | `int` | FK → `User.Id` of the person giving the rating |
| `RatedUserId` | `int` | FK → `User.Id` of the person being rated |
| `Rating` | `double` | Score — range 1.0 to 5.0 |
| `Review` | `string` | Optional text review |
| `CreatedAt` | `DateTime` | When the rating was submitted |
| `Trip` | `Trip` | Navigation → Trip (M:1) |
| `RatedByUser` | `User` | Navigation → rater User (M:1) |
| `RatedUser` | `User` | Navigation → rated User (M:1) |

---

### 4.10 CacheEntry

**Namespace:** `DOMAIN_LAYER.Entity.Cache`  
**File:** `Entity/Cache/CacheEntry.cs`

Tracks metadata for individual cached items. Used by the cache management system to monitor statistics, enforce expiry, and support cleanup.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `CacheKey` | `string` | The Redis key |
| `CachedData` | `string` | JSON-serialized cached value |
| `DataType` | `string` | CLR type name of the cached object |
| `ExpiresAt` | `DateTime?` | Expiry timestamp (null = no expiry) |
| `CachedAt` | `DateTime` | When entry was cached (default: `UtcNow`) |
| `AccessCount` | `long` | Total times this entry was read (default: `0`) |
| `LastAccessedAt` | `DateTime?` | Last read timestamp |
| `CachePolicyId` | `int?` | FK → `CachePolicy.Id` (nullable) |
| `CachePolicy` | `CachePolicy` | Navigation → policy (M:1, nullable) |

#### Computed / Methods

| Member | Description |
|---|---|
| `IsExpired` | Computed property — `true` if `ExpiresAt` is set and has passed |
| `UpdateAccessInfo()` | Increments `AccessCount` and sets `LastAccessedAt = UtcNow` |

---

### 4.11 CachePolicy

**Namespace:** `DOMAIN_LAYER.Entity.Cache`  
**File:** `Entity/Cache/CachePolicy.cs`

Defines a reusable cache expiration strategy that can be assigned to one or more `CacheEntry` records.

#### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key |
| `PolicyName` | `string` | Descriptive name (e.g., "TripList-1h") |
| `PolicyType` | `CachePolicyType` | Absolute / Sliding / NoExpiration / EventBased |
| `AbsoluteExpirationSeconds` | `int?` | Used when `PolicyType = Absolute` |
| `SlidingExpirationSeconds` | `int?` | Used when `PolicyType = Sliding` |
| `IsActive` | `bool` | Whether this policy is currently in use (default: `true`) |
| `CreatedAt` | `DateTime` | Creation timestamp |
| `ModifiedAt` | `DateTime?` | Last modification timestamp |

#### Methods

| Method | Returns | Description |
|---|---|---|
| `GetExpirationTimeSpan()` | `TimeSpan?` | Calculates the `TimeSpan` based on `PolicyType` and the configured seconds fields |

---

## 5. Entity Relationships

```
User
 ├── (1:M) Trip               [as Host via Trip.HostId]
 ├── (1:M) TripMember         [UserId]
 ├── (1:M) TripRequest        [UserId]
 ├── (1:M) ChatMessage        [SenderId]
 ├── (1:M) Expense            [PaidBy]
 ├── (1:M) ExpenseSplit       [UserId]
 ├── (1:M) TripRating         [RatedBy  — as rater]
 └── (1:M) TripRating         [RatedUserId — as rated]

Trip
 ├── (M:1) User               [HostId]
 ├── (1:M) TripDay            [TripId]
 ├── (1:M) TripMember         [TripId]
 ├── (1:M) TripRequest        [TripId]
 ├── (1:M) Expense            [TripId]
 ├── (1:M) ChatMessage        [TripId]
 └── (1:M) TripRating         [TripId]

Expense
 ├── (M:1) Trip               [TripId]
 ├── (M:1) User               [PaidBy]
 └── (1:M) ExpenseSplit       [ExpenseId]

ExpenseSplit
 ├── (M:1) Expense            [ExpenseId]
 └── (M:1) User               [UserId]

TripMember
 ├── (M:1) Trip               [TripId]
 └── (M:1) User               [UserId]

TripRequest
 ├── (M:1) Trip               [TripId]
 └── (M:1) User               [UserId]

ChatMessage
 ├── (M:1) Trip               [TripId]
 └── (M:1) User               [SenderId]

TripRating
 ├── (M:1) Trip               [TripId]
 ├── (M:1) User               [RatedBy]
 └── (M:1) User               [RatedUserId]

TripDay
 └── (M:1) Trip               [TripId]

CacheEntry
 └── (M:1) CachePolicy        [CachePolicyId — nullable]
```

---

## 6. Enumerations

All enums are in the `DOMAIN_LAYER.Enum` namespace.

### TripStatus

Tracks the lifecycle of a `Trip`.

| Value | Int | Description |
|---|---|---|
| `Planned` | 1 | Trip is created and upcoming |
| `Ongoing` | 2 | Trip is currently happening |
| `Completed` | 3 | Trip has ended successfully |
| `Cancelled` | 4 | Trip was cancelled by the host |

---

### TripMemberRole

Defines a member's authority level within a trip.

| Value | Int | Description |
|---|---|---|
| `Host` | 1 | Trip organizer — can accept/reject requests, manage members |
| `Member` | 2 | Regular participant — can chat, log expenses, rate others |

---

### TripMemberStatus

Tracks whether a member is currently active in a trip.

| Value | Int | Description |
|---|---|---|
| `Active` | 1 | Currently participating in the trip |
| `Left` | 2 | Voluntarily withdrew from the trip |
| `Removed` | 3 | Removed by the host |

---

### TripRequestStatus

Lifecycle states for a join request.

| Value | Int | Description |
|---|---|---|
| `Pending` | 1 | Awaiting host response |
| `Accepted` | 2 | Host approved — user becomes a TripMember |
| `Rejected` | 3 | Host declined |
| `Cancelled` | 4 | User withdrew the request before a response |

---

### CachePolicyType

Defines expiration behavior for a `CachePolicy`.

| Value | Int | Description |
|---|---|---|
| `Absolute` | 0 | Entry expires at a fixed point in time |
| `Sliding` | 1 | Expiry resets each time the entry is accessed |
| `NoExpiration` | 2 | Entry lives indefinitely until manually removed |
| `EventBased` | 3 | Entry is invalidated by a specific application event |

---

## 7. Repository Interfaces

All interfaces live in the `DOMAIN_LAYER.Repository` namespace. They are **contracts only** — implementations reside in `INFRASTRUCTURE_LAYER`.

---

### 7.1 `IRepository<T>` — Generic Base

The base interface extended by all domain-specific repositories.

```
IRepository<T> where T : class
```

| Method | Return | Description |
|---|---|---|
| `GetByIdAsync(int id)` | `Task<T>` | Fetch a single entity by its PK |
| `GetAllAsync()` | `Task<IEnumerable<T>>` | Fetch all entities |
| `AddAsync(T entity)` | `Task` | Insert a new entity |
| `UpdateAsync(T entity)` | `Task` | Update an existing entity |
| `DeleteAsync(int id)` | `Task` | Remove entity by PK |
| `SaveChangesAsync()` | `Task` | Persist pending changes to the database |
| `ExistsAsync(int id)` | `Task<bool>` | Check if an entity with this PK exists |

---

### 7.2 `IUserRepository`

Extends `IRepository<User>`. User-specific query methods.

| Method | Return | Description |
|---|---|---|
| `GetByEmailAsync(string email)` | `Task<User>` | Lookup by email |
| `GetByPhoneAsync(string phone)` | `Task<User>` | Lookup by phone number |
| `GetByUsernameAsync(string username)` | `Task<User>` | Lookup by username |
| `GetUserForAuthenticationAsync(string username)` | `Task<User>` | Includes password fields for login |
| `GetByRefreshTokenAsync(string refreshToken)` | `Task<User>` | Used during token refresh flow |
| `GetByEmailVerificationTokenAsync(string token)` | `Task<User?>` | Email verification link lookup |
| `GetVerifiedUsersAsync()` | `Task<IEnumerable<User>>` | All users with both verifications complete |
| `EmailExistsAsync(string email)` | `Task<bool>` | Uniqueness check before registration |
| `PhoneExistsAsync(string phone)` | `Task<bool>` | Uniqueness check |
| `UsernameExistsAsync(string username)` | `Task<bool>` | Uniqueness check |
| `UpdateVerificationStatusAsync(int userId, bool phone, bool id)` | `Task` | Update phone/ID verified flags |
| `UpdateRatingAsync(int userId, double rating)` | `Task` | Persist new average rating |
| `UpdatePasswordAsync(int userId, string hash, string salt)` | `Task` | Password change |

---

### 7.3 `ITripRepository`

Extends `IRepository<Trip>`. Trip-specific query and search methods.

| Method | Return | Description |
|---|---|---|
| `GetTripsByStatusAsync(TripStatus status)` | `Task<IEnumerable<Trip>>` | Filter by lifecycle status |
| `GetTripsByLocationAsync(string location)` | `Task<IEnumerable<Trip>>` | Location-based search |
| `GetTripsByHostAsync(int hostId)` | `Task<IEnumerable<Trip>>` | Trips organized by a user |
| `GetUpcomingTripsAsync()` | `Task<IEnumerable<Trip>>` | Future trips not yet started |
| `GetTripsByBudgetAsync(decimal min, decimal max)` | `Task<IEnumerable<Trip>>` | Budget range filter |
| `GetTripsByTravelTypeAsync(string travelType)` | `Task<IEnumerable<Trip>>` | Filter by travel category |
| `SearchTripsAsync(string location, DateTime? startDate, decimal? maxBudget, string travelType)` | `Task<IEnumerable<Trip>>` | Multi-criteria search |
| `GetTripWithDetailsAsync(int tripId)` | `Task<Trip>` | Eager-loads members, requests, expenses |

---

### 7.4 `ITripRequestRepository`

Extends `IRepository<TripRequest>`. Join request lifecycle management.

| Method | Return | Description |
|---|---|---|
| `GetRequestsByTripAsync(int tripId)` | `Task<IEnumerable<TripRequest>>` | All requests for a trip |
| `GetRequestsByUserAsync(int userId)` | `Task<IEnumerable<TripRequest>>` | All requests by a user |
| `GetRequestsByStatusAsync(TripRequestStatus status)` | `Task<IEnumerable<TripRequest>>` | Filter by status |
| `GetPendingRequestsByTripAsync(int tripId)` | `Task<IEnumerable<TripRequest>>` | Only pending requests |
| `HasUserRequestedAsync(int userId, int tripId)` | `Task<bool>` | Prevent duplicate requests |
| `GetRequestByUserAndTripAsync(int userId, int tripId)` | `Task<TripRequest>` | Specific request lookup |
| `UpdateRequestStatusAsync(int requestId, TripRequestStatus status)` | `Task` | Accept / Reject / Cancel |

---

### 7.5 `ITripMemberRepository`

Extends `IRepository<TripMember>`. Trip membership queries.

| Method | Return | Description |
|---|---|---|
| `GetMembersByTripAsync(int tripId)` | `Task<IEnumerable<TripMember>>` | All members of a trip |
| `GetMembershipsByUserAsync(int userId)` | `Task<IEnumerable<TripMember>>` | All trips a user belongs to |
| `GetMembersByRoleAsync(int tripId, TripMemberRole role)` | `Task<IEnumerable<TripMember>>` | Filter by Host/Member |
| `GetActiveMembersAsync(int tripId)` | `Task<IEnumerable<TripMember>>` | Only `Status = Active` members |
| `GetMemberAsync(int userId, int tripId)` | `Task<TripMember>` | Specific member record |
| `IsMemberAsync(int userId, int tripId)` | `Task<bool>` | Membership check |
| `GetMemberCountAsync(int tripId)` | `Task<int>` | Active member count (for expense splitting) |
| `UpdateMemberStatusAsync(int memberId, TripMemberStatus status)` | `Task` | Leave / Remove member |

---

### 7.6 `IExpenseRepository`

Extends `IRepository<Expense>`. Expense tracking and totals.

| Method | Return | Description |
|---|---|---|
| `GetExpensesByTripAsync(int tripId)` | `Task<IEnumerable<Expense>>` | All expenses for a trip |
| `GetExpensesByUserAsync(int userId)` | `Task<IEnumerable<Expense>>` | Expenses paid by a user |
| `GetTotalExpensesByTripAsync(int tripId)` | `Task<decimal>` | Sum of all expense amounts for a trip |
| `GetTotalPaidByUserInTripAsync(int userId, int tripId)` | `Task<decimal>` | Amount a specific user paid in a trip |
| `GetExpensesByDescriptionAsync(int tripId, string description)` | `Task<IEnumerable<Expense>>` | Filter by category |
| `GetExpenseWithSplitsAsync(int expenseId)` | `Task<Expense>` | Eager-loads splits |
| `DeleteExpenseWithSplitsAsync(int expenseId)` | `Task` | Cascades deletion to splits |

---

### 7.7 `IExpenseSplitRepository`

Extends `IRepository<ExpenseSplit>`. Settlement tracking per member.

| Method | Return | Description |
|---|---|---|
| `GetSplitsByExpenseAsync(int expenseId)` | `Task<IEnumerable<ExpenseSplit>>` | All splits for one expense |
| `GetOwedSplitsByUserAsync(int userId, int tripId)` | `Task<IEnumerable<ExpenseSplit>>` | What a user owes in a trip |
| `GetUnsettledSplitsByUserAsync(int userId, int tripId)` | `Task<IEnumerable<ExpenseSplit>>` | Only unpaid splits |
| `GetAllUnsettledSplitsAsync(int tripId)` | `Task<IEnumerable<ExpenseSplit>>` | All unsettled splits in a trip |
| `GetTotalOwedByUserAsync(int userId, int tripId)` | `Task<decimal>` | Total amount a user still owes |
| `MarkAsSettledAsync(int splitId)` | `Task` | Mark one split as paid |
| `IsSettledAsync(int splitId)` | `Task<bool>` | Check settlement status |

---

### 7.8 `IChatRepository`

Extends `IRepository<ChatMessage>`. Chat history and search.

| Method | Return | Description |
|---|---|---|
| `GetMessagesByTripAsync(int tripId)` | `Task<IEnumerable<ChatMessage>>` | All messages in a trip |
| `GetMessagesBySenderAsync(int userId)` | `Task<IEnumerable<ChatMessage>>` | Messages sent by a user |
| `GetMessagesByTripPaginatedAsync(int tripId, int page, int size)` | `Task<IEnumerable<ChatMessage>>` | Paginated history |
| `GetLatestMessagesAsync(int tripId, int count)` | `Task<IEnumerable<ChatMessage>>` | Most recent N messages |
| `GetMessagesFromAsync(int tripId, DateTime dateTime)` | `Task<IEnumerable<ChatMessage>>` | Messages after a timestamp (for polling) |
| `SearchMessagesAsync(int tripId, string searchText)` | `Task<IEnumerable<ChatMessage>>` | Full-text search within a trip |
| `GetMessageCountAsync(int tripId)` | `Task<int>` | Total message count |
| `DeleteTripMessagesAsync(int tripId)` | `Task` | Bulk delete on trip deletion |

---

### 7.9 `ITripRatingRepository`

Extends `IRepository<TripRating>`. Rating queries and averages.

| Method | Return | Description |
|---|---|---|
| `GetRatingsByTripAsync(int tripId)` | `Task<IEnumerable<TripRating>>` | All ratings in a trip |
| `GetRatingsGivenByUserAsync(int userId)` | `Task<IEnumerable<TripRating>>` | Ratings a user gave |
| `GetRatingsReceivedByUserAsync(int userId)` | `Task<IEnumerable<TripRating>>` | Ratings a user received |
| `GetRatingsWithReviewAsync(int userId)` | `Task<IEnumerable<TripRating>>` | Only ratings with text review |
| `GetRatingByUserAsync(int tripId, int ratedBy, int ratedUserId)` | `Task<TripRating>` | Specific rating lookup |
| `HasUserRatedAsync(int tripId, int ratedBy, int ratedUserId)` | `Task<bool>` | Prevent duplicate ratings |
| `GetTripRatingsForUserAsync(int tripId, int userId)` | `Task<IEnumerable<TripRating>>` | Ratings for a user in one trip |
| `GetAverageRatingForUserAsync(int userId)` | `Task<double>` | User reputation score |
| `GetAverageRatingForTripAsync(int tripId)` | `Task<double>` | Average score across a trip |
| `GetRatingsCountAsync(int userId)` | `Task<int>` | Total ratings received |

---

### 7.10 `IUnitOfWork`

Implements the **Unit of Work** pattern. Aggregates all repositories under one transaction scope. Implements `IDisposable`.

```csharp
public interface IUnitOfWork : IDisposable
```

#### Repository Properties

| Property | Type |
|---|---|
| `Users` | `IUserRepository` |
| `Trips` | `ITripRepository` |
| `TripRequests` | `ITripRequestRepository` |
| `TripMembers` | `ITripMemberRepository` |
| `Expenses` | `IExpenseRepository` |
| `ExpenseSplits` | `IExpenseSplitRepository` |
| `ChatMessages` | `IChatRepository` |
| `TripRatings` | `ITripRatingRepository` |

#### Methods

| Method | Return | Description |
|---|---|---|
| `SaveChangesAsync()` | `Task<int>` | Commits all pending changes in the current DbContext transaction |

**Usage pattern in Application Layer:**

```csharp
// Multiple operations, one commit
await _unitOfWork.TripMembers.AddAsync(member);
await _unitOfWork.TripRequests.UpdateRequestStatusAsync(requestId, Accepted);
await _unitOfWork.SaveChangesAsync();
```

---

### 7.11 Cache Repository Interfaces

#### `ICacheRepository`

Core get/set/delete operations against Redis.

| Method | Return | Description |
|---|---|---|
| `GetAsync<T>(string key)` | `Task<T?>` | Read and deserialize a cached value |
| `SetAsync<T>(string key, T value, TimeSpan? expiration)` | `Task` | Serialize and store with optional TTL |
| `RemoveAsync(string key)` | `Task` | Delete a single key |
| `ContainsKeyAsync(string key)` | `Task<bool>` | Check key existence |
| `RemoveByPatternAsync(string pattern)` | `Task<int>` | Wildcard-based bulk delete; returns count removed |
| `ClearAllAsync()` | `Task` | Flush all cached data |

#### `ICacheEntryRepository`

Extends `IRepository<CacheEntry>`. DB-backed cache metadata management.

| Method | Return | Description |
|---|---|---|
| `GetByCacheKeyAsync(string cacheKey)` | `Task<CacheEntry>` | Fetch metadata by Redis key |
| `GetExpiredEntriesAsync()` | `Task<IEnumerable<CacheEntry>>` | Entries past their `ExpiresAt` |
| `GetByDataTypeAsync(string dataType)` | `Task<IEnumerable<CacheEntry>>` | Filter by cached CLR type |
| `DeleteExpiredEntriesAsync()` | `Task<int>` | Cleanup job — returns count deleted |
| `DeleteByPatternAsync(string pattern)` | `Task<int>` | Pattern-based bulk delete |
| `ClearAllAsync()` | `Task<int>` | Remove all entries |
| `GetCacheStatisticsAsync()` | `Task<CacheStatistics>` | Returns aggregate stats (total, expired, active, access counts, average age) |
| `UpdateAccessInfoAsync(int cacheEntryId)` | `Task` | Increments access count and updates last-accessed time |

#### `ICacheInvalidationRepository`

Tag-based cache invalidation coordination.

| Method | Return | Description |
|---|---|---|
| `InvalidateByPatternAsync(string pattern)` | `Task<int>` | Remove keys matching a Redis key pattern |
| `InvalidateByTagAsync(string tag)` | `Task<int>` | Remove all keys associated with a tag |
| `InvalidateByTagsAsync(params string[] tags)` | `Task<int>` | Bulk tag invalidation |
| `AddTagToCacheAsync(string cacheKey, string tag)` | `Task` | Associate a tag with a key |
| `AddTagsToCacheAsync(string cacheKey, params string[] tags)` | `Task` | Associate multiple tags |
| `GetTagsAsync(string cacheKey)` | `Task<IEnumerable<string>>` | Get all tags on a key |
| `GetCacheKeysByTagAsync(string tag)` | `Task<IEnumerable<string>>` | Find all keys with a given tag |
| `RegisterInvalidationAsync(string pattern, string dependencyPattern)` | `Task` | Register cascade invalidation rule |
| `GetDependentPatternsAsync(string pattern)` | `Task<IEnumerable<string>>` | Retrieve registered dependency patterns |

#### `ICachePolicyRepository`

Extends `IRepository<CachePolicy>`. Manage reusable expiry policies.

| Method | Return | Description |
|---|---|---|
| `GetByPolicyNameAsync(string policyName)` | `Task<CachePolicy>` | Lookup by name |
| `GetActivePoliciesAsync()` | `Task<IEnumerable<CachePolicy>>` | All `IsActive = true` policies |
| `GetByPolicyTypeAsync(CachePolicyType type)` | `Task<IEnumerable<CachePolicy>>` | Filter by expiry strategy |
| `DisablePolicyAsync(int policyId)` | `Task<bool>` | Set `IsActive = false` |
| `EnablePolicyAsync(int policyId)` | `Task<bool>` | Set `IsActive = true` |

---

## 8. Cache Key Constants

`CacheKeyConstants` is a `static class` in `DOMAIN_LAYER.Enum`. It provides centralized, format-string templates for all Redis keys used across the system. All keys are combined with the global prefix `tripconnect:` at runtime.

| Constant | Template | Example resolved key |
|---|---|---|
| `USER_BY_ID` | `user:id:{0}` | `tripconnect:user:id:42` |
| `USER_BY_EMAIL` | `user:email:{0}` | `tripconnect:user:email:john@example.com` |
| `USER_ALL` | `user:all` | `tripconnect:user:all` |
| `USER_PROFILE` | `user:profile:{0}` | `tripconnect:user:profile:42` |
| `TRIP_BY_ID` | `trip:id:{0}` | `tripconnect:trip:id:7` |
| `TRIP_ALL` | `trip:all` | `tripconnect:trip:all` |
| `TRIP_BY_USER` | `trip:user:{0}` | `tripconnect:trip:user:42` |
| `TRIP_MEMBERS` | `trip:members:{0}` | `tripconnect:trip:members:7` |
| `TRIP_DETAILS` | `trip:details:{0}` | `tripconnect:trip:details:7` |
| `EXPENSE_BY_ID` | `expense:id:{0}` | `tripconnect:expense:id:15` |
| `EXPENSE_BY_TRIP` | `expense:trip:{0}` | `tripconnect:expense:trip:7` |
| `EXPENSE_SUMMARY` | `expense:summary:{0}` | `tripconnect:expense:summary:7` |
| `EXPENSE_SPLITS` | `expense:splits:{0}` | `tripconnect:expense:splits:15` |
| `CHAT_MESSAGES` | `chat:messages:{0}` | `tripconnect:chat:messages:7` |
| `CHAT_HISTORY` | `chat:history:{0}:{1}` | `tripconnect:chat:history:7:1` |
| `JOIN_REQUEST_BY_ID` | `joinrequest:id:{0}` | — |
| `JOIN_REQUEST_BY_TRIP` | `joinrequest:trip:{0}` | — |
| `JOIN_REQUEST_BY_USER` | `joinrequest:user:{0}` | — |
| `RATING_BY_ID` | `rating:id:{0}` | — |
| `RATING_BY_TRIP` | `rating:trip:{0}` | — |
| `RATING_USER` | `rating:user:{0}` | — |
| `TRIP_MEMBER_BY_ID` | `tripmember:id:{0}` | — |
| `TRIP_MEMBER_BY_TRIP` | `tripmember:trip:{0}` | — |
| `CACHE_INVALIDATE_PATTERN` | `{0}:*` | `tripconnect:trip:*` |

**Usage pattern:**

```csharp
var key = string.Format(CacheKeyConstants.TRIP_BY_ID, tripId);
// key = "trip:id:7"
// final Redis key = "tripconnect:trip:id:7"
```

---

## 9. Design Decisions

### Why no data annotations on entities?

EF Core fluent configuration (in `INFRASTRUCTURE_LAYER/Data/TripConnectDbContext.cs`) is preferred over `[Required]`, `[MaxLength]` etc. on entities. This keeps Domain entities clean and independent of EF Core.

### Why separate `TripRequest` and `TripMember`?

A user goes through an approval workflow before becoming a member. `TripRequest` is the pending state; `TripMember` is the confirmed state. Keeping them as separate entities makes status tracking clean and queryable independently.

### Why does `User` store `RefreshToken` directly?

The refresh token is stored on `User` for simplicity in this single-user-session design. Each new login overwrites the previous token. `RefreshTokenExpiryTime` ensures old tokens cannot be reused.

### Why are `CacheEntry` and `CachePolicy` in the Domain?

Cache management is treated as a first-class business concern, not just an infrastructure detail. Storing cache metadata in the domain allows business rules (e.g., "entries for premium content never expire") to be expressed at this level.

### Why integers for enum values?

All enums start at `1` (not `0`) intentionally — a value of `0` in the database unambiguously indicates an unset/default state rather than a valid enum member. This aids data integrity debugging.

---

*Document generated from source code analysis — May 8, 2026.*

