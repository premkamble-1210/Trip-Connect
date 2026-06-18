import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of, catchError, map } from 'rxjs';
import { Coordinate } from '../models/trip-map.models';
import { environment } from '../../../../environments/environment';

const INTERPOLATION_POINTS = 80;

@Injectable({ providedIn: 'root' })
export class RouteService {
  private readonly http = inject(HttpClient);
  private readonly orsUrl = 'https://api.openrouteservice.org/v2/directions/driving-car/geojson';

  getRoute(waypoints: Coordinate[]): Observable<Coordinate[]> {
    if (waypoints.length < 2) return of([]);

    const key = (environment as any).openRouteServiceApiKey as string | undefined;
    if (key) {
      return this.fetchOrsRoute(waypoints, key);
    }
    return of(this.interpolateStraightLine(waypoints));
  }

  private fetchOrsRoute(waypoints: Coordinate[], apiKey: string): Observable<Coordinate[]> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${apiKey}`,
      'Content-Type': 'application/json',
    });

    const coordinates = waypoints.map(c => [c.lng, c.lat]);
    const body = { coordinates };

    return this.http.post<any>(this.orsUrl, body, { headers }).pipe(
      map(res => {
        const coords: [number, number][] = res.features[0].geometry.coordinates;
        return coords.map(([lng, lat]) => ({ lat, lng }));
      }),
      catchError(err => {
        console.error('[ORS] Route fetch failed:', err?.status, err?.error ?? err);
        return of(this.interpolateStraightLine(waypoints));
      }),
    );
  }

  /** Linear interpolation between each waypoint pair */
  private interpolateStraightLine(waypoints: Coordinate[]): Coordinate[] {
    const result: Coordinate[] = [];
    for (let i = 0; i < waypoints.length - 1; i++) {
      const start = waypoints[i];
      const end = waypoints[i + 1];
      for (let t = 0; t <= INTERPOLATION_POINTS; t++) {
        const ratio = t / INTERPOLATION_POINTS;
        result.push({
          lat: start.lat + (end.lat - start.lat) * ratio,
          lng: start.lng + (end.lng - start.lng) * ratio,
        });
      }
    }
    return result;
  }
}
