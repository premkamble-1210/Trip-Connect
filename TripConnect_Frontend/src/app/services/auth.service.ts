import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AuthResponseDto,
  LoginUserDto,
  CreateUserDto
} from '../models/api.types';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly base = environment.apiBaseUrl;

  login(username: string, password: string): Observable<AuthResponseDto> {
    const dto: LoginUserDto = { username, password };
    return this.http.post<AuthResponseDto>(`${this.base}/api/user/login`, dto).pipe(
      tap(res => {
        if (res.success) {
          localStorage.setItem('tc_token', res.token);
          localStorage.setItem('tc_userId', String(res.user.id));
          localStorage.setItem('tc_user', JSON.stringify(res.user));
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
    return this.http.post<AuthResponseDto>(`${this.base}/api/user/register`, dto);
  }

  logout(): void {
    localStorage.removeItem('tc_token');
    localStorage.removeItem('tc_userId');
    localStorage.removeItem('tc_user');
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('tc_token');
  }

  getCurrentUserId(): number {
    return Number(localStorage.getItem('tc_userId'));
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('tc_token');
  }
}
