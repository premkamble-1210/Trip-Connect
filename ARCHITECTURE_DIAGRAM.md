# TripConnect - Visual Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        TRIPCONNECT PLATFORM                              │
└─────────────────────────────────────────────────────────────────────────┘

                            ┌──────────────────────┐
                            │   EXTERNAL SERVICES  │
                            ├──────────────────────┤
                            │ • Google Maps API    │
                            │ • Payment Gateway    │
                            │ • SMS/Email Service  │
                            │ • Cloud Storage      │
                            └──────────────────────┘
                                      ▲
                                      │
                    ┌─────────────────┴─────────────────┐
                    │                                   │
        ┌───────────────────────┐          ┌───────────────────────┐
        │   ANGULAR FRONTEND    │          │   .NET BACKEND APIs   │
        ├───────────────────────┤          ├───────────────────────┤
        │ • Trip Posting UI     │◄────────►│ Rest/GraphQL/SignalR  │
        │ • Trip Browsing       │ HTTPS    ├───────────────────────┤
        │ • Chat Interface      │          │ API_LAYER (Routes)    │
        │ • Dashboard           │          ├───────────────────────┤
        │ • Expense Tracker     │          │ APPLICATION_LAYER     │
        │ • User Profile        │          │ (Business Logic)      │
        │ • Notifications       │          ├───────────────────────┤
        │ • Maps Integration    │          │ DOMAIN_LAYER          │
        └───────────────────────┘          │ (Entities/Models)     │
                    │                       ├───────────────────────┤
                    │                       │ INFRASTRUCTURE_LAYER  │
                    │                       │ (Data Access)         │
                    │                       ├───────────────────────┤
                    │                       │ Database              │
                    └──────────────┬────────┘
                                   │
                    ┌──────────────┴───────────────┐
                    │                              │
            ┌───────────────────┐        ┌──────────────────┐
            │   SQL DATABASE    │        │ CACHE (Redis)    │
            ├───────────────────┤        ├──────────────────┤
            │ • Users           │        │ • Session Data   │
            │ • Trips           │        │ • Trip Cache     │
            │ • Groups          │        │ • Recommendations│
            │ • Messages        │        │ • Real-time Data │
            │ • Expenses        │        └──────────────────┘
            │ • Ratings         │
            │ • Verification    │
            └───────────────────┘


DATA FLOW - Trip Posting & Participation:

┌──────────────┐
│ User Posts   │
│ Trip Details │
└──────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ API: POST /api/trips                    │
│ Validation → Business Logic → Database  │
└─────────────────────────────────────────┘
       │
       ▼
┌──────────────────────┐
│ Trip Stored in DB    │
│ Cache Updated        │
│ Notifications Sent   │
└──────────────────────┘
       │
       ├─► Other Users Browse Trips (GET /api/trips)
       │
       ▼
┌──────────────────────┐
│ User Sends Join Req  │
│ POST /api/joinreq    │
└──────────────────────┘
       │
       ▼
┌──────────────────────────────────────────┐
│ Organizer Receives Notification          │
│ Views Join Requests                      │
│ Accepts/Rejects (PUT /api/joinreq/{id}) │
└──────────────────────────────────────────┘
       │
       ├─► If Accepted ──────────┐
       │                         │
       ▼                         ▼
┌──────────────────┐    ┌─────────────────────────┐
│ Participant      │    │ Group Created           │
│ Joins Group      │    │ Members Notified        │
│ Gets Added to    │    │ Chat Initialized        │
│ Group Chat       │    │ Expense Tracker Opened  │
└──────────────────┘    │ Location Sharing Ready  │
                        └─────────────────────────┘


FEATURE INTERACTION MAP:

                    ┌─────────────────────────┐
                    │  USER REGISTRATION &    │
                    │  VERIFICATION           │
                    └────────────┬────────────┘
                                 │
                ┌────────────────┼────────────────┐
                │                │                │
                ▼                ▼                ▼
         ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
         │ TRIP POSTING │ │ TRIP BROWSING│ │   RATINGS &  │
         └──────┬───────┘ └──────┬───────┘ │  REPUTATION  │
                │                │         └──────────────┘
                │                │
                │    ┌───────────┴────────────┐
                │    │                        │
                ▼    ▼                        ▼
         ┌────────────────────┐    ┌─────────────────────┐
         │  JOIN REQUEST      │◄───│  AI TRIP MATCHING   │
         │  SYSTEM            │    │  (Smart Suggestions)│
         └────────┬───────────┘    └─────────────────────┘
                  │
                  ▼
         ┌────────────────────────────┐
         │  GROUP COLLABORATION       │
         ├────────────────────────────┤
         │ • Real-time Chat (SignalR) │
         │ • Expense Sharing          │
         │ • Trip Voting              │
         │ • Location Sharing         │
         │ • Document Sharing         │
         └────────────────────────────┘
                  │
                  ▼
         ┌────────────────────┐
         │  TRIP COMPLETION   │
         │  & POST-FEEDBACK   │
         └────────────────────┘


SAFETY & TRUST LAYER:

┌──────────────────────────────────────────┐
│          INTEGRATED THROUGHOUT            │
├──────────────────────────────────────────┤
│ ✓ User Verification (Phone/ID)           │
│ ✓ Verified User Badges                   │
│ ✓ Emergency Contact Management           │
│ ✓ Live Location Sharing (Optional)       │
│ ✓ SOS Feature                            │
│ ✓ Trip & Participant Ratings             │
│ ✓ Travel Reputation Score                │
│ ✓ User Blocking/Reporting                │
│ ✓ Secure Payment Gateway Integration     │
└──────────────────────────────────────────┘
```

## Component Dependencies

```
┌─────────────────────┐
│  Frontend (Angular) │
└──────────┬──────────┘
           │ HTTP Requests
           │ WebSocket (Chat)
           ▼
┌─────────────────────────────────┐
│  API Layer (.NET Controllers)   │
│  - TripController               │
│  - UserController               │
│  - ChatController               │
│  - ExpenseController            │
│  - RecommendationController     │
└──────────┬──────────────────────┘
           │ Uses Services
           ▼
┌─────────────────────────────────┐
│ Application Layer (Services)    │
│ - TripService                   │
│ - UserService                   │
│ - ChatService                   │
│ - AIMatchingEngine              │
│ - ExpenseService                │
│ - VerificationService           │
└──────────┬──────────────────────┘
           │ Uses Models/Repos
           ▼
┌─────────────────────────────────┐
│  Domain Layer (Entities)        │
│  - User, Trip, Group            │
│  - ChatMessage, Expense, etc.   │
└──────────┬──────────────────────┘
           │ Mapped/Accessed By
           ▼
┌─────────────────────────────────┐
│  Infrastructure Layer           │
│  - EntityFramework DbContext    │
│  - Repository Pattern           │
│  - External Service Integrations│
└──────────┬──────────────────────┘
           │ Persists To
           ▼
┌─────────────────────────────────┐
│  Database                       │
│  - SQL Server / PostgreSQL      │
└─────────────────────────────────┘
```

## Real-time Communication Flow

```
┌─────────────────┐
│ User 1 (Client) │
└────────┬────────┘
         │
         │ WebSocket Connection
         │ (SignalR)
         │
         ▼
    ┌────────────────────────┐
    │ Chat Hub (Backend)     │
    │ Real-time Message Sync │
    └────┬───────────────────┘
         │
         ├─► Message Saved to DB
         │
         ├─► Broadcast to Other Group Members
         │
         ▼
    ┌─────────────────┐
    │ User 2 (Client) │
    └─────────────────┘
```

## User Journey - Complete Flow

```
START: User Visits Platform
   │
   ├─► [Sign Up / Login]
   │   └─► Verify Phone/Email
   │
   ├─► [Dashboard]
   │   ├─► View AI Recommendations
   │   ├─► Browse Trips
   │   └─► Post New Trip
   │
   ├─► [Post Trip]
   │   ├─► Destination, Budget, Date, Type
   │   ├─► Available Seats
   │   └─► Trip Published
   │
   ├─► [Browse Trips] (as Traveler)
   │   ├─► Filter (Location, Budget, Date, Type)
   │   ├─► View Trip Details
   │   └─► Send Join Request
   │
   ├─► [Wait for Approval] (as Traveler)
   │   └─► Organizer Accepts/Rejects
   │
   ├─► [Join Accepted] (Multiple Users)
   │   ├─► Group Created
   │   ├─► Group Chat Activated
   │   ├─► Expense Tracker Initialized
   │   └─► NotificationsSent
   │
   ├─► [Group Collaboration]
   │   ├─► Real-time Chat
   │   ├─► Itinerary Planning
   │   ├─► Vote on Hotels/Routes
   │   ├─► Add Expenses
   │   ├─► Live Location Sharing
   │   └─► Document Sharing
   │
   ├─► [Trip Execution]
   │   ├─► Users Travel Together
   │   └─► Real-time Chat for Updates
   │
   ├─► [Trip Completion]
   │   ├─► Settle Expenses
   │   ├─► Rate Other Users
   │   ├─► Rate Organizer
   │   └─► Give Feedback
   │
   └─► [Profile Updated]
       ├─► Reputation Score Changed
       ├─► Trip Count Increased
       └─► Ready for Next Trip

END: User Back to Dashboard
```

---

**This map provides a complete understanding of TripConnect's architecture, features, data flow, and user interactions.**
