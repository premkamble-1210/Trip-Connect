import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  TripResponseDto,
  CreateTripDto,
  TripSearchFilters
} from '../models/api.types';

@Injectable({ providedIn: 'root' })
export class TripService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/trip`;

  getAllTrips(pageNumber = 1, pageSize = 20): Observable<TripResponseDto[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<TripResponseDto[]>(this.base, { params });
  }

  searchTrips(filters: TripSearchFilters): Observable<TripResponseDto[]> {
    let params = new HttpParams();
    if (filters.location) params = params.set('location', filters.location);
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.maxBudget != null) params = params.set('maxBudget', filters.maxBudget);
    if (filters.travelType) params = params.set('travelType', filters.travelType);
    return this.http.get<TripResponseDto[]>(`${this.base}/search`, { params });
  }

  getTripById(id: number): Observable<TripResponseDto> {
    return this.http.get<TripResponseDto>(`${this.base}/${id}`);
  }

  createTrip(userId: number, dto: CreateTripDto): Observable<TripResponseDto> {
    const params = new HttpParams().set('userId', userId);
    return this.http.post<TripResponseDto>(this.base, dto, { params });
  }

  getHostedTrips(userId: number): Observable<TripResponseDto[]> {
    return this.http.get<TripResponseDto[]>(`${environment.apiBaseUrl}/api/Trip/user/${userId}`);
  }

  getJoinedTrips(userId: number): Observable<TripResponseDto[]> {
    return this.http.get<TripResponseDto[]>(`${environment.apiBaseUrl}/api/Trip/member/${userId}`);
  }

  getUpcomingTrips(): Observable<TripResponseDto[]> {
    return this.http.get<TripResponseDto[]>(`${this.base}/upcoming`);
  }
}
