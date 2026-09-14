import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  GenerateContentDto,
  GeneratedContentDto,
  GenerateItineraryDto,
  GeneratedItineraryDto,
  GenerateBannerDto,
  GeneratedBannerDto
} from '../models/api.types';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/ai`;

  generateTripContent(dto: GenerateContentDto): Observable<GeneratedContentDto> {
    return this.http.post<GeneratedContentDto>(`${this.base}/generate-trip-content`, dto);
  }

  generateItinerary(dto: GenerateItineraryDto): Observable<GeneratedItineraryDto> {
    return this.http.post<GeneratedItineraryDto>(`${this.base}/generate-itinerary`, dto);
  }

  generateBanner(dto: GenerateBannerDto): Observable<GeneratedBannerDto> {
    return this.http.post<GeneratedBannerDto>(`${this.base}/generate-banner`, dto);
  }
}