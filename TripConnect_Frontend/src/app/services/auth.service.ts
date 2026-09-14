import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpBackend } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable, firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AuthResponseDto,
  LoginUserDto,
  CreateUserDto,
  JwtTokenResponseDto
} from '../models/api.types';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly rawHttp = new HttpClient(inject(HttpBackend));
  private readonly base = environment.apiBaseUrl;

  login(username: string, password: string): Observable<AuthResponseDto> {
    const dto: LoginUserDto = { username, password };
    return this.http.post<AuthResponseDto>(`${this.base}/api/user/login`, dto).pipe(
      tap(res => {
        console.log('🔐 Login response received:', res);
        if (res.success) {
          console.log('✅ Success = true');
          if (res.token && res.user) {
            console.log('✅ Token exists');
            localStorage.setItem('tc_token', res.token.accessToken);
            localStorage.setItem('tc_refresh_token', res.token.refreshToken);
            localStorage.setItem('tc_userId', String(res.user.id));
            console.log('💾 Tokens stored:', localStorage.getItem('tc_token') ? 'SUCCESS' : 'FAILED');
          } else {
            console.log('❌ Token or user is null/undefined');
          }
        } else {
          console.log('❌ Success = false, message:', res.message);
        }
      })
    );
  }

  register(
    name: string,
    username: string,
    email: string,
    phone: string,
    password: string
  ): Observable<AuthResponseDto> {
    const dto: CreateUserDto = { name, email, username, phone, password };
    return this.http.post<AuthResponseDto>(`${this.base}/api/user/register`, dto).pipe(
      tap(res => {
        if (res.success && res.token && res.user) {
          localStorage.setItem('tc_token', res.token.accessToken);
          localStorage.setItem('tc_refresh_token', res.token.refreshToken);
          localStorage.setItem('tc_userId', String(res.user.id));
        }
      })
    );
  }

  logout(): Observable<any> {
    return this.http.post<any>(`${this.base}/api/user/logout`, {}).pipe(
      tap(() => {
        console.log('✅ Backend logout successful');
        this.clearSession();
        this.router.navigate(['/login']);
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem('tc_token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('tc_refresh_token');
  }

  refreshAccessToken(): Promise<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return Promise.resolve(false);
    }
    return firstValueFrom(
      this.rawHttp.post<JwtTokenResponseDto>(`${this.base}/api/user/refresh-token`, { refreshToken })
    )
      .then(res => {
        if (res?.accessToken) {
          localStorage.setItem('tc_token', res.accessToken);
          if (res.refreshToken) {
            localStorage.setItem('tc_refresh_token', res.refreshToken);
          }
          return true;
        }
        this.clearSession();
        return false;
      })
      .catch(() => {
        this.clearSession();
        return false;
      });
  }

  clearSession(): void {
    localStorage.removeItem('tc_token');
    localStorage.removeItem('tc_refresh_token');
    localStorage.removeItem('tc_userId');
  }

  getCurrentUserId(): number {
    return Number(localStorage.getItem('tc_userId'));
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('tc_token');
  }
}
