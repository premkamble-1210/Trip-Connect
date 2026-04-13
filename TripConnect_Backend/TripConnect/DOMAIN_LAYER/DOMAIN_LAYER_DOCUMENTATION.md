# **DOMAIN_LAYER - Detailed Documentation**

## **Overview**

The **DOMAIN_LAYER** is the core of the TripConnect application following **Clean Architecture** principles. It contains:
- **Entity Models** - Represent database tables
- **Enumerations** - Define constant values
- **Repository Interfaces** - Define data access contracts

**Location:** `TripConnect_Backend/TripConnect/DOMAIN_LAYER/`

---

## **Directory Structure**

```
DOMAIN_LAYER/
├── Entity/               # Entity models
│   ├── User/
│   ├── Trip/
│   ├── TripRequest/
│   ├── TripMember/
│   ├── Expense/
│   ├── ExpenseSplit/
│   ├── ChatMessage/
│   └── TripRating/
├── Enum/                 # Enumeration values
│   ├── TripStatus.cs
│   ├── TripRequestStatus.cs
│   ├── TripMemberRole.cs
│   └── TripMemberStatus.cs
├── Repository/           # Repository interfaces
│   ├── IRepository.cs
│   ├── IUserRepository.cs
│   ├── ITripRepository.cs
│   ├── ITripRequestRepository.cs
│   ├── ITripMemberRepository.cs
│   ├── IExpenseRepository.cs
│   ├── IExpenseSplitRepository.cs
│   ├── IChatRepository.cs
│   ├── ITripRatingRepository.cs
│   └── IUnitOfWork.cs
└── DOMAIN_LAYER.csproj   # Project file
```

---

## **📊 Entity Models**

### **1. User Entity** 👤
**Purpose:** Represents a user in the TripConnect platform

**Properties:**
- `Id` - Unique identifier (PK)
- `Name` - User's full name
- `Email` - User's email (unique)
- `Username` - Username for authentication (unique)
- `Phone` - Phone number
- `Rating` - User reputation score (0.0-5.0)
- `PhoneVerified` - Phone verification flag
- `IdVerified` - ID verification flag
- `PasswordHash` - Hashed password (never plain text)
- `PasswordSalt` - Salt for password hashing
- `CreatedAt` - Account creation timestamp

**Relationships (Navigation Properties):**
- `CreatedTrips` (1:M) - Trips created by this user
- `TripRequests` (1:M) - Join requests sent
- `TripMembers` (1:M) - Trip memberships
- `PaidExpenses` (1:M) - Expenses paid
- `ExpenseSplits` (1:M) - Expense splits owed
- `ChatMessages` (1:M) - Messages sent
- `RatingsGiven` (1:M) - Ratings given to others
- `RatingsReceived` (1:M) - Ratings received from others

---

### **2. Trip Entity** ✈️
**Purpose:** Represents a trip posting

**Properties:**
- `Id` - Unique identifier (PK)
- `Title` - Trip title
- `Description` - Trip details
- `Location` - Destination/location
- `Budget` - Planned budget (decimal)
- `StartDate` - Trip start date
- `EndDate` - Trip end date
- `Seats` - Available seats
- `TravelType` - Type (Trekking, Beach, Biking, etc.)
- `Status` - Trip status (Planned, Ongoing, Completed, Cancelled)
- `HostId` - Trip organizer's user ID (FK)
- `CreatedAt` - Creation timestamp

**Relationships:**
- `Host` (1:1) - Trip organizer
- `TripRequests` (1:M) - Join requests
- `TripMembers` (1:M) - Trip members
- `Expenses` (1:M) - Trip expenses
- `ChatMessages` (1:M) - Group chat messages
- `Ratings` (1:M) - User ratings for this trip

---

### **3. TripRequest Entity** 📬
**Purpose:** Represents a join request for a trip

**Properties:**
- `Id` - Unique identifier (PK)
- `TripId` - Trip ID (FK)
- `UserId` - Requesting user ID (FK)
- `Status` - Request status (Pending, Accepted, Rejected, Cancelled)
- `RequestedAt` - Request creation timestamp
- `RespondedAt` - Response timestamp (optional)

**Relationships:**
- `User` (1:M) - User who sent request
- `Trip` (1:M) - Trip requested to join

---

### **4. TripMember Entity** 👥
**Purpose:** Represents an active member in a trip group

**Properties:**
- `Id` - Unique identifier (PK)
- `TripId` - Trip ID (FK)
- `UserId` - User ID (FK)
- `Role` - Role (Host, Member)
- `Status` - Status (Active, Left, Removed)
- `JoinedAt` - Join timestamp

**Relationships:**
- `User` (1:M) - User reference
- `Trip` (1:M) - Trip reference

---

### **5. Expense Entity** 💰
**Purpose:** Represents an expense in a trip

**Properties:**
- `Id` - Unique identifier (PK)
- `TripId` - Trip ID (FK)
- `PaidBy` - User ID who paid (FK)
- `Amount` - Expense amount (decimal)
- `Description` - Category/description (Hotel, Fuel, Food, etc.)
- `CreatedAt` - Creation timestamp

**Relationships:**
- `Trip` (1:M) - Trip this belongs to
- `PaidByUser` (1:M) - User who paid
- `ExpenseSplits` (1:M) - How expense is split

---

### **6. ExpenseSplit Entity** 🧾
**Purpose:** Represents how an expense is split among trip members

**Properties:**
- `Id` - Unique identifier (PK)
- `ExpenseId` - Expense ID (FK)
- `UserId` - User who owes (FK)
- `AmountOwed` - Amount owed (decimal)
- `IsSettled` - Settlement flag (true = paid)

**Relationships:**
- `Expense` (1:M) - Parent expense
- `User` (1:M) - User who owes

---

### **7. ChatMessage Entity** 💬
**Purpose:** Represents a message in trip group chat

**Properties:**
- `Id` - Unique identifier (PK)
- `TripId` - Trip ID (FK)
- `SenderId` - Sender's user ID (FK)
- `Message` - Message content
- `CreatedAt` - Timestamp

**Relationships:**
- `Trip` (1:M) - Trip this message belongs to
- `Sender` (1:M) - User who sent message

---

### **8. TripRating Entity** ⭐
**Purpose:** Represents a user rating given for a trip

**Properties:**
- `Id` - Unique identifier (PK)
- `TripId` - Trip ID (FK)
- `RatedBy` - Rater's user ID (FK)
- `RatedUserId` - Rated user ID (FK)
- `Rating` - Rating score (1.0-5.0)
- `Review` - Review/comment
- `CreatedAt` - Timestamp

**Relationships:**
- `Trip` (1:M) - Trip reference
- `RatedByUser` (1:M) - User giving rating
- `RatedUser` (1:M) - User being rated

---

## **📋 Enumerations**

### **TripStatus**
```
Planned = 1      → Trip is planned but not started
Ongoing = 2      → Trip is currently happening
Completed = 3    → Trip has finished
Cancelled = 4    → Trip was cancelled
```

### **TripRequestStatus**
```
Pending = 1      → Waiting for organizer response
Accepted = 2     → User accepted
Rejected = 3     → User rejected
Cancelled = 4    → User cancelled request
```

### **TripMemberRole**
```
Host = 1         → Trip organizer
Member = 2       → Regular participant
```

### **TripMemberStatus**
```
Active = 1       → Actively participating
Left = 2         → Voluntarily left
Removed = 3      → Removed by host
```

---

## **🗄️ Repository Interfaces**

### **IRepository<T>** - Generic Repository

**Generic CRUD Operations:**
- `GetByIdAsync(int id)` - Get entity by ID
- `GetAllAsync()` - Get all entities
- `AddAsync(T entity)` - Add new entity
- `UpdateAsync(T entity)` - Update entity
- `DeleteAsync(int id)` - Delete entity
- `SaveChangesAsync()` - Save to database
- `ExistsAsync(int id)` - Check if exists

---

### **Specific Repository Interfaces**

1. **IUserRepository** - User-specific queries
2. **ITripRepository** - Trip-specific queries
3. **ITripRequestRepository** - Join request queries
4. **ITripMemberRepository** - Member queries
5. **IExpenseRepository** - Expense queries
6. **IExpenseSplitRepository** - Split queries
7. **IChatRepository** - Message queries
8. **ITripRatingRepository** - Rating queries

---

### **IUnitOfWork** - Transaction Manager

**Properties:**
- `Users` (IUserRepository)
- `Trips` (ITripRepository)
- `TripRequests` (ITripRequestRepository)
- `TripMembers` (ITripMemberRepository)
- `Expenses` (IExpenseRepository)
- `ExpenseSplits` (IExpenseSplitRepository)
- `ChatMessages` (IChatRepository)
- `TripRatings` (ITripRatingRepository)

**Methods:**
- `SaveChangesAsync()` - Commit all changes
- `BeginTransactionAsync()` - Start transaction
- `CommitTransactionAsync()` - Commit transaction
- `RollbackTransactionAsync()` - Rollback transaction
- `HasActiveTransaction` - Check if transaction is active

---

## **🔗 Entity Relationships (ER Diagram Representation)**

```
USER (1) ──────────→ (M) TRIP (as Host)
  ↓
  ├─→ (1:M) TRIP_REQUEST
  ├─→ (1:M) TRIP_MEMBER
  ├─→ (1:M) EXPENSE (as PaidBy)
  ├─→ (1:M) EXPENSE_SPLIT
  ├─→ (1:M) CHAT_MESSAGE
  ├─→ (1:M) TRIP_RATING (as RatedBy)
  └─→ (1:M) TRIP_RATING (as RatedUser)

TRIP (1) ────────────→ (M) TRIP_REQUEST
  ↓
  ├─→ (1:M) TRIP_MEMBER
  ├─→ (1:M) EXPENSE
  ├─→ (1:M) CHAT_MESSAGE
  └─→ (1:M) TRIP_RATING

EXPENSE (1) ─────────→ (M) EXPENSE_SPLIT (relationship tracking)
```

---

## **Key Design Patterns**

1. **Unit of Work Pattern** - Manages transactions and repositories
2. **Repository Pattern** - Abstract data access
3. **Dependency Injection** - Through IServiceCollection
4. **Entity-First Design** - Models drive database schema via EF Core migrations

---


This is the **DOMAIN_LAYER** structure - the foundation of your TripConnect backend architecture! ✅
