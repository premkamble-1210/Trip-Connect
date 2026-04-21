# JWT Token Storage Fix - Summary

## **Issue Found**
After login, the JWT token was being stored as `null` in localStorage. The token object was being stored instead of the actual access token string.

## **Root Cause**
In `src/app/services/auth.service.ts`, the login method was incorrectly storing the entire token object:

```typescript
// ❌ WRONG - Storing the object
localStorage.setItem('tc_token', res.token);
```

The `res.token` is an object with structure:
```typescript
{
  accessToken: "eyJhbGciOiJIUzI1NiIs...",
  refreshToken: "random-secure-string",
  tokenType: "Bearer",
  expiresIn: 3600,
  issuedAt: "2026-04-21T...",
  expiresAt: "2026-04-21T..."
}
```

## **Fixes Applied**

### **1. Fixed Auth Service Login Method**
```typescript
// ✅ CORRECT - Storing only the accessToken string
localStorage.setItem('tc_token', res.token.accessToken);
localStorage.setItem('tc_refresh_token', res.token.refreshToken);
```

**File:** `TripConnect_Frontend/src/app/services/auth.service.ts`

### **2. Fixed Auth Service Register Method**
Added token storage to the register method (it was missing before):
```typescript
register(...): Observable<AuthResponseDto> {
  // ... 
  .pipe(
    tap(res => {
      if (res.success && res.token) {
        localStorage.setItem('tc_token', res.token.accessToken);
        localStorage.setItem('tc_refresh_token', res.token.refreshToken);
        localStorage.setItem('tc_userId', String(res.user.id));
        localStorage.setItem('tc_user', JSON.stringify(res.user));
      }
    })
  );
}
```

### **3. Added Refresh Token Support**
Added new methods to handle token refresh:
```typescript
getRefreshToken(): string | null {
  return localStorage.getItem('tc_refresh_token');
}

refreshToken(): Observable<any> {
  const refreshToken = this.getRefreshToken();
  return this.http.post<any>(
    `${this.base}/api/user/refresh-token`, 
    { refreshToken }
  ).pipe(
    tap(res => {
      if (res.accessToken) {
        localStorage.setItem('tc_token', res.accessToken);
        if (res.refreshToken) {
          localStorage.setItem('tc_refresh_token', res.refreshToken);
        }
      }
    })
  );
}
```

### **4. Updated Logout Method**
```typescript
logout(): void {
  localStorage.removeItem('tc_token');
  localStorage.removeItem('tc_refresh_token');  // Added
  localStorage.removeItem('tc_userId');
  localStorage.removeItem('tc_user');
  this.router.navigate(['/login']);
}
```

## **Backend Fixes**

### **1. Fixed RegisterAsync Method**
Ensured refresh token is properly stored in the database during registration.

### **2. Added LogoutAsync Implementation**
Implemented the `LogoutAsync` method in UserService:
```csharp
public async Task LogoutAsync(int userId)
{
  var user = await _unitOfWork.Users.GetByIdAsync(userId);
  user.RefreshToken = null;
  user.RefreshTokenExpiryTime = DateTime.MinValue;
  user.IsTokenBlacklisted = true;
  await _unitOfWork.Users.UpdateAsync(user);
  await _unitOfWork.SaveChangesAsync();
}
```

**File:** `TripConnect_Backend/TripConnect/APPLICATION_LAYER/Services/Implementations/UserService.cs`

## **LocalStorage Keys Used**
- `tc_token` - JWT Access Token (short-lived)
- `tc_refresh_token` - Refresh Token (long-lived, stored in DB)
- `tc_userId` - Current user ID
- `tc_user` - User object (JSON)

## **How The Interceptor Works**
The existing auth interceptor in `src/app/interceptors/auth.interceptor.ts` correctly retrieves the token:

```typescript
const token = localStorage.getItem('tc_token');
if (token) {
  req = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
}
return next(req);
```

Now that the token is properly stored, the interceptor will automatically add it to all API requests.

## **Testing After Fix**

1. **Login:** Token should now be stored in `tc_token`
2. **Check DevTools:** Open DevTools → Storage/Application → LocalStorage
   - `tc_token` should contain a JWT string
   - `tc_refresh_token` should contain the refresh token
   - `tc_userId` should contain the user ID
   - `tc_user` should contain user data

3. **API Requests:** All subsequent API calls will automatically include:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ```

## **Files Modified**
- ✅ `TripConnect_Frontend/src/app/services/auth.service.ts` - Fixed token storage
- ✅ `TripConnect_Backend/TripConnect/APPLICATION_LAYER/Services/Implementations/UserService.cs` - Added LogoutAsync, fixed RegisterAsync

## **Status**
🟢 **FIXED** - Token should now be properly stored and sent with API requests
