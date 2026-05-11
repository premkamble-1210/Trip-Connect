# TripConnect — Architecture Document

> **Analyzed from source code on May 8, 2026.**  
> This document reflects the actual implementation, not older documentation files.

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [High-Level Architecture](#2-high-level-architecture)
3. [Backend — Clean Architecture](#3-backend--clean-architecture)
   - [Domain Layer](#31-domain-layer)
   - [Application Layer](#32-application-layer)
   - [Infrastructure Layer](#33-infrastructure-layer)
   - [API Layer](#34-api-layer)
4. [Frontend — Angular](#4-frontend--angular)
5. [Data Flow](#5-data-flow)
6. [Entity Relationship Overview](#6-entity-relationship-overview)
7. [Security Architecture](#7-security-architecture)
8. [Caching Architecture](#8-caching-architecture)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [Technology Stack](#10-technology-stack)

---

## 1. System Overview

TripConnect is a full-stack travel companion platform that allows users to:

- **Create and explore** group trips with detailed itineraries
- **Join trips** via a request/approval workflow
- **Chat** within trip groups
- **Track and split expenses** among trip members
- **Rate** each other after trips
- **Upload images** for profiles and trips via ImageKit CDN

The system is split into two main parts:

| Part | Technology | Port |
|---|---|---|
| Backend API | ASP.NET Core 8, C# | `https://localhost:7xxx` |
| Frontend SPA | Angular 17+ (standalone) | `http://localhost:4200` |
| Database | SQL Server (LocalDB for dev) | — |
| Cache | Redis (RedisLabs cloud) | — |

---

## 2. High-Level Architecture

```
┌──────────────────────────────────────────────────────────┐
│                   Angular Frontend (SPA)                  │
│  Pages · Components · Services · Guards · Interceptors    │
└──────────────────────┬───────────────────────────────────┘
                       │  HTTP/REST  (JWT Bearer token)
                       ▼
┌──────────────────────────────────────────────────────────┐
│                  ASP.NET Core API Layer                   │
│  Controllers · JWT Auth · CORS · Swagger · Middleware     │
└──────────────────────┬───────────────────────────────────┘
                       │  Interfaces (DI)
                       ▼
┌──────────────────────────────────────────────────────────┐
│                   Application Layer                       │
│  Services · DTOs · AutoMapper · Email · SMS · Cache       │
└──────────────────────┬───────────────────────────────────┘
                       │  Repository Interfaces (DI)
                       ▼
┌──────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                     │
│  EF Core · Repositories · Redis Cache · ImageKit · Serilog│
└──────────────────────┬───────────────────────────────────┘
                       │
           ┌───────────┴────────────┐
           ▼                        ▼
┌─────────────────┐      ┌────────────────────┐
│  SQL Server DB  │      │  Redis Cache Cloud  │
│  (EF Core ORM)  │      │  (RedisLabs)        │
└─────────────────┘      └────────────────────┘
```

---

## 3. Backend — Clean Architecture

The backend follows **Clean Architecture** (also known as Onion Architecture). Dependencies only point inward — outer layers depend on inner ones, never the reverse.

```
Domain Layer          ← No external dependencies
Application Layer     ← Depends on Domain
Infrastructure Layer  ← Depends on Domain + Application
API Layer             ← Depends on Application + Infrastructure (for DI registration)
```

---

### 3.1 Domain Layer

**Project:** `DOMAIN_LAYER`  
**Role:** Core business entities and repository contracts. Has zero external NuGet dependencies.

#### Entities

| Entity | Key Fields | Relationships |
|---|---|---|
| `User` | Id, Name, Email, Username, Phone, Rating, PhoneVerified, IdVerified, PasswordHash, PasswordSalt | Host of Trips, Member of Trips, Sender of ChatMessages, TripRequests, Expenses |
| `Trip` | Id, Title, Description, Location, Budget, StartDate, EndDate, Seats, TravelType, ImgUrl, Status, HostId | Has TripDays (1:M), TripMembers (1:M), TripRequests (1:M), Expenses (1:M), ChatMessages (1:M) |
| `TripDay` | Id, TripId, Day, Title, Activities | Belongs to Trip |
| `TripMember` | Id, TripId, UserId, Role, Status, JoinedAt | Junction between User and Trip; Role = Host/Member; Status = Active/Left/Removed |
| `TripRequest` | Id, TripId, UserId, Status, RequestedAt, RespondedAt | Join request from User to Trip; Status = Pending/Accepted/Rejected/Cancelled |
| `Expense` | Id, TripId, PaidBy, Amount, Description, CreatedAt | Belongs to Trip; paid by User; has ExpenseSplits |
| `ExpenseSplit` | Id, ExpenseId, UserId, Amount, IsSettled | Each member's share of an Expense |
| `ChatMessage` | Id, TripId, SenderId, Message, CreatedAt | Belongs to Trip; sent by User |
| `TripRating` | Id, TripId, RaterId, RatedUserId, Rating, Review, CreatedAt | User-to-user rating within a Trip |

#### Enumerations

| Enum | Values |
|---|---|
| `TripStatus` | Planned, Ongoing, Completed, Cancelled |
| `TripMemberRole` | Host, Member |
| `TripMemberStatus` | Active, Left, Removed |
| `TripRequestStatus` | Pending, Accepted, Rejected, Cancelled |
| `CachePolicyType` | (cache policy variants) |
| `CacheKeyConstants` | Centralized cache key prefix constants |

#### Repository Interfaces

All interfaces live in `DOMAIN_LAYER/Repository/`:

- `IRepository<T>` — Generic CRUD interface
- `IUserRepository`
- `ITripRepository`
- `ITripRequestRepository`
- `ITripMemberRepository`
- `IExpenseRepository`
- `IExpenseSplitRepository`
- `IChatRepository`
- `ITripRatingRepository`
- `IUnitOfWork` — Transaction coordination across repositories
- `ICacheRepository`, `ICacheEntryRepository`, `ICacheInvalidationRepository`, `ICachePolicyRepository`

---

### 3.2 Application Layer

**Project:** `APPLICATION_LAYER`  
**Role:** Business logic, orchestration, DTO mapping, and service contracts.

#### Service Interfaces → Implementations

| Interface | Responsibility |
|---|---|
| `IUserService` | Register, Login, GetById, Update, VerifyPhone/Id, UsernameExists |
| `ITripService` | Create, GetById, GetAll (paginated), Search (location/date/budget/type), GetByStatus, GetUpcoming, Update, Cancel, GetUserTrips, GetTripMembers |
| `IJoinRequestService` | Send join request, Accept/Reject, Cancel, GetRequestsForTrip, GetRequestsByUser |
| `IExpenseService` | CreateExpense (auto-splits among members), GetByTrip, GetSummary, SettleExpense, GetUnsettled, GetTotalOwed |
| `IChatService` | SendMessage, GetByTrip (paginated), GetLatest, SearchMessages, DeleteMessage |
| `IRatingService` | Rate user, GetRatingsForUser, GetRatingsForTrip |
| `IImageService` | UploadProfileImage, UploadTripImage, UploadExpenseImage, DeleteImage, GetFileId/Url |
| `ITokenService` | GenerateAccessToken (JWT), GenerateRefreshToken, GetPrincipalFromExpiredToken |
| `IRefreshTokenService` | Token refresh workflow |
| `IEmailService` | Send transactional emails (SMTP/Gmail) |
| `ISmsService` | Send SMS via Twilio Verify |

#### DTOs

Organized by domain folder under `DTOs/`:

| Folder | Key DTOs |
|---|---|
| `Auth/` | `JwtTokenResponseDto`, `AuthResponseDto`, `RefreshTokenRequestDto` |
| `User/` | `CreateUserDto`, `LoginUserDto`, `UpdateUserDto`, `UserResponseDto` |
| `Trip/` | `CreateTripDto`, `UpdateTripDto`, `TripResponseDto` |
| `TripMember/` | `TripMemberResponseDto` |
| `JoinRequest/` | `CreateJoinRequestDto`, `JoinRequestResponseDto` |
| `Expense/` | `CreateExpenseDto`, `ExpenseResponseDto`, `ExpenseSummaryDto` |
| `Chat/` | `SendMessageDto`, `ChatMessageResponseDto` |
| `Rating/` | `CreateRatingDto`, `RatingResponseDto` |
| Root | `ImageUploadRequestDto`, `ImageUploadResponseDto` |

#### AutoMapper Profiles

- `UserProfile`
- `TripProfile`
- `TripRequestProfile`
- `ExpenseProfile`
- `ChatMessageProfile`
- `TripRatingProfile`

#### Configuration Models

- `JwtSettings` — SecretKey, Issuer, Audience, ExpirationMinutes, RefreshTokenExpirationDays
- `EmailSettings` — SMTP host, port, sender
- `TwilioSettings` — AccountSid, AuthToken, VerifyServiceSid

---

### 3.3 Infrastructure Layer

**Project:** `INFRASTRUCTURE_LAYER`  
**Role:** All I/O concerns — database, cache, external services, and logging.

#### Database (EF Core)

- **DbContext:** `TripConnectDbContext`
- **ORM:** Entity Framework Core with SQL Server provider
- **Connection:** SQL Server (LocalDB in dev — `TripConnect` database)
- **Migrations:** managed under `Migrations/`
- **Factory:** `TripConnectDbContextFactory` for design-time migration tooling

#### Repositories (Concrete Implementations)

| Repository | Description |
|---|---|
| `Repository<T>` | Generic EF Core CRUD implementation |
| `UserRepository` | User-specific queries (by email, username) |
| `TripRepository` | Trip queries (by status, location, user, upcoming) |
| `TripRequestRepository` | Request status queries |
| `TripMemberRepository` | Member management queries |
| `ExpenseRepository` | Expense + split queries |
| `ExpenseSplitRepository` | Per-member split settlement |
| `ChatRepository` | Message queries with pagination/search |
| `TripRatingRepository` | Rating retrieval |
| `UnitOfWork` | Wraps `DbContext.SaveChangesAsync()` |

#### Redis Cache

| Component | Description |
|---|---|
| `RedisCacheConfiguration` | ConnectionString, KeyPrefix (`tripconnect:`), TTL settings |
| `CacheService` | Core get/set/delete operations using StackExchange.Redis |
| `CacheStatisticsService` | Cache hit/miss tracking |
| `CacheWarmingService` | Pre-loads frequently accessed data on startup |
| `CacheCleanupBackgroundService` | Background service (`IHostedService`) for periodic cleanup |
| Cache Repositories | `ICacheEntryRepository`, `ICacheInvalidationRepository`, `ICachePolicyRepository` implementations |

**Redis settings (from appsettings.json):**

```
ConnectionString: redis-15777.crce286.ap-south-1-1.ec2.cloud.redislabs.com:15777
KeyPrefix:        tripconnect:
AbsoluteExpiry:   3600s (1 hour)
SlidingExpiry:    1800s (30 min)
CleanupInterval:  60 min
```

#### ImageKit Service

- `IImageKitService` / `ImageKitService` — Wraps ImageKit REST API via `HttpClient`
- Supports upload to folders: `profiles/`, `trips/`, `expenses/`
- Returns `fileId` + CDN URL for each uploaded image

#### Logging (Serilog)

- **Provider:** Serilog with rolling file sink
- **Log file pattern:** `logs/tripconnect-<date>.txt` (rolling daily, 7-day retention)
- **Minimum level:** Information (Microsoft/EF Core overridden to Warning to suppress noise)

---

### 3.4 API Layer

**Project:** `API_LAYER`  
**Role:** HTTP surface — controllers, authentication, middleware, and pipeline configuration.

#### Controllers

| Controller | Route | Auth Required | Key Actions |
|---|---|---|---|
| `UserController` | `/api/user` | Partial | Register, Login, RefreshToken, GetById, Update, VerifyPhone, VerifyId |
| `TripController` | `/api/trip` | Partial | Create, GetById, GetAll, Search, GetByStatus, GetUpcoming, Update, Cancel, GetUserTrips, GetMembers |
| `JoinRequestController` | `/api/joinrequest` | Yes | SendRequest, Accept, Reject, Cancel, GetForTrip, GetByUser |
| `ExpenseController` | `/api/expense` | Yes | CreateExpense, GetByTrip, GetSummary, Settle, GetUnsettled |
| `ChatController` | `/api/chat` | Yes | SendMessage, GetByTrip, GetPaginated, SearchMessages, Delete |
| `RatingController` | `/api/rating` | Yes | RateUser, GetRatingsForUser, GetRatingsForTrip |
| `ImageController` | `/api/image` | Yes | UploadProfileImage, UploadTripImage, UploadExpenseImage, Delete |
| `HealthController` | `/api/health` | No | Health check endpoint |

#### JWT Authentication

- **Scheme:** `JwtBearer` (RS256 → HS256 via HMAC-SHA)
- **Claims stored:** `NameIdentifier` (userId), `Name` (username), `Email`
- **Access token lifetime:** 60 minutes
- **Refresh token lifetime:** 7 days
- **Custom events:**
  - `OnChallenge` → returns `{ success: false, errorCode: "TOKEN_EXPIRED" | "AUTH_FAILED" }`
  - `OnForbidden` → returns `{ success: false, errorCode: "FORBIDDEN" }`

#### Middleware Pipeline (in order)

```
ErrorHandlingMiddleware  ← global exception → structured JSON error
HTTPS Redirection
Static Files
Routing
CORS (AllowAngularFrontend)
Authentication (JWT)
Authorization
Map Controllers
```

#### Error Handling (`ErrorHandlingMiddleware`)

Maps exceptions to HTTP status codes:

| Exception | Status | Error Code |
|---|---|---|
| `SecurityTokenExpiredException` | 401 | `TOKEN_EXPIRED` |
| `SecurityTokenInvalidSignatureException` | 401 | `INVALID_SIGNATURE` |
| `SecurityTokenValidationException` | 401 | `TOKEN_INVALID` |
| `UnauthorizedAccessException` | 401 | `UNAUTHORIZED` |
| `ArgumentException` | 400 | `BAD_REQUEST` |
| `InvalidOperationException` | 400 | `INVALID_OPERATION` |
| `KeyNotFoundException` | 404 | `NOT_FOUND` |
| Others | 500 | `INTERNAL_ERROR` |

#### CORS Policy

Allowed origins: `http://localhost:4200`, `https://localhost:4200`, `http://localhost:3000`, `https://localhost:3000`. Credentials allowed.

---

## 4. Frontend — Angular

**Framework:** Angular 17+ with standalone components  
**Base URL:** configured in `environments/environment.ts`

### Application Structure

```
src/app/
├── app.ts                  ← Root component
├── app.routes.ts           ← Route definitions
├── app.config.ts           ← Provider configuration
│
├── Pages/                  ← Route-level page components
│   ├── login/              ← Login form
│   ├── register/           ← Registration form
│   ├── expore-trips/       ← Trip discovery / browse
│   ├── trip/               ← Single trip detail view
│   ├── new-trip/           ← Create trip form
│   ├── edit-trip/          ← Edit existing trip
│   ├── join-request/       ← Join request management
│   ├── profile/            ← Current user profile
│   └── view-profile/       ← View another user's profile
│
├── Components/             ← Reusable UI components
│   ├── navbar/
│   ├── trip-card/
│   ├── trip-itinerary/
│   └── trip-member-list/
│
├── services/               ← HTTP service layer
│   ├── auth.service.ts     ← Login, register, logout, token storage
│   ├── trip.service.ts     ← Trip CRUD, search, members
│   ├── join-request.service.ts
│   └── image.service.ts
│
├── interceptors/
│   └── auth.interceptor.ts ← Attaches Bearer token to every request
│
├── guards/
│   └── auth.guard.ts       ← Route guard — redirects to /login if unauthenticated
│
└── models/
    └── api.types.ts        ← TypeScript interfaces matching all backend DTOs
```

### Routing

| Route | Component | Auth Guard |
|---|---|---|
| `/login` | `Login` | No |
| `/register` | `Register` | No |
| `/explore` | `ExporeTrips` | No |
| `/create` | `NewTrip` | Yes |
| `/profile` | `Profile` | Yes |
| `/trip/:id` | `Trip` | Yes |
| `/edit/:id` | `EditTrip` | Yes |
| `/request/:id` | `JoinRequest` | Yes |
| `/user/:id` | `ViewProfile` | Yes |
| `/` | redirects to `/explore` | — |

### Token Management (Frontend)

Tokens are stored in `localStorage` under these keys:

| Key | Value |
|---|---|
| `tc_token` | JWT access token |
| `tc_refresh_token` | Refresh token |
| `tc_userId` | Logged-in user's ID |

The `authInterceptor` reads `tc_token` and appends `Authorization: Bearer <token>` to every outgoing HTTP request.

---

## 5. Data Flow

### User Registration / Login

```
Angular (AuthService)
  → POST /api/user/register  or  /api/user/login
  → UserController
  → IUserService.RegisterAsync / LoginAsync
  → IUserRepository (SQL Server)  +  ITokenService (JWT generation)
  ← AuthResponseDto { success, user, token { accessToken, refreshToken } }
  → localStorage (tc_token, tc_refresh_token, tc_userId)
```

### Create a Trip

```
Angular (TripService)
  → POST /api/trip  (Authorization: Bearer <token>)
  → TripController.CreateTrip
  → ClaimsPrincipal → extract userId
  → ITripService.CreateTripAsync(dto, userId)
  → ITripRepository.Add + IUnitOfWork.SaveAsync
  → ICacheService.InvalidateAsync (trip list cache)
  ← TripResponseDto (201 Created)
```

### Expense Split Flow

```
Angular (ExpenseService)
  → POST /api/expense
  → ExpenseController
  → IExpenseService.CreateExpenseAsync
  → Fetches active TripMembers via ITripMemberRepository
  → Creates Expense + auto-splits (Amount / MemberCount) per member
  → IUnitOfWork.SaveAsync
  ← ExpenseResponseDto with splits
```

---

## 6. Entity Relationship Overview

```
User ──────────────────────── hosts ──────────── Trip
 │                                                │
 ├── TripMember (role: Host/Member) ──────────── Trip
 ├── TripRequest (join request) ────────────────  Trip
 ├── ChatMessage (sender) ──────────────────────  Trip
 ├── Expense (paidBy) ──────────────────────────  Trip
 │     └── ExpenseSplit (per member) ─── User
 └── TripRating (rater → ratedUser, within Trip)

Trip
 └── TripDay (day-by-day itinerary)
```

---

## 7. Security Architecture

### Authentication & Authorization

- **JWT Bearer tokens** with HMAC-SHA256 signing
- Access token: 60-minute lifetime, validated on every protected request
- Refresh token: 7-day lifetime, stored in DB, used to issue new access tokens
- Claims: `NameIdentifier` (userId), `Name`, `Email`

### Password Security

- Passwords are **never stored in plaintext**
- Each user has a unique `PasswordSalt`
- `PasswordHash` is derived using a cryptographic hash function

### API Security

- All mutating endpoints (`POST`, `PUT`, `DELETE`) require `[Authorize]`
- Business logic validates that the requesting user owns the resource (e.g., only trip host can cancel/update)
- Global `ErrorHandlingMiddleware` prevents stack traces from leaking to clients
- CORS restricted to known frontend origins

### Frontend Security

- `authGuard` prevents unauthenticated access to protected routes
- `authInterceptor` automatically injects tokens — no manual token injection needed per service call
- Tokens stored in `localStorage` (with `tc_` prefix namespace)

---

## 8. Caching Architecture

Redis (hosted on RedisLabs, ap-south-1 region) is used as a distributed cache.

### Cache Layers

| Cache Target | TTL | Invalidated On |
|---|---|---|
| Trip lists (all, by status, by location) | 1 hour | Trip create/update/cancel |
| Single trip detail | 1 hour | Trip update/cancel |
| User profile | 1 hour | User update |
| Chat messages | 30 min | New message sent |

### Cache Key Convention

All keys are prefixed with `tripconnect:` (configurable). Example: `tripconnect:trips:all`, `tripconnect:trip:42`.

### Background Services

- `CacheWarmingService` — pre-loads popular trips on application startup
- `CacheCleanupBackgroundService` — runs on a 60-minute interval to remove stale entries

---

## 9. Cross-Cutting Concerns

| Concern | Implementation |
|---|---|
| Logging | Serilog → rolling file (`logs/tripconnect-<date>.txt`), min level Info |
| Error Handling | `ErrorHandlingMiddleware` → structured JSON errors |
| Validation | ASP.NET Core `ModelState` validation on all request DTOs |
| Mapping | AutoMapper profiles in Application Layer |
| DI Registration | `AddApplicationLayer()` + `AddInfrastructure()` extension methods |
| External Email | SMTP via Gmail (`trip.connect123@gmail.com`) |
| External SMS | Twilio Verify API |
| Image CDN | ImageKit.io (upload → CDN URL returned) |
| Health Check | `/api/health` endpoint |

---

## 10. Technology Stack

### Backend

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API framework |
| C# 12 | Language |
| Entity Framework Core | ORM / Migrations |
| SQL Server (LocalDB) | Relational database |
| StackExchange.Redis | Redis client |
| RedisLabs Cloud | Hosted Redis |
| AutoMapper | Object mapping |
| Serilog | Structured logging |
| Microsoft.IdentityModel.Tokens | JWT handling |
| ImageKit.io (REST) | Image CDN |
| Twilio Verify | SMS verification |
| SMTP / Gmail | Email notifications |
| Swagger / Swashbuckle | API documentation |

### Frontend

| Technology | Purpose |
|---|---|
| Angular 17+ | SPA framework |
| TypeScript | Language |
| Angular Standalone Components | Component architecture |
| Angular Router | Client-side routing |
| Angular HttpClient | HTTP communication |
| RxJS | Reactive streams |
| localStorage | Token persistence |

---

*Document generated from source code analysis — May 8, 2026.*


---

# Interview Questions — Architecture (with Answers)

**1. What is Clean Architecture and how did you implement it in TripConnect?**

Clean Architecture is a way of organizing code so that the core business logic is separated from technical details like databases, frameworks, or user interfaces. It uses layers, where each layer has a specific responsibility and depends only on the layers inside it.

In TripConnect, Clean Architecture is implemented using four layers:
- Domain Layer: Contains the core business entities and rules, with no dependencies on other layers.
- Application Layer: Contains business logic and service interfaces, using the domain layer but not knowing about infrastructure or APIs.
- Infrastructure Layer: Handles technical details like database access, caching, and external services. It implements interfaces defined in the application layer.
- API Layer: Handles HTTP requests, authentication, and routing. It depends on the application layer to process requests.

This structure makes the code easier to test, maintain, and change, because business logic is not mixed with technical details.

**2. Why did you choose a 4-layer architecture (Domain, Application, Infrastructure, API)?**

A 4-layer architecture was chosen to clearly separate concerns and make the project easier to develop, test, and maintain. Each layer has a specific role:

- Domain Layer: Focuses only on core business rules and entities, independent of any technology.
- Application Layer: Handles business logic and coordinates tasks, using the domain layer but not knowing about technical details.
- Infrastructure Layer: Deals with technical implementations like databases, caching, and external services, keeping these details out of business logic.
- API Layer: Manages HTTP requests, authentication, and communication with clients, without containing business rules.

This separation allows each part of the system to change or be tested independently, improves code organization, and makes the project more flexible for future updates.

**3. What is the dependency rule in Clean Architecture and how does it apply here?**

The dependency rule in Clean Architecture means that code dependencies always point inward, toward the core business logic. Outer layers (like API and Infrastructure) can depend on inner layers (like Application and Domain), but inner layers never depend on outer layers.

In TripConnect, this means:
- The Domain layer does not depend on any other layer.
- The Application layer depends only on the Domain layer.
- The Infrastructure layer depends on Application and Domain layers, but not the other way around.
- The API layer depends on Application and Domain layers, but they do not depend on the API.

This keeps business logic isolated from technical details, making the codebase more maintainable and flexible.

**4. What is the difference between the Domain layer and the Application layer in your project?**

The Domain layer contains only the core business entities, rules, and repository interfaces. It knows nothing about how things are stored or implemented.

The Application layer contains business logic, service interfaces, and DTOs. It uses the Domain layer to perform operations but does not know about infrastructure or APIs. The Application layer coordinates tasks and applies business rules using the entities from the Domain layer.

**5. Why does the Domain layer have zero external dependencies?**

The Domain layer has zero external dependencies to keep the core business logic pure and independent. This ensures that business rules are not affected by changes in frameworks, databases, or external services. It also makes the core logic easy to test and reuse in different contexts.

**6. What is the Repository Pattern and why did you use it?**

The Repository Pattern is a design pattern that provides an abstraction over data access, so the rest of the application does not need to know how data is stored or retrieved. In TripConnect, repositories are interfaces in the Domain layer and implemented in the Infrastructure layer. This allows changing the data source (like switching databases) without affecting business logic.

**7. What is the Unit of Work pattern and how does it coordinate transactions?**

The Unit of Work pattern is used to group multiple operations into a single transaction. In TripConnect, the Unit of Work interface is in the Domain layer and implemented in Infrastructure. It ensures that all changes to the database are committed together, so either all succeed or all fail, keeping data consistent.

**8. What is the difference between a Repository and the Unit of Work?**

A Repository handles operations for a single entity or aggregate (like User or Trip). The Unit of Work coordinates multiple repositories and manages the transaction, making sure all changes are saved together.

**9. How did you implement Dependency Injection in ASP.NET Core?**

Dependency Injection (DI) is implemented using the built-in DI container in ASP.NET Core. Services, repositories, and other dependencies are registered in the Startup or Program file, and in `DependencyInjection.cs` files in the Application and Infrastructure layers. The framework then provides these dependencies automatically where needed.

**10. What is `DependencyInjection.cs` and why do you have one in both Application and Infrastructure layers?**

`DependencyInjection.cs` files contain extension methods to register all services and dependencies for each layer. Having one in both Application and Infrastructure layers keeps the registration logic organized and ensures that each layer can be added to the DI container cleanly from the API layer.

**11. What are the advantages of separating repository interfaces (Domain) from implementations (Infrastructure)?**

Separating interfaces from implementations allows you to change how data is stored or accessed without affecting business logic. It also makes unit testing easier, since you can mock the interfaces. This separation improves flexibility and maintainability.

**12. What is a Generic Repository and what are its pros and cons?**

A Generic Repository provides common CRUD operations for any entity type, reducing code duplication. Pros: less repetitive code, easier to add new entities. Cons: may not handle complex queries well, and can hide important details if overused.

**13. How does AutoMapper work and why did you use it instead of manual mapping?**

AutoMapper is a library that automatically maps properties between objects, like from entities to DTOs and vice versa. It reduces boilerplate code and the risk of errors in manual mapping. In TripConnect, AutoMapper profiles are used to define these mappings for all major entities and DTOs.

**14. Can you explain the flow of a request from the HTTP call to the database?**

1. The client (Angular frontend) sends an HTTP request to the API layer.
2. The API controller receives the request and calls the appropriate service in the Application layer.
3. The Application layer service applies business logic and uses repository interfaces from the Domain layer.
4. The Infrastructure layer implements these repositories and interacts with the database (using Entity Framework Core) or other services.
5. The result is returned back through the layers to the API, which sends the response to the client.
