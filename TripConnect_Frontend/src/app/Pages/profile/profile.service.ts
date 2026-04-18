import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UserResponseDto,
  UpdateUserDto,
  TripResponseDto,
  RatingResponseDto
} from '../../models/api.types';
import { TripService } from '../../services/trip.service';

// Re-export response types so components that import from here continue to compile
export type { UserResponseDto as User, TripResponseDto as Trip, RatingResponseDto as Review };

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly tripService = inject(TripService);
  private readonly base = environment.apiBaseUrl;

  getUserById(id: number): Observable<UserResponseDto> {
    return this.http.get<UserResponseDto>(`${this.base}/api/user/${id}`);
  }

  updateUser(id: number, dto: UpdateUserDto): Observable<UserResponseDto> {
    return this.http.put<UserResponseDto>(`${this.base}/api/user/${id}`, dto);
  }

  getHostedTrips(userId: number): Observable<TripResponseDto[]> {
    return this.tripService.getHostedTrips(userId);
  }

  getJoinedTrips(userId: number): Observable<TripResponseDto[]> {
    return this.tripService.getJoinedTrips(userId);
  }

  getReviews(userId: number): Observable<RatingResponseDto[]> {
    return this.http.get<RatingResponseDto[]>(`${this.base}/api/rating/for-user/${userId}`);
  }
}
