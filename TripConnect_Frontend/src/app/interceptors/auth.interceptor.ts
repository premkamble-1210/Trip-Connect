import {
  HttpBackend,
  HttpErrorResponse,
  HttpInterceptorFn
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const EXTERNAL_HOSTS = ['nominatim.openstreetmap.org', 'api.openrouteservice.org'];
const SKIP_AUTH_URLS = ['/api/user/login', '/api/user/register', '/api/user/refresh-token'];
const RETRIED_HEADER = 'x-tc-refreshed';

let refreshPromise: Promise<boolean> | null = null;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isExternal = EXTERNAL_HOSTS.some(host => req.url.includes(host));
  const isAuthEndpoint = SKIP_AUTH_URLS.some(url => req.url.includes(url));

  const token = localStorage.getItem('tc_token');
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  if (isExternal || isAuthEndpoint) {
    return next(authReq);
  }

  const authService = inject(AuthService);
  const router = inject(Router);

  const redirectToLogin = () =>
    router.navigate(['/login'], { queryParams: { expired: true } });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || !token || req.headers.has(RETRIED_HEADER)) {
        return throwError(() => error);
      }

      const refresh = () => {
        if (!refreshPromise) {
          refreshPromise = authService.refreshAccessToken().finally(() => {
            refreshPromise = null;
          });
        }
        return refreshPromise;
      };

      return from(refresh()).pipe(
        switchMap(success => {
          if (!success) {
            redirectToLogin();
            return throwError(() => error);
          }
          const newToken = localStorage.getItem('tc_token') ?? '';
          const retried = authReq.clone({
            setHeaders: { Authorization: `Bearer ${newToken}`, [RETRIED_HEADER]: 'true' }
          });
          return next(retried).pipe(
            catchError((finalError: HttpErrorResponse) => {
              if (finalError.status === 401) {
                authService.clearSession();
                redirectToLogin();
              }
              return throwError(() => finalError);
            })
          );
        })
      );
    })
  );
};