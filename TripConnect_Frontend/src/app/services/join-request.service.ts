import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  JoinRequestResponseDto,
  SendJoinRequestDto
} from '../models/api.types';

@Injectable({ providedIn: 'root' })
export class JoinRequestService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/joinrequest`;

  getRequestsForTrip(tripId: number): Observable<JoinRequestResponseDto[]> {
    return this.http.get<JoinRequestResponseDto[]>(`${this.base}/trip/${tripId}/all`);
  }

  sendJoinRequest(userId: number, tripId: number): Observable<JoinRequestResponseDto> {
    const params = new HttpParams().set('userId', userId);
    const dto: SendJoinRequestDto = { tripId };
    return this.http.post<JoinRequestResponseDto>(this.base, dto, { params });
  }

  acceptRequest(requestId: number, hostId: number): Observable<void> {
    const params = new HttpParams().set('hostId', hostId);
    return this.http.put<void>(`${this.base}/${requestId}/accept`, null, { params });
  }

  rejectRequest(requestId: number, hostId: number): Observable<void> {
    const params = new HttpParams().set('hostId', hostId);
    return this.http.put<void>(`${this.base}/${requestId}/reject`, null, { params });
  }

  checkHasRequested(userId: number, tripId: number): Observable<boolean> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('tripId', tripId);
    return this.http.get<boolean>(`${this.base}/check-request`, { params });
  }

  getUserRequests(userId: number): Observable<JoinRequestResponseDto[]> {
    return this.http.get<JoinRequestResponseDto[]>(`${this.base}/user/${userId}`);
  }
}
