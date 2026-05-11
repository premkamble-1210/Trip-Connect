# FRONTEND DOCUMENTATION

## 1. Overview

The **Frontend** is a standalone Angular 21 application built with standalone components, reactive patterns (signals), and Tailwind CSS for styling. It communicates with the TripConnect backend API over HTTP using HttpClient with JWT Bearer authentication.

**Purpose:**
- Provide user interface for trip planning, searching, expense sharing, and community interaction
- Handle user authentication (login/register) with JWT token management
- Display trip listings, details, and management workflows
- Manage user profiles, ratings, and join requests
- Enable trip chat, expense splitting, and image uploads
- Redirect unauthenticated users to login page via route guards

**Key Principles:**
- Standalone component-based architecture (no NgModules)
- Reactive signals for state management (`signal()`, `computed()`)
- HTTP interceptors for automatic JWT token injection
- Route guards for authentication enforcement
- Tailwind CSS for responsive styling
- Environment-based API configuration
- Type-safe API communication via TypeScript interfaces

---

## 2. Project Structure

```
TripConnect_Frontend/
├── angular.json                       # Angular CLI configuration
├── tsconfig.json                      # TypeScript configuration
├── tsconfig.app.json                  # App-specific TypeScript config
├── package.json                       # Dependencies (Angular 21, Tailwind, RxJS)
├── .postcssrc.json                    # PostCSS/Tailwind configuration
├── .prettierrc                        # Code formatting rules
├── src/
│   ├── index.html                     # Bootstrap HTML
│   ├── main.ts                        # Bootstrap application
│   ├── styles.css                     # Global styles + Tailwind imports
│   ├── app/
│   │   ├── app.ts                     # Root component
│   │   ├── app.html                   # Root template
│   │   ├── app.css                    # Root styles
│   │   ├── app.config.ts              # Dependency injection config
│   │   ├── app.routes.ts              # Route definitions
│   │   ├── Services/                  # HTTP client services (4)
│   │   │   ├── auth.service.ts        # Login, register, token management
│   │   │   ├── trip.service.ts        # Trip CRUD and search
│   │   │   ├── image.service.ts       # Image uploads to ImageKit
│   │   │   └── join-request.service.ts# Join request operations
│   │   ├── Interceptors/
│   │   │   └── auth.interceptor.ts    # JWT token injection
│   │   ├── Guards/
│   │   │   └── auth.guard.ts          # Route protection for authenticated users
│   │   ├── Models/
│   │   │   └── api.types.ts           # TypeScript DTOs (single source of truth)
│   │   ├── Components/                # Reusable UI components (4)
│   │   │   ├── navbar/                # Navigation bar with profile menu
│   │   │   ├── trip-card/             # Trip card component for listings
│   │   │   ├── trip-itinerary/        # Trip day/itinerary display
│   │   │   └── trip-member-list/      # Display trip members
│   │   └── Pages/                     # Page-level components (9)
│   │       ├── login/                 # Login form and flow
│   │       ├── register/              # Registration form and flow
│   │       ├── expore-trips/          # Trip discovery with filters
│   │       ├── trip/                  # Trip details and management
│   │       ├── new-trip/              # Create new trip
│   │       ├── edit-trip/             # Edit trip details
│   │       ├── join-request/          # Join request management (for hosts)
│   │       ├── profile/               # User profile and settings
│   │       └── view-profile/          # View other user profiles
│   └── environments/
│       └── environment.ts             # API base URL configuration
├── public/                            # Static assets
└── dist/                              # Production build output
```

---

## 3. Technologies & Dependencies

**Core Framework:**
- **Angular 21.2.0** — Standalone components, signals, dependency injection, routing
- **TypeScript 5.9.2** — Type-safe JavaScript with interfaces and generics
- **RxJS 7.8.0** — Observable streams for async operations

**Styling:**
- **Tailwind CSS 4.1.12** — Utility-first CSS framework
- **PostCSS 8.5.3** — CSS processing
- **@tailwindcss/postcss 4.1.12** — Tailwind integration

**HTTP & Networking:**
- **@angular/common/http** — HttpClient for REST API calls
- **Functional HTTP Interceptors** — JWT token injection

**Development Tools:**
- **@angular/cli 21.2.3** — Build and serve tools
- **Vitest 4.0.8** — Unit test runner
- **jsdom 28.0.0** — DOM simulation for testing
- **Prettier 3.8.1** — Code formatting

---

## 4. Configuration & Startup

**Bootstrap Flow (main.ts):**
```typescript
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
```

**Dependency Injection (app.config.ts):**
```typescript
providers: [
  provideBrowserGlobalErrorListeners(),  // Global error handling
  provideRouter(routes),                 // Routing
  provideHttpClient(
    withInterceptors([authInterceptor])  // HTTP + JWT injection
  )
]
```

**Root Component (app.ts):**
- Imports Navbar and RouterOutlet
- Signals for state management
- Template: `<app-navbar></app-navbar><router-outlet></router-outlet>`

**Environment Configuration:**
```typescript
// environment.ts
export const environment = {
  apiBaseUrl: 'https://localhost:7142'
};
```

---

## 5. Routing

**File:** `app.routes.ts`

**Route Structure:**

| Path | Component | Auth | Purpose |
|------|-----------|------|---------|
| `/login` | Login | ❌ | User login |
| `/register` | Register | ❌ | User registration |
| `/` | (redirect) | — | Redirect to `/explore` |
| `/explore` | ExporeTrips | ❌ | Browse/search trips |
| `/create` | NewTrip | ✅ | Create new trip |
| `/profile` | Profile | ✅ | View/edit own profile |
| `/trip/:id` | Trip | ✅ | View trip details |
| `/edit/:id` | EditTrip | ✅ | Edit trip details |
| `/request/:id` | JoinRequest | ✅ | Manage join requests (host) |
| `/user/:id` | ViewProfile | ✅ | View other user's profile |

**Protected Routes:**
Routes with `canActivate: [authGuard]` require valid JWT token in localStorage. If token missing, user redirected to `/login`.

---

## 6. Authentication Flow

### 6.1 Login

**Component:** Login (`Pages/login/`)

**Flow:**
1. User enters username and password
2. Calls `authService.login(username, password)`
3. Service POSTs to `/api/user/login`
4. On success:
   - Access token stored in `localStorage['tc_token']`
   - Refresh token stored in `localStorage['tc_refresh_token']`
   - User ID stored in `localStorage['tc_userId']`
   - Navigate to `/explore`
5. On error: Display error message

**Form Fields:**
- Username (text input)
- Password (password input)
- Error message display
- Loading state indicator

### 6.2 Registration

**Component:** Register (`Pages/register/`)

**Flow:**
1. User enters name, username, email, phone, password
2. Calls `authService.register(...)`
3. Service POSTs to `/api/user/register`
4. On success:
   - Tokens stored automatically (same as login)
   - Navigate to `/profile` (or `/login` if token not stored)
5. On error: Display error message (e.g., "Username already exists")

**Form Fields:**
- Name (text input)
- Username (text input)
- Email (email input)
- Phone (tel input)
- Password (password input)
- Error message display
- Loading state indicator

### 6.3 Token Management

**AuthService Methods:**

```typescript
login(username, password): Observable<AuthResponseDto>
register(name, username, email, phone, password): Observable<AuthResponseDto>
logout(): Observable<any>
getToken(): string | null
getRefreshToken(): string | null
refreshToken(): Observable<any>
getCurrentUserId(): number
isAuthenticated(): boolean
```

**Token Storage:**
- `tc_token` — Access token (60 min expiry)
- `tc_refresh_token` — Refresh token (7 day expiry)
- `tc_userId` — Current user ID (numeric string)

**Token Injection (authInterceptor):**
```typescript
const token = localStorage.getItem('tc_token');
if (token) {
  req = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
}
return next(req);
```

**Auth Guard (authGuard):**
```typescript
const token = localStorage.getItem('tc_token');
if (token) {
  return true;  // Allow access
}
return inject(Router).createUrlTree(['/login']);  // Redirect to login
```

### 6.4 Logout

**Method:** `authService.logout()`

**Flow:**
1. Send POST request to `/api/user/logout`
2. Clear all tokens from localStorage
3. Navigate to `/login`

---

## 7. HTTP Services

### 7.1 AuthService

**File:** `services/auth.service.ts`

**Dependencies:**
- HttpClient
- Router
- environment (API base URL)

**Methods:**

```typescript
// Login with username/password
login(username: string, password: string): Observable<AuthResponseDto>

// Register new user
register(name, username, email, phone, password): Observable<AuthResponseDto>

// Logout and clear tokens
logout(): Observable<any>

// Get stored access token
getToken(): string | null

// Get stored refresh token
getRefreshToken(): string | null

// Refresh expired access token using refresh token
refreshToken(): Observable<any>

// Get current user ID from localStorage
getCurrentUserId(): number

// Check if user is authenticated
isAuthenticated(): boolean
```

**Logging:**
- Logs token storage/retrieval for debugging
- Console logs indicate success/failure

### 7.2 TripService

**File:** `services/trip.service.ts`

**Base URL:** `{apiBaseUrl}/api/trip`

**Methods:**

```typescript
// Get paginated trips (public)
getAllTrips(pageNumber = 1, pageSize = 20): Observable<TripResponseDto[]>

// Search trips with filters (public)
searchTrips(filters: TripSearchFilters): Observable<TripResponseDto[]>

// Get single trip by ID (protected)
getTripById(id: number): Observable<TripResponseDto>

// Create new trip (protected)
createTrip(userId: number, dto: CreateTripDto): Observable<TripResponseDto>

// Get trips hosted by user
getHostedTrips(userId: number): Observable<TripResponseDto[]>

// Get trips user has joined (via accepted join requests)
getJoinedTrips(userId: number): Observable<TripResponseDto[]>

// Get upcoming trips
getUpcomingTrips(): Observable<TripResponseDto[]>

// Get members of a trip
getTripMembers(tripId: number): Observable<TripMemberResponseDto[]>

// Update trip details (protected)
updateTrip(tripId, userId, dto): Observable<TripResponseDto>

// Delete trip (protected)
deleteTrip(tripId, userId): Observable<void>
```

**Search Filters:**
```typescript
interface TripSearchFilters {
  location?: string;        // Exact location match
  startDate?: string;       // ISO date string
  maxBudget?: number;       // Maximum budget filter
  travelType?: string;      // e.g., "Adventure", "Leisure"
}
```

### 7.3 ImageService

**File:** `services/image.service.ts`

**Base URL:** `{apiBaseUrl}/api/image`

**Methods:**

```typescript
// Upload image for trip
uploadTripImage(tripId: number, file: File): Observable<ImageUploadResponse>

// Upload profile image
uploadProfileImage(file: File): Observable<ImageUploadResponse>

// Upload expense receipt image
uploadExpenseImage(expenseId: number, file: File): Observable<ImageUploadResponse>
```

**Response:**
```typescript
interface ImageUploadResponse {
  fileId: string;           // ImageKit file ID
  imageUrl: string;         // Full image URL with transformations
  publicUrl: string;        // Public image URL
  uploadedAt: string;       // DateTime ISO string
  success: boolean;
  message: string;
  fileSize: number;         // Bytes
  width?: number;           // Image width in pixels
  height?: number;          // Image height in pixels
  fileName: string;         // Original file name
}
```

**Implementation:**
- Converts `File` to `FormData`
- POSTs to `/upload-trip/{tripId}` endpoint
- Backend validates size (5MB max), format (jpg/jpeg/png/webp)

### 7.4 JoinRequestService

**File:** `services/join-request.service.ts`

**Base URL:** `{apiBaseUrl}/api/joinrequest`

**Methods:**

```typescript
// Send join request to trip
sendJoinRequest(userId: number, tripId: number): Observable<JoinRequestResponseDto>

// Get join requests for a trip (host only)
getRequestsForTrip(tripId: number): Observable<JoinRequestResponseDto[]>

// Accept join request (host only)
acceptRequest(requestId: number, hostId: number): Observable<void>

// Reject join request (host only)
rejectRequest(requestId: number, hostId: number): Observable<void>

// Cancel own join request
cancelRequest(requestId: number, userId: number): Observable<void>

// Check if user already requested to join
checkHasRequested(userId: number, tripId: number): Observable<boolean>

// Get requests sent by user
getUserRequests(userId: number): Observable<JoinRequestResponseDto[]>
```

---

## 8. Models & Type Definitions

**File:** `models/api.types.ts` — Single source of truth for all backend DTOs

### 8.1 Authentication Types

```typescript
interface LoginUserDto {
  username: string;
  password: string;
}

interface CreateUserDto {
  name: string;
  email: string;
  username: string;
  phone: string;
  password: string;
}

interface JwtTokenResponseDto {
  accessToken: string;           // JWT Bearer token
  refreshToken: string;          // For token refresh
  tokenType: string;             // "Bearer"
  expiresIn: number;             // Seconds (3600 = 60 min)
  issuedAt: string;              // ISO datetime
  expiresAt: string;             // ISO datetime
}

interface AuthResponseDto {
  success: boolean;
  message: string;
  user: UserResponseDto;         // Authenticated user info
  token: JwtTokenResponseDto;
}
```

### 8.2 User Types

```typescript
interface UserResponseDto {
  id: number;
  name: string;
  username: string;
  email: string;
  phone: string;
  rating: number;                // Average rating (1.0-5.0)
  phoneVerified: boolean;
  idVerified: boolean;
  emailVerified: boolean;
  createdAt: string;             // ISO datetime
}

interface UserFullDto extends UserResponseDto {
  avatarUrl?: string;
  averageRating?: number;
  totalRatings?: number;
}

interface UpdateUserDto {
  name: string;
  phone: string;
}
```

### 8.3 Trip Types

```typescript
interface TripDayDto {
  day: number;
  location?: string;
  date: string;                  // ISO date: "YYYY-MM-DD"
  description?: string;
  imgUrl?: string;
}

interface TripResponseDto {
  id: number;
  title: string;
  description: string;
  location: string;
  budget: number;                // Trip budget
  startDate: string;             // ISO datetime
  endDate: string;               // ISO datetime
  seats: number;                 // Available seats
  travelType: string;            // e.g., "Adventure", "Leisure"
  status: string;                // "Planned", "Ongoing", "Completed", "Cancelled"
  hostId: number;
  hostName: string | null;
  imgUrl: string;                // Hero image URL
  createdAt: string;             // ISO datetime
  tripDays: TripDayResponseDto[];
}

interface CreateTripDto {
  title: string;
  description: string;
  location: string;
  budget: number;
  startDate: string;             // ISO format
  endDate: string;
  seats: number;
  travelType: string;
  ImgUrl: string;                // Capitalized (backend quirk)
  tripDays?: TripDayDto[];
}

interface UpdateTripDto {
  title: string;
  description: string;
  location: string;
  budget: number;
  startDate: string;
  endDate: string;
  seats: number;
  travelType: string;
  ImgUrl?: string;
  tripDays?: TripDayDto[];
}

interface TripSearchFilters {
  location?: string;
  startDate?: string;
  maxBudget?: number;
  travelType?: string;
}
```

### 8.4 Join Request Types

```typescript
interface SendJoinRequestDto {
  tripId: number;
}

interface JoinRequestResponseDto {
  id: number;
  tripId: number;
  userId: number;
  userName: string;
  userUsername?: string;
  userAvatar?: string;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
  requestedAt: string;           // ISO datetime
  respondedAt?: string | null;
}
```

### 8.5 Rating Types

```typescript
interface CreateRatingDto {
  tripId: number;
  ratedUserId: number;
  rating: number;                // 1.0-5.0
  review: string;
}

interface RatingResponseDto {
  id: number;
  tripId: number;
  tripTitle: string;
  raterUserId: number;
  raterUserName: string;
  ratedUserId: number;
  rating: number;
  review: string;
  createdAt: string;
}
```

---

## 9. Components

### 9.1 Root Component (app.ts)

**Selector:** `app-root`
**Imports:** RouterOutlet, Navbar
**Template:**
```html
<app-navbar></app-navbar>
<router-outlet></router-outlet>
```

**Purpose:** Bootstrap component containing navigation and route placeholder

**State:**
```typescript
title = signal('TripConnect');
```

---

### 9.2 Navbar Component

**Selector:** `app-navbar`
**Location:** `Components/navbar/`
**Sub-components:** NavLink, NavbarProfile

**Purpose:** Top navigation bar with:
- TripConnect logo/branding
- Navigation links (Explore, Create, Profile)
- Profile dropdown (user menu, logout)
- Responsive mobile menu

**Template Structure:**
```html
<!-- Logo / Brand -->
<!-- Navigation Links -->
<!-- Profile Menu with Dropdown -->
```

---

### 9.3 Trip Card Component

**Selector:** `app-trip-card`
**Location:** `Components/trip-card/`

**Purpose:** Display trip in grid/list with:
- Hero image
- Title
- Dates
- Budget
- Member avatars
- Status badge

**Inputs:**
```typescript
trip: {
  id: number;
  title: string;
  image: string;
  dates: string;
  status: string;
  price: number;
  avatars: string[];
}
```

**Usage:** `expore-trips` page displays multiple trip cards

---

### 9.4 Trip Itinerary Component

**Selector:** `app-trip-itinerary`
**Location:** `Components/trip-itinerary/`

**Purpose:** Display trip days/schedule in timeline format

**Inputs:**
```typescript
tripDays: TripDayResponseDto[];
```

---

### 9.5 Trip Member List Component

**Selector:** `app-trip-member-list`
**Location:** `Components/trip-member-list/`

**Purpose:** Display trip members with:
- User avatars
- Names
- Roles (Host/Member)
- Join date

**Inputs:**
```typescript
members: TripMemberResponseDto[];
```

---

## 10. Pages

### 10.1 Login Page

**Route:** `/login`
**File:** `Pages/login/login.ts`
**Auth Required:** ❌

**Features:**
- Username and password input
- Error message display
- Loading state during submission
- Link to registration
- Form validation

**Implementation:**
- Uses `FormsModule` for two-way binding
- Signal-based state management
- Calls `authService.login()`
- Navigates to `/explore` on success

**UI:**
```html
<input [(ngModel)]="username()">
<input [(ngModel)]="password()" type="password">
<button (click)="login()" [disabled]="isLoading()">Login</button>
<p *ngIf="errorMessage()">{{ errorMessage() }}</p>
```

### 10.2 Register Page

**Route:** `/register`
**File:** `Pages/register/register.ts`
**Auth Required:** ❌

**Features:**
- Name, username, email, phone, password inputs
- Error message display
- Loading state
- Link to login
- Form validation
- Duplicate username/email handling

**Implementation:**
- Uses `FormsModule`
- Signal-based state
- Calls `authService.register()`
- Navigates to `/profile` on success

### 10.3 Explore Trips Page

**Route:** `/explore`
**File:** `Pages/expore-trips/expore-trips.ts`
**Auth Required:** ❌

**Features:**
- Trip listing with grid layout
- Filter by location (dropdown)
- Filter by trip type (dropdown)
- Budget range slider (min/max)
- Loading state
- Cancelled trips excluded

**Implementation:**
```typescript
location = signal<string>('');
budgetMin = signal<number>(30);
budgetMax = signal<number>(1400);
tripType = signal<string>('');

ngOnInit(): void {
  this.loadTrips();
}

loadTrips(): void {
  const hasFilters = this.location() || this.tripType();
  
  const request$ = hasFilters 
    ? this.tripService.searchTrips({...})
    : this.tripService.getAllTrips();

  request$.subscribe({
    next: (data) => this.trips.set(data),
    error: () => {}
  });
}
```

**UI Components:**
- Location dropdown
- Trip type dropdown
- Budget sliders
- Trip card grid (TripCard components)

### 10.4 Trip Details Page

**Route:** `/trip/:id`
**File:** `Pages/trip/trip.ts`
**Auth Required:** ✅

**Features:**
- Trip title, description, dates, budget
- Hero image
- Trip itinerary (days with activities)
- Member list
- Join request button (if not member)
- Chat messages (if member)
- Expense list (if member)
- Ratings and reviews
- Edit/delete buttons (if host)

**Implementation:**
```typescript
tripId = input<number>();  // From route params
trip = signal<TripResponseDto | null>(null);

ngOnInit(): void {
  const id = this.tripId();
  this.tripService.getTripById(id).subscribe(
    trip => this.trip.set(trip)
  );
}
```

### 10.5 New Trip Page

**Route:** `/create`
**File:** `Pages/new-trip/new-trip.ts`
**Auth Required:** ✅

**Features:**
- Trip title, description, location inputs
- Budget input
- Start/end date pickers
- Available seats input
- Travel type selection
- Hero image upload
- Itinerary day management (add/remove days)
- Submit button

**Implementation:**
- Form-based trip creation
- Image upload via `ImageService`
- Call `tripService.createTrip(userId, dto)`
- Navigate to `/trip/:id` on success

### 10.6 Edit Trip Page

**Route:** `/edit/:id`
**File:** `Pages/edit-trip/edit-trip.ts`
**Auth Required:** ✅

**Features:**
- Load existing trip data
- Edit all trip fields
- Update hero image
- Manage itinerary days
- Cancel/delete trip button

**Implementation:**
- Load trip via `getTripById()`
- Update via `updateTrip(tripId, userId, dto)`
- Delete via `deleteTrip(tripId, userId)`

### 10.7 Join Request Page

**Route:** `/request/:id`
**File:** `Pages/join-request/join-request.ts`
**Auth Required:** ✅

**Features (for trip host):**
- List pending join requests
- Show requester profile preview
- Accept/reject buttons
- Request status display

**Implementation:**
```typescript
tripId = input<number>();

ngOnInit(): void {
  this.joinRequestService.getRequestsForTrip(this.tripId()).subscribe(
    requests => this.requests.set(requests)
  );
}

acceptRequest(requestId: number): void {
  this.joinRequestService.acceptRequest(requestId, this.currentUserId).subscribe(
    () => this.loadRequests()
  );
}
```

### 10.8 Profile Page

**Route:** `/profile`
**File:** `Pages/profile/profile.ts`
**Auth Required:** ✅

**Features:**
- Display current user info
- Edit name/phone
- Profile picture upload
- View hosted trips
- View joined trips
- Rating/reputation score
- Email/phone verification status

**Implementation:**
- Load user from `authService.getCurrentUserId()`
- Update via API
- Display trips lists

### 10.9 View Profile Page

**Route:** `/user/:id`
**File:** `Pages/view-profile/view-profile.ts`
**Auth Required:** ✅

**Features:**
- Display user info (name, avatar, rating)
- User statistics
- Trips hosted/participated
- Ratings and reviews from others

---

## 11. HTTP Interceptor

**File:** `interceptors/auth.interceptor.ts`

**Purpose:** Automatically inject JWT token into all outgoing requests

**Implementation:**
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('tc_token');
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};
```

**How It Works:**
1. Read token from localStorage
2. Clone request and add Authorization header
3. Pass modified request to next interceptor/handler
4. Backend validates JWT signature

**Note:** No custom error handling for 401s (frontend assumes tokens don't expire during session)

---

## 12. Route Guard

**File:** `guards/auth.guard.ts`

**Purpose:** Protect routes that require authentication

**Implementation:**
```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const token = localStorage.getItem('tc_token');
  if (token) {
    return true;
  }
  return inject(Router).createUrlTree(['/login']);
};
```

**Usage in Routes:**
```typescript
{
  path: 'create',
  component: NewTrip,
  canActivate: [authGuard]  // Redirect to /login if no token
}
```

---

## 13. Styling

**Framework:** Tailwind CSS 4.1.12

**Configuration Files:**
- `.postcssrc.json` — PostCSS config for Tailwind
- `tailwind.config.ts` — Tailwind configuration (if exists)
- `src/styles.css` — Global imports and custom animations

**Global Styles (styles.css):**
```css
@import 'tailwindcss';

@keyframes fadeIn {
  from { opacity: 0; }
  to   { opacity: 1; }
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to   { opacity: 1; transform: translateY(0); }
}

.animate-fadeIn {
  animation: fadeIn 0.2s ease-out;
}

.animate-slideUp {
  animation: slideUp 0.25s ease-out;
}
```

**Tailwind Classes Used:**
- `container` — Max-width wrapper
- `grid grid-cols-{n}` — Responsive grid layouts
- `flex justify-{x} items-{y}` — Flexbox alignment
- `bg-{color}` — Background colors
- `text-{color}` — Text colors
- `rounded-lg` — Border radius
- `shadow-md` — Box shadow
- `p-{x} m-{x}` — Padding/margin
- `hover:bg-{color}` — Hover states
- `max-w-{x}` — Max width
- `hidden md:block` — Responsive visibility

**Color Palette:**
- Primary: Blue (`bg-blue-500`, `text-blue-600`)
- Secondary: Gray (`bg-gray-100`, `text-gray-700`)
- Success: Green (`bg-green-500`)
- Danger: Red (`bg-red-500`)
- Warning: Yellow (`bg-yellow-500`)

---

## 14. Development Workflow

### 14.1 Development Server

**Start:**
```bash
npm install          # Install dependencies
npm start            # or: ng serve
```

**Access:** http://localhost:4200

**Auto-reload:** Changes to `.ts`, `.html`, `.css` trigger automatic rebuild

### 14.2 Building for Production

```bash
npm run build        # Builds to dist/ directory
```

**Build Optimization:**
- Output hashing (cache busting)
- Minification
- Tree-shaking (unused code removal)
- Lazy loading

**Production Config:** `angular.json` > `build.configurations.production`

### 14.3 Testing

```bash
npm test             # Run Vitest unit tests
npm run e2e          # End-to-end testing (not configured by default)
```

**Test Setup:**
- Vitest configured via `angular.json`
- jsdom for DOM simulation
- `.spec.ts` file suffix convention

### 14.4 Code Formatting

```bash
npx prettier --write src/      # Auto-format code
```

**Prettier Config:** `.prettierrc`
- Trailing commas
- Semi-colons
- Quote style

---

## 15. State Management

**Pattern:** Angular Signals (fine-grained reactivity)

**Examples:**

```typescript
// Simple signal
const name = signal('John');
name.set('Jane');
console.log(name());  // Output: Jane

// Computed signal (derived state)
const fullName = computed(() => 
  `${firstName()} ${lastName()}`
);

// Array signal
const trips = signal<TripResponseDto[]>([]);
trips.update(current => [...current, newTrip]);
```

**Benefits:**
- No NgModule boilerplate
- Automatic change detection
- Fine-grained reactivity
- Type-safe

---

## 16. Error Handling

**HTTP Errors:**
- 401 Unauthorized → Assumed to not happen (token managed)
- 400 Bad Request → Display backend error message in component
- 404 Not Found → Display "Not found" message
- 500 Server Error → Display generic error message

**Pattern:**
```typescript
this.tripService.getTripById(id).subscribe({
  next: (trip) => this.trip.set(trip),
  error: (err) => {
    console.error('Error fetching trip:', err);
    this.errorMessage.set('Failed to load trip');
  }
});
```

**Error Handling in Services:**
- Services don't catch errors (let components handle)
- Exceptions logged to console for debugging

---

## 17. API Integration Patterns

### 17.1 GET Request (No Body)

```typescript
getTripById(id: number): Observable<TripResponseDto> {
  return this.http.get<TripResponseDto>(`${this.base}/${id}`);
}
```

### 17.2 GET with Query Parameters

```typescript
getAllTrips(page = 1, size = 20): Observable<TripResponseDto[]> {
  const params = new HttpParams()
    .set('pageNumber', page)
    .set('pageSize', size);
  return this.http.get<TripResponseDto[]>(this.base, { params });
}
```

### 17.3 POST with Body

```typescript
login(username: string, password: string): Observable<AuthResponseDto> {
  const dto: LoginUserDto = { username, password };
  return this.http.post<AuthResponseDto>(`${this.base}/login`, dto);
}
```

### 17.4 PUT (Update)

```typescript
updateTrip(tripId: number, userId: number, dto: UpdateTripDto): Observable<TripResponseDto> {
  const params = new HttpParams().set('userId', userId);
  return this.http.put<TripResponseDto>(`${this.base}/${tripId}`, dto, { params });
}
```

### 17.5 DELETE

```typescript
deleteTrip(tripId: number, userId: number): Observable<void> {
  const params = new HttpParams().set('userId', userId);
  return this.http.delete<void>(`${this.base}/${tripId}`, { params });
}
```

### 17.6 FormData (File Upload)

```typescript
uploadTripImage(tripId: number, file: File): Observable<ImageUploadResponse> {
  const formData = new FormData();
  formData.append('imageFile', file);
  return this.http.post<ImageUploadResponse>(`${this.base}/upload-trip/${tripId}`, formData);
}
```

**Note:** HttpClient automatically sets Content-Type header to `multipart/form-data` when FormData is posted

### 17.7 RxJS Operators

**Tap (side effects):**
```typescript
return this.http.post(...).pipe(
  tap(res => {
    if (res.success) {
      localStorage.setItem('tc_token', res.token.accessToken);
    }
  })
);
```

**CatchError (error handling):**
```typescript
return this.http.post(...).pipe(
  catchError(err => {
    console.error(err);
    return throwError(() => err);
  })
);
```

---

## 18. Environment Configuration

**File:** `environments/environment.ts`

```typescript
export const environment = {
  apiBaseUrl: 'https://localhost:7142'
};
```

**Usage:**
```typescript
import { environment } from '../../environments/environment';

const url = `${environment.apiBaseUrl}/api/trip`;
```

**For Production:**
- Create `environment.prod.ts` with production URL
- Use `--configuration production` flag during build

---

## 19. Key Features Checklist

✅ **Authentication**
- Login with username/password
- Register new user
- JWT token storage
- Token injection in requests
- Logout with token cleanup
- Protected routes

✅ **Trip Management**
- Browse all trips
- Search/filter trips (location, budget, type)
- Create new trip
- Edit trip details
- Delete trip
- View trip members
- Manage itinerary days

✅ **Join Requests**
- Send join request
- Accept/reject requests (host)
- View request status
- Check if already requested

✅ **User Profiles**
- View own profile
- Edit profile (name, phone)
- Upload profile picture
- View other user profiles
- Ratings/reputation display

✅ **Image Uploads**
- Upload trip hero image
- Upload profile picture
- Upload expense receipts
- ImageKit integration

✅ **Responsive Design**
- Mobile-friendly layout (Tailwind CSS)
- Desktop optimized
- Tablet compatible

---

## 20. Deployment

### 20.1 Build Production Bundle

```bash
npm run build  # Generates dist/ folder
```

**Output:** `dist/trip-connect-frontend/browser/`

**Files Include:**
- `index.html` — Main HTML file
- `main-*.js` — Main application bundle
- `polyfills-*.js` — Browser compatibility shims
- `*.css` — Compiled Tailwind styles
- Static assets from `public/`

### 20.2 Hosting Options

**Option 1: Netlify**
```bash
npm install -g netlify-cli
netlify deploy --prod --dir dist/trip-connect-frontend/browser
```

**Option 2: Vercel**
```bash
npm install -g vercel
vercel --prod
```

**Option 3: GitHub Pages**
- Set `base href` in `angular.json`
- Build and push `dist/` to `gh-pages` branch

**Option 4: Self-hosted (Nginx/Apache)**
- Copy `dist/` contents to web server
- Configure SPA routing (fallback to index.html)

### 20.3 Environment Variables

```bash
# .env.production
VITE_API_BASE_URL=https://api.tripconnect.com
```

**Update environment.ts:**
```typescript
export const environment = {
  apiBaseUrl: process.env['VITE_API_BASE_URL'] || 'https://localhost:7142'
};
```

---

## 21. Performance Considerations

**Lazy Loading:**
- Routes can be lazy-loaded (if configured)
- Components only loaded when route activated

**Change Detection:**
- Signals provide fine-grained reactivity
- Reduces unnecessary checks

**Bundle Size:**
- Angular 21 with standalone: ~150KB gzipped
- Tailwind CSS: ~10-20KB gzipped
- Total initial: ~160-170KB

**Caching:**
- HTTP caching via Cache-Control headers
- localStorage for tokens (persists across sessions)

---

## 22. Common Tasks

### Create New Page

```bash
ng generate component Pages/new-page-name
# or manually create Pages/new-page-name/ folder with:
# - new-page-name.ts (component)
# - new-page-name.html (template)
# - new-page-name.css (styles)
# - new-page-name.spec.ts (tests)
```

### Create New Service

```bash
ng generate service services/my-service
# or manually create services/my-service.ts with @Injectable decorator
```

### Add Route

Edit `app.routes.ts`:
```typescript
{
  path: 'my-path',
  component: MyComponent,
  canActivate: [authGuard]  // Optional
}
```

### Import Component in Template

```typescript
@Component({
  imports: [MyComponent, CommonModule, FormsModule],
  // ...
})
```

---

## 23. Summary

The **Frontend** provides a modern, responsive user interface for TripConnect with:

1. **Clean Architecture** — Standalone components, services, guards, interceptors
2. **Authentication** — JWT-based login/register with token management
3. **Trip Management** — Create, browse, search, edit, delete trips
4. **Social Features** — Join requests, ratings, user profiles, chat
5. **Image Handling** — Upload and display images via ImageKit
6. **Responsive Design** — Tailwind CSS for all screen sizes
7. **Type Safety** — TypeScript with shared DTO interfaces
8. **State Management** — Angular Signals for reactive updates
9. **HTTP Integration** — Interceptors and guards for API communication
10. **Developer Experience** — Fast development server, hot reload, easy scaffolding

Perfect for group trip planning and expense sharing!

