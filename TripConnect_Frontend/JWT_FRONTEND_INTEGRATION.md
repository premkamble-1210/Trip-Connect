# JWT Authentication - Frontend Integration Guide (Angular)

## **Overview**

This guide provides implementation steps for integrating JWT-based authentication in the TripConnect Angular frontend.

---

## **1. HTTP Interceptor Setup**

### **Create JWT Interceptor**

**File:** `src/app/interceptors/jwt.interceptor.ts`

```typescript
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Skip interceptor for auth endpoints
    if (this.isAuthEndpoint(request.url)) {
      return next.handle(request);
    }

    // Add JWT token to request header if available
    const token = this.authService.getAccessToken();
    if (token) {
      request = this.addToken(request, token);
    }

    return next.handle(request).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          return this.handle401Error(request, next);
        } else {
          return throwError(() => error);
        }
      })
    );
  }

  private addToken(request: HttpRequest<any>, token: string) {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const refreshToken = this.authService.getRefreshToken();
      if (refreshToken) {
        return this.authService.refreshToken(refreshToken).pipe(
          switchMap((response: any) => {
            this.isRefreshing = false;
            this.refreshTokenSubject.next(response.accessToken);
            return next.handle(this.addToken(request, response.accessToken));
          }),
          catchError((err) => {
            this.isRefreshing = false;
            this.authService.logout();
            return throwError(() => err);
          })
        );
      }
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(token => {
        return next.handle(this.addToken(request, token));
      })
    );
  }

  private isAuthEndpoint(url: string): boolean {
    return url.includes('/api/user/register') ||
           url.includes('/api/user/login') ||
           url.includes('/api/user/refresh-token') ||
           url.includes('/api/user/check-email') ||
           url.includes('/api/user/check-username');
  }
}
```

### **Register Interceptor in App Module**

**File:** `src/app/app.config.ts` (standalone) or `app.module.ts` (traditional)

**For Standalone Components:**
```typescript
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { JwtInterceptor } from './interceptors/jwt.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... other providers
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }
  ]
};
```

---

## **2. Auth Service Setup**

### **Create/Update Auth Service**

**File:** `src/app/services/auth.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

export interface AuthResponse {
  success: boolean;
  message: string;
  token: {
    accessToken: string;
    refreshToken: string;
    tokenType: string;
    expiresIn: number;
    issuedAt: string;
    expiresAt: string;
  };
  user: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5126/api/user';
  private readonly TOKEN_KEY = 'jwt_token';
  private readonly REFRESH_TOKEN_KEY = 'jwt_refresh_token';
  private readonly USER_KEY = 'current_user';

  private currentUserSubject: BehaviorSubject<any>;
  public currentUser$: Observable<any>;
  private isAuthenticatedSubject: BehaviorSubject<boolean>;
  public isAuthenticated$: Observable<boolean>;

  constructor(private http: HttpClient) {
    const storedUser = this.getStoredUser();
    this.currentUserSubject = new BehaviorSubject<any>(storedUser);
    this.currentUser$ = this.currentUserSubject.asObservable();

    this.isAuthenticatedSubject = new BehaviorSubject<boolean>(this.isTokenValid());
    this.isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  }

  // Authentication Methods
  register(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/register`, credentials).pipe(
      tap(response => {
        if (response.success) {
          this.storeTokens(response.token);
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  login(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/login`, credentials).pipe(
      tap(response => {
        if (response.success) {
          this.storeTokens(response.token);
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  logout(): void {
    // Optional: Call logout endpoint to revoke token on server
    this.http.post(`${this.API_URL}/logout`, {}).subscribe({
      next: () => this.clearTokens(),
      error: () => this.clearTokens() // Clear tokens even if endpoint fails
    });
  }

  refreshToken(refreshToken: string): Observable<any> {
    return this.http.post(`${this.API_URL}/refresh-token`, { refreshToken }).pipe(
      tap(response => {
        this.storeTokens(response);
      })
    );
  }

  // Token Management Methods
  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  storeTokens(tokenResponse: any): void {
    localStorage.setItem(this.TOKEN_KEY, tokenResponse.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, tokenResponse.refreshToken);
  }

  clearTokens(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  // Token Validation
  isTokenValid(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;

    try {
      const payload = this.parseJwt(token);
      const expirationTime = payload.exp * 1000; // Convert to milliseconds
      return expirationTime > Date.now();
    } catch {
      return false;
    }
  }

  private parseJwt(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  }

  getTokenExpirationTime(): number | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = this.parseJwt(token);
      return payload.exp * 1000; // Convert to milliseconds
    } catch {
      return null;
    }
  }

  // User Management
  getCurrentUser(): any {
    return this.currentUserSubject.value;
  }

  setCurrentUser(user: any): void {
    this.currentUserSubject.next(user);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  getStoredUser(): any {
    const storedUser = localStorage.getItem(this.USER_KEY);
    return storedUser ? JSON.parse(storedUser) : null;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value && this.isTokenValid();
  }
}
```

---

## **3. Auth Guard Setup**

### **Create Route Guard**

**File:** `src/app/guards/auth.guard.ts`

```typescript
import { Injectable } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    } else {
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
  }
}
```

### **Apply Guard to Routes**

**File:** `src/app/app.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'trips',
    component: TripsComponent,
    canActivate: [AuthGuard]
  },
  // ... other protected routes
];
```

---

## **4. Login Component Setup**

### **Create Login Component**

**File:** `src/app/pages/login/login.component.ts`

```typescript
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  submitted = false;
  returnUrl: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {
    this.loginForm = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';

    // If already logged in, redirect
    if (this.authService.isAuthenticated()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = null;

    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.router.navigateByUrl(this.returnUrl!);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Login failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
```

---

## **5. Register Component Setup**

### **Create Register Component**

**File:** `src/app/pages/register/register.component.ts`

```typescript
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  submitted = false;
  errorMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.registerForm = this.formBuilder.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      username: ['', Validators.required],
      phone: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = null;

    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;
    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Registration failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
```

---

## **6. Protected API Calls**

### **Using the Interceptor**

The JWT interceptor automatically adds the `Authorization: Bearer <token>` header to all API requests (except auth endpoints).

**Example: Calling a Protected Endpoint**

```typescript
// In any service
constructor(private http: HttpClient) {}

createTrip(tripData: any): Observable<any> {
  // The interceptor automatically adds the JWT token
  return this.http.post('/api/trip', tripData);
}

// No need to manually add Authorization header!
```

---

## **7. Error Handling**

### **Global Error Interceptor**

**File:** `src/app/interceptors/error.interceptor.ts`

```typescript
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expired or unauthorized
          this.authService.logout();
          this.router.navigate(['/login']);
        } else if (error.status === 403) {
          // Forbidden
          console.error('Access denied');
        } else if (error.status === 400) {
          // Bad request
          console.error('Invalid request:', error.error);
        }

        return throwError(() => error);
      })
    );
  }
}
```

---

## **8. Token Refresh Strategy**

### **Automatic Token Refresh Before Expiration**

```typescript
// In auth.service.ts
refreshTokenBeforeExpiration(): void {
  const expirationTime = this.getTokenExpirationTime();
  if (!expirationTime) return;

  // Refresh token 5 minutes before expiration
  const timeUntilExpiration = expirationTime - Date.now() - (5 * 60 * 1000);

  if (timeUntilExpiration > 0) {
    setTimeout(() => {
      const refreshToken = this.getRefreshToken();
      if (refreshToken) {
        this.refreshToken(refreshToken).subscribe();
      }
    }, timeUntilExpiration);
  }
}
```

---

## **9. LocalStorage vs Cookies**

### **Recommended Storage Strategy**

| Storage | Pros | Cons |
|---------|------|------|
| **LocalStorage** | Simple, accessible | XSS vulnerable |
| **SessionStorage** | Cleared on tab close | Still XSS vulnerable |
| **HttpOnly Cookies** | XSS protected | CSRF risk (mitigated with SameSite) |

### **Hybrid Approach (Recommended)**

```typescript
// Access Token: LocalStorage (short-lived, less critical if exposed)
localStorage.setItem('access_token', accessToken);

// Refresh Token: HttpOnly Cookie (more sensitive, server-protected)
// Backend sets: Set-Cookie: refresh_token=...; HttpOnly; Secure; SameSite=Strict
```

---

## **10. Environment Configuration**

### **Setup Environment Files**

**File:** `src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5126',
  apiEndpoints: {
    auth: '/api/user',
    trips: '/api/trip',
    expenses: '/api/expense',
    chat: '/api/chat',
    ratings: '/api/rating',
    joinRequests: '/api/joinrequest'
  }
};
```

**File:** `src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.tripconnect.com',
  apiEndpoints: {
    auth: '/api/user',
    trips: '/api/trip',
    expenses: '/api/expense',
    chat: '/api/chat',
    ratings: '/api/rating',
    joinRequests: '/api/joinrequest'
  }
};
```

---

## **11. Logout Functionality**

### **Complete Logout Flow**

```typescript
logout(): void {
  // Call server to revoke token
  this.http.post(`${this.API_URL}/logout`, {}).subscribe({
    complete: () => this.clearTokensAndRedirect()
  });
}

private clearTokensAndRedirect(): void {
  this.clearTokens(); // Clear localStorage
  this.router.navigate(['/login']);
}
```

---

## **12. Testing**

### **Unit Test Example**

```typescript
describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should store tokens after login', () => {
    const credentials = { username: 'test', password: 'password' };
    const mockResponse = {
      token: {
        accessToken: 'mock_token',
        refreshToken: 'mock_refresh'
      }
    };

    service.login(credentials).subscribe();

    const req = httpMock.expectOne('/api/user/login');
    req.flush(mockResponse);

    expect(localStorage.getItem('jwt_token')).toBe('mock_token');
  });
});
```

---

## **Checklist for Frontend Implementation**

- [ ] Create `JwtInterceptor`
- [ ] Register interceptor in `app.config.ts` or `app.module.ts`
- [ ] Create/Update `AuthService`
- [ ] Create `AuthGuard`
- [ ] Apply guards to protected routes
- [ ] Create `LoginComponent`
- [ ] Create `RegisterComponent`
- [ ] Create `ErrorInterceptor` (optional but recommended)
- [ ] Setup environment configuration
- [ ] Test token refresh flow
- [ ] Test logout functionality
- [ ] Implement token expiration check
- [ ] Add loading indicators during auth requests
- [ ] Handle 401 errors gracefully

---

**Last Updated:** April 21, 2026
**Status:** Ready for Implementation
