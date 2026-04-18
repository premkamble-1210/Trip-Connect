# TripConnect Frontend — Developer Reference

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Tech Stack](#2-tech-stack)
3. [Folder Structure](#3-folder-structure)
4. [Entry Points](#4-entry-points)
5. [Routing](#5-routing)
6. [Components](#6-components)
   - [Shared Components](#61-shared-components)
   - [Page Components](#62-page-components)
7. [Services](#7-services)
8. [State Management](#8-state-management)
9. [Type Definitions / Interfaces](#9-type-definitions--interfaces)
10. [Styling Approach](#10-styling-approach)
11. [Configuration Files](#11-configuration-files)
12. [Known Gaps & TODOs](#12-known-gaps--todos)

---

## 1. Project Overview

TripConnect is a social travel platform where users can create group trips, discover existing ones, and manage join requests. The frontend is an **Angular 21 standalone application** that currently serves as a fully styled UI prototype. All data is mocked — no backend integration exists yet.

The app allows users to:
- Browse and filter available trips (`/explore`)
- Create a new trip (`/create`)
- View trip details (`/trip/:id`)
- Manage incoming join requests as a host (`/request/:id`)
- View and edit their own profile (`/profile`)

---

## 2. Tech Stack

| Technology | Version | Role |
|---|---|---|
| Angular | ^21.2.0 | Framework — standalone components, signals |
| TypeScript | ~5.9.2 | Language (strict mode) |
| Tailwind CSS | ^4.1.12 | Utility-first styling (v4 PostCSS setup) |
| RxJS | ~7.8.0 | Reactive utilities (pulled in by Angular, not used directly yet) |
| Angular CLI | ^21.2.3 | Build tooling (`@angular/build:application`) |
| Vitest | ^4.0.8 | Unit test runner |
| Prettier | ^3.8.1 | Code formatter |

> Angular 21 uses the modern **standalone component API** — there are no `NgModule` declarations anywhere in this project.

---

## 3. Folder Structure

```
TripConnect_Frontend/
├── angular.json                         # Angular CLI workspace config
├── package.json                         # npm dependencies
├── tsconfig.json                        # Root TS config
├── tsconfig.app.json                    # App-specific TS config
├── tsconfig.spec.json                   # Test-specific TS config
├── .postcssrc.json                      # PostCSS config (wires in Tailwind v4)
├── .prettierrc                          # Prettier formatting rules
├── public/
│   └── favicon.ico
└── src/
    ├── index.html                       # HTML shell — mounts <app-root>
    ├── main.ts                          # Bootstrap entry point
    ├── styles.css                       # Global CSS — only: @import 'tailwindcss'
    └── app/
        ├── app.ts                       # Root component (selector: app-root)
        ├── app.html                     # Root template: <app-navbar> + <router-outlet>
        ├── app.css                      # Empty
        ├── app.config.ts                # Application providers
        ├── app.routes.ts                # Route definitions
        ├── app.spec.ts                  # Root component spec
        │
        ├── Components/                  # Reusable shared components
        │   ├── navbar/
        │   │   ├── navbar.ts / .html / .css / .spec.ts
        │   │   ├── nav-link/
        │   │   │   └── nav-link.ts / .html / .css / .spec.ts
        │   │   └── navbar-profile/
        │   │       └── navbar-profile.ts / .html / .css / .spec.ts
        │   ├── trip-card/
        │   │   └── trip-card.ts / .html / .css / .spec.ts
        │   ├── trip-itinerary/
        │   │   └── trip-itinerary.ts / .html / .css / .spec.ts
        │   └── trip-member-list/
        │       └── trip-member-list.ts / .html / .css / .spec.ts
        │
        └── Pages/                       # Routed page components
            ├── login/
            │   └── login.ts / .html / .css / .spec.ts
            ├── register/
            │   └── register.ts / .html / .css / .spec.ts
            ├── expore-trips/            # ← note: typo in folder name (missing 'l')
            │   └── expore-trips.ts / .html / .css / .spec.ts
            ├── new-trip/
            │   └── new-trip.ts / .html / .css / .spec.ts
            ├── trip/
            │   └── trip.ts / .html / .css / .spec.ts
            ├── join-request/
            │   └── join-request.ts / .html / .css / .spec.ts
            ├── profile/
            │   ├── profile.ts / .html / .spec.ts
            │   ├── profile-tabs.ts / .html
            │   ├── profile.service.ts   # Only service in the app
            │   └── components/
            │       ├── hosted-trips/
            │       │   └── hosted-trips.ts / .html
            │       ├── joined-trips/
            │       │   └── joined-trips.ts / .html
            │       └── reviews/
            │           └── reviews.ts / .html
            └── view-profile/
                ├── view-profile.ts / .html / .spec.ts
                ├── view-profile-tabs.ts / .html
                ├── view-hosted-trips.ts / .html
```

---

## 4. Entry Points

### `src/index.html`
The HTML shell. Contains `<base href="/">` for the Angular router and renders `<app-root>`. Page title: `TripConnectFrontend`.

### `src/main.ts`
Application bootstrap:
```typescript
bootstrapApplication(App, appConfig).catch(err => console.error(err));
```
Uses the modern **standalone bootstrap API** — no `NgModule`.

### `src/app/app.config.ts`
Configures application-level providers:
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes)
  ]
};
```
> **Note:** `provideHttpClient()` is NOT present — HTTP calls are not yet wired up.

### `src/app/app.ts`
Root component (`app-root`). Imports all page and component classes directly via the standalone `imports` array. The template renders only:
```html
<app-navbar />
<router-outlet />
```

### `src/styles.css`
The only global stylesheet — a single line:
```css
@import 'tailwindcss';
```

---

## 5. Routing

Defined in `src/app/app.routes.ts`. Uses Angular's standalone `provideRouter(routes)`.

| Path | Component | Notes |
|---|---|---|
| `/` | — | Redirects to `/explore` |
| `/explore` | `ExporeTrips` | Main browse/discovery page |
| `/create` | `NewTrip` | Trip creation form |
| `/profile` | `Profile` | Logged-in user's profile |
| `/trip/:id` | `Trip` | Trip detail view |
| `/request/:id` | `JoinRequest` | Host's join request management |

### Unrouted Components (exist but have no route)

| Component | Reason |
|---|---|
| `Login` | Imported in `app.ts` but no route registered; form logic is empty |
| `Register` | Same — styled HTML only, no logic, no route |
| `ViewProfile` | Built and imported in `app.ts` but no `/profile/:id` route exists |

---

## 6. Components

### 6.1 Shared Components

Located in `src/app/Components/`.

---

#### `Navbar` — `Components/navbar/navbar.ts`
**Selector:** `app-navbar`

The persistent top navigation bar rendered on every page. Contains:
- App logo / brand name
- A search input field
- Three `NavLink` children: Explore (`/explore`), Create (`/create`), Profile (`/profile`)
- A `NavbarProfile` avatar button

---

#### `NavLink` — `Components/navbar/nav-link/nav-link.ts`
**Selector:** `app-nav-link`

| Input | Type | Description |
|---|---|---|
| `label` | `string` | Display text for the link |
| `route` | `string` | Angular `routerLink` path |

Renders a single navigation link. Uses `routerLinkActive` to apply an active style class when the current route matches.

---

#### `NavbarProfile` — `Components/navbar/navbar-profile/navbar-profile.ts`
**Selector:** `app-navbar-profile`

Displays the user's avatar in the navbar alongside a notification bell icon. The avatar URL is currently hardcoded as a signal:
```typescript
userAvatar = signal("https://images.unsplash.com/...")
```
No inputs. Will need to be wired to auth state when authentication is implemented.

---

#### `TripCard` — `Components/trip-card/trip-card.ts`
**Selector:** `app-trip-card`

| Input | Type | Description |
|---|---|---|
| `trip` | `Trip` | Trip data object (required) |

Renders a card UI for a single trip in the explore grid. Displays:
- Cover image
- Trip title
- Date range
- Status badge (`Upcoming`, `Live`, `Completed`)
- Price per person
- Member avatar stack
- Optional "New" badge (`trip.isNew`)

The `Trip` interface used here (defined locally in `trip-card.ts`) is:
```typescript
interface Trip {
  id: number; title: string; image: string; dates: string;
  status: string; price: number; isNew?: boolean; avatars: string[];
}
```

---

#### `TripItinerary` — `Components/trip-itinerary/trip-itinerary.ts`
**Selector:** `app-trip-itinerary`

| Input | Type | Description |
|---|---|---|
| `itinerary` | `any[]` | Array of itinerary day objects |

Renders a vertical timeline of itinerary days. Each item shows a day label, title, description, and optional image. Uses `any[]` — not yet typed.

---

#### `TripMemberList` — `Components/trip-member-list/trip-member-list.ts`
**Selector:** `app-trip-member-list`

| Input | Type | Description |
|---|---|---|
| `members` | `any[]` | Array of member objects |

Renders a list of trip members with avatar, name, and role badge. Uses `any[]` — not yet typed.

---

### 6.2 Page Components

Located in `src/app/Pages/`.

---

#### `Login` — `Pages/login/login.ts`
**Route:** None (not registered)

Styled HTML login form with username and password fields. The component class is empty — no form binding, no submit logic, no navigation. Authentication is not implemented.

---

#### `Register` — `Pages/register/register.ts`
**Route:** None (not registered)

Styled HTML registration form with name, username, email, phone, and password fields. The component class is empty — same state as Login.

---

#### `ExporeTrips` — `Pages/expore-trips/expore-trips.ts`
**Route:** `/explore`

Main trip discovery page. Layout: left sidebar with filters + right grid of `TripCard` components.

**Filter state (via Angular signals):**
| Signal | Type | Default |
|---|---|---|
| `location` | `signal<string>` | `'Location'` |
| `budgetMin` | `signal<number>` | `30` |
| `budgetMax` | `signal<number>` | `1400` |
| `tripType` | `signal<string>` | `'Tours None'` |

**Trips data:** Hardcoded array of 6 trip objects directly in the component class.  
**Filter interaction:** Budget uses a dual-range slider with `[&::-webkit-slider-thumb]` Tailwind overrides.  
**No API call** — filtering logic is not yet implemented (signals are set but not used to filter the hardcoded array).

---

#### `Trip` — `Pages/trip/trip.ts`
**Route:** `/trip/:id`

Detail page for a single trip. Shows:
- Hero banner image
- Trip title, location, description
- `TripItinerary` component (itinerary days)
- `TripMemberList` component (members)
- Sticky sidebar card with host info, price, dates, seats, and a "Request to Join" button

**Data:** Hardcoded `TripData` object and hardcoded `itinerary`/`members` arrays in the component class.

**TripData interface (local to this file):**
```typescript
enum TripStatus { Planning, Available, Ongoing, Completed }

interface TripData {
  Id: number; Title: string; Description: string; Location: string;
  Budget: number; StartDate: Date; EndDate: Date; Seats: number;
  TravelType: string; Status: TripStatus; HostId: number; CreatedAt: Date;
}
```

---

#### `NewTrip` — `Pages/new-trip/new-trip.ts`
**Route:** `/create`

Full trip creation form. Uses Angular signals for all form fields.

**Form fields (all signals):**

| Signal | Type | Notes |
|---|---|---|
| `title` | `signal<string>` | |
| `description` | `signal<string>` | |
| `location` | `signal<string>` | |
| `budget` | `signal<number \| null>` | |
| `startDate` | `signal<string>` | |
| `endDate` | `signal<string>` | |
| `seats` | `signal<number>` | |
| `imageUrl` | `signal<string>` | |
| `travelType` | `signal<string>` | |
| `errors` | `signal<Record<string, string>>` | Validation error map |
| `travelTypes` | `signal<string[]>` | Dropdown options |
| `minEndDate` | `computed()` | Derived from `startDate` |

**`createTrip()` method:** Validates all fields, populates `errors` signal if invalid. On valid form, currently only calls `console.log()` + `alert()` — **no HTTP call yet**.

---

#### `JoinRequest` — `Pages/join-request/join-request.ts`
**Route:** `/request/:id`

Trip host's dashboard to review incoming join requests. Shows a list of requests filterable by status.

**Interfaces (local):**
```typescript
interface JoinRequestItem {
  id: number; name: string; username: string; avatar: string;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
}

interface FilterOption {
  label: string; value: string; selected: boolean;
}
```

**State:** Plain class properties (not signals). Filter options and request list are hardcoded.

**Methods:**
- `acceptRequest(id: number)` — mutates local array, sets status to `'Accepted'`
- `declineRequest(id: number)` — mutates local array, sets status to `'Rejected'`
- `filterRequests(value: string)` — toggles filter selection in `filterOptions`
- `getFilteredRequests()` — returns requests filtered by selected statuses

No API calls. All mutations are in-memory only.

---

#### `Profile` — `Pages/profile/profile.ts`
**Route:** `/profile`

The logged-in user's own profile page. Shows:
- User avatar, name, handle, star rating, member-since date
- Verification badges (Phone, ID)
- Masked contact info (email/phone)
- Edit modal for updating email and phone (uses `[(ngModel)]` two-way binding)
- `ProfileTabs` component (Hosted / Joined / Reviews tabs)

**Data:** Injects `ProfileService` and calls `getUserProfile()` in `ngOnInit`. Uses `[(ngModel)]` for the edit modal fields `editEmail` and `editPhone`.

---

#### `ProfileTabs` — `Pages/profile/profile-tabs.ts`
**Parent:** `Profile`

Tab switcher component. Manages `activeTab: string` as a plain class property (`'hosted'` | `'joined'` | `'reviews'`). Renders the appropriate sub-component based on the active tab:
- `'hosted'` → `HostedTrips`
- `'joined'` → `JoinedTrips`
- `'reviews'` → `Reviews`

---

#### `HostedTrips` — `Pages/profile/components/hosted-trips/hosted-trips.ts`
**Parent:** `ProfileTabs`

Lists trips hosted by the logged-in user. Fetches data from `ProfileService.getHostedTrips()` in `ngOnInit`. Clicking the "Manage Members" button navigates to `/request/:id` using Angular Router.

---

#### `JoinedTrips` — `Pages/profile/components/joined-trips/joined-trips.ts`
**Parent:** `ProfileTabs` and `ViewProfileTabs` (reused in both)

Lists trips the user has joined. Fetches from `ProfileService.getJoinedTrips()` in `ngOnInit`. Reused in both own profile and view-profile pages.

---

#### `Reviews` — `Pages/profile/components/reviews/reviews.ts`
**Parent:** `ProfileTabs` and `ViewProfileTabs` (reused in both)

Lists reviews received by the user. Fetches from `ProfileService.getReviews()` in `ngOnInit`. Renders star ratings and helpful-count indicators. Reused across both profile views.

---

#### `ViewProfile` — `Pages/view-profile/view-profile.ts`
**Route:** None (no route registered yet — `/profile/:id` is missing)

Read-only view of another user's public profile. Mirrors the `Profile` layout but without any edit capability. Injects `ProfileService` (same mock data as own profile). Uses `ViewProfileTabs` for its tab section.

---

#### `ViewProfileTabs` — `Pages/view-profile/view-profile-tabs.ts`
**Parent:** `ViewProfile`

Tab switcher for the viewed user's profile. Same `activeTab` pattern as `ProfileTabs`. Renders:
- `'hosted'` → `ViewHostedTrips`
- `'joined'` → `JoinedTrips` (shared component)
- `'reviews'` → `Reviews` (shared component)

---

#### `ViewHostedTrips` — `Pages/view-profile/view-hosted-trips.ts`
**Parent:** `ViewProfileTabs`

Read-only version of the hosted trips list for another user's profile. Fetches from `ProfileService.getHostedTrips()`. All action buttons are disabled — no "Manage Members" navigation.

---

## 7. Services

### `ProfileService` — `Pages/profile/profile.service.ts`

The **only service** in the entire frontend. It is:

```typescript
@Injectable({ providedIn: 'root' })
export class ProfileService { ... }
```

Provided at the root level — a singleton shared across all components that inject it.

**Methods:**

| Method | Return Type | Description |
|---|---|---|
| `getUserProfile()` | `User` | Returns a single hardcoded user object (Sarah Peterson) |
| `getHostedTrips()` | `Trip[]` | Returns 9 hardcoded trips hosted by the user |
| `getJoinedTrips()` | `Trip[]` | Returns 6 hardcoded trips the user has joined |
| `getReviews()` | `Review[]` | Returns 10 hardcoded reviews received by the user |

> All methods return **static in-memory data**. There are no `HttpClient` calls. When backend integration is added, these methods should be converted to return `Observable<T>` using `HttpClient`.

**Consumed by:** `Profile`, `ViewProfile`, `HostedTrips`, `JoinedTrips`, `ViewHostedTrips`, `Reviews`

---

## 8. State Management

There is no external state management library. The app uses two approaches:

### Angular Signals (newer components)
Used in `ExporeTrips`, `NewTrip`, `NavbarProfile`, and `App`:
```typescript
// Simple reactive state
location = signal<string>('Location');

// Derived state
minEndDate = computed(() => this.startDate() || this.today);
```

### Plain class properties (older-style components)
Used in `JoinRequest`, `Profile`, `ViewProfile`, `ProfileTabs`, `ViewProfileTabs`:
```typescript
activeTab: string = 'hosted';
requests: JoinRequestItem[] = [...];
```

### ProfileService as data source
`ProfileService` acts as a root-level singleton "store" — but it only holds static mock data. It does not implement observables, BehaviorSubjects, or any reactivity.

---

## 9. Type Definitions / Interfaces

There is no dedicated `types/` or `models/` folder. All interfaces are defined **inline** in the file where they are used, and exported from `profile.service.ts`.

### Exported from `profile.service.ts`

```typescript
interface User {
  id: number;
  name: string;
  handle: string;
  rating: number;
  reviews: number;
  memberSince: string;
  email: string;
  phone: string;
  avatar: string;
  badges: Badge[];
}

interface Badge {
  id: number;
  name: string;
  verified: boolean;
}

// Trip used in profile context (hosted/joined lists)
interface Trip {
  id: number;
  title: string;
  location: string;
  dates: string;
  budget: string;      // string e.g. "$1,400"
  seats: string;       // string e.g. "4/6 available"
  status: 'Upcoming' | 'Completed' | 'Live';
  organizerName: string;
  organizerAvatar: string;
}

interface Review {
  id: number;
  reviewerName: string;
  reviewerAvatar: string;
  reviewDate: string;
  rating: number;
  tripName: string;
  reviewText: string;
  helpfulCount: number;
}
```

### Local to `trip-card.ts` (explore trips — different shape)

```typescript
// Different Trip interface — NOT compatible with profile.service.ts Trip
interface Trip {
  id: number;
  title: string;
  image: string;
  dates: string;
  status: string;
  price: number;       // number, not string
  isNew?: boolean;
  avatars: string[];
}
```

> **Warning:** Two separate `Trip` interfaces exist with incompatible shapes. This needs to be unified into a shared types file.

### Local to `trip.ts` (trip detail page)

```typescript
enum TripStatus { Planning, Available, Ongoing, Completed }

interface TripData {
  Id: number; Title: string; Description: string; Location: string;
  Budget: number; StartDate: Date; EndDate: Date; Seats: number;
  TravelType: string; Status: TripStatus; HostId: number; CreatedAt: Date;
}
```

### Local to `join-request.ts`

```typescript
interface JoinRequestItem {
  id: number;
  name: string;
  username: string;
  avatar: string;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
}

interface FilterOption {
  label: string;
  value: string;
  selected: boolean;
}
```

---

## 10. Styling Approach

All styling is done via **Tailwind CSS v4** utility classes inline in HTML templates.

### Setup
- Tailwind v4 is configured via PostCSS (`.postcssrc.json`), not a `tailwind.config.js`
- `src/styles.css` contains only `@import 'tailwindcss';`
- All component `.css` files are **empty**

### Patterns Used

**Custom colors (arbitrary values):**
```html
<div class="bg-[#f4f7fb] text-[#417cca] rounded-[20px]">
```

**Custom scrollbar hiding:**
```html
<div class="[&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]">
```

**Custom range slider thumb styling:**
```html
<input class="[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-4">
```

**Responsive breakpoints:** `sm:`, `md:`, `lg:` prefixes used throughout.

### Missing Definitions
The following CSS animation classes are referenced in templates but are **not defined anywhere** — they will not render:
- `animate-fadeIn`
- `animate-slideUp`

These need to be added to `styles.css` as custom Tailwind animations or via `@keyframes`.

---

## 11. Configuration Files

### `angular.json`
- Builder: `@angular/build:application` (Vite-based, not Webpack)
- Entry point: `src/main.ts`
- Global styles: `src/styles.css`
- Default output: `dist/trip-connect-frontend`
- No environment file substitutions configured

### `tsconfig.json`
Strict TypeScript settings:
```json
{
  "strict": true,
  "noImplicitOverride": true,
  "noPropertyAccessFromIndexSignature": true,
  "noImplicitReturns": true,
  "target": "ES2022",
  "module": "preserve"
}
```
Angular-specific: `strictTemplates: true`, `strictInjectionParameters: true`.

### `.postcssrc.json`
```json
{ "plugins": { "@tailwindcss/postcss": {} } }
```
This is the Tailwind v4 PostCSS integration — replaces the old `tailwind.config.js` approach.

### `.prettierrc`
Standard Prettier config — enforces consistent code formatting. Run with `npx prettier --write .`.

### Environment Files
**None exist.** There are no `environment.ts` / `environment.prod.ts` files and no `.env` file. An `API_BASE_URL` constant will need to be created when backend integration begins.

---

## 12. Known Gaps & TODOs

The following items are incomplete or missing in the current codebase:

| # | Area | Issue |
|---|---|---|
| 1 | HTTP | `provideHttpClient()` is not configured in `app.config.ts` — no API calls can be made |
| 2 | Auth — Login | `Login` component has no logic; no route; form fields have no binding |
| 3 | Auth — Register | `Register` component has no logic; no route; form fields have no binding |
| 4 | Auth — Guards | No `canActivate` guards on any routes — all pages are publicly accessible |
| 5 | Auth — Interceptor | No `HttpInterceptor` for attaching JWT tokens to API requests |
| 6 | Auth — Storage | No `localStorage`/`sessionStorage` usage for persisting session |
| 7 | Routing | `ViewProfile` has no route — `/profile/:id` path is missing from `app.routes.ts` |
| 8 | Data | All data is hardcoded mock data — no API integration anywhere |
| 9 | NewTrip | `createTrip()` only calls `console.log()` + `alert()` — no HTTP POST |
| 10 | Types | Two incompatible `Trip` interfaces exist — needs a shared types file |
| 11 | Types | `TripItinerary` and `TripMemberList` use `any[]` — need typed interfaces |
| 12 | Styling | `animate-fadeIn` and `animate-slideUp` are referenced but undefined |
| 13 | Naming | Folder typo: `expore-trips/` should be `explore-trips/` |
| 14 | NavbarProfile | User avatar is hardcoded — should come from auth state |
| 15 | Environment | No `environment.ts` / `API_BASE_URL` constant for backend URL |
| 16 | ExporeTrips | Filter signals exist but filtering logic is not applied to the trip list |
