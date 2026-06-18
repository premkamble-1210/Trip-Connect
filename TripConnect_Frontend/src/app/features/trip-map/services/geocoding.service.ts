import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of, from, concatMap, delay, map, catchError } from 'rxjs';
import { Coordinate } from '../models/trip-map.models';

interface NominatimResult {
  lat: string;
  lon: string;
  display_name: string;
}

export interface LocationSuggestion {
  displayName: string;
  lat: number;
  lng: number;
}

@Injectable({ providedIn: 'root' })
export class GeocodingService {
  private readonly http = inject(HttpClient);
  private readonly cache = new Map<string, Coordinate>();
  private readonly nominatimUrl = 'https://nominatim.openstreetmap.org/search';

  geocodeLocation(name: string): Observable<Coordinate> {
    const normalised = name.trim().toLowerCase();

    if (this.cache.has(normalised)) {
      return of(this.cache.get(normalised)!);
    }

    const headers = new HttpHeaders({ 'Accept-Language': 'en' });
    const url = `${this.nominatimUrl}?q=${encodeURIComponent(name)}&format=json&limit=1`;

    return this.http.get<NominatimResult[]>(url, { headers }).pipe(
      map(results => {
        if (!results.length) throw new Error(`Location not found: ${name}`);
        const coord: Coordinate = {
          lat: parseFloat(results[0].lat),
          lng: parseFloat(results[0].lon),
        };
        this.cache.set(normalised, coord);
        return coord;
      }),
      catchError(() => {
        // Rough fallback to centre of India if geocoding fails
        return of({ lat: 18.5, lng: 73.9 });
      }),
    );
  }

  geocodeAll(names: string[]): Observable<Coordinate> {
    // concatMap + delay to respect Nominatim rate limits (1 req/s)
    return from(names).pipe(concatMap(name => this.geocodeLocation(name).pipe(delay(600))));
  }

  searchSuggestions(query: string): Observable<LocationSuggestion[]> {
    if (query.trim().length < 2) return of([]);
    const headers = new HttpHeaders({ 'Accept-Language': 'en' });
    const url = `${this.nominatimUrl}?q=${encodeURIComponent(query)}&format=json&limit=5`;
    return this.http.get<NominatimResult[]>(url, { headers }).pipe(
      map(results => results.map(r => ({
        displayName: r.display_name,
        lat: parseFloat(r.lat),
        lng: parseFloat(r.lon),
      }))),
      catchError(() => of([])),
    );
  }
}
