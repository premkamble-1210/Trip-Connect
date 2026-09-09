import {
  Component,
  OnInit,
  OnDestroy,
  AfterViewInit,
  signal,
  inject,
  NgZone,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Subscription, from, of } from 'rxjs';
import { concatMap, delay, switchMap, toArray } from 'rxjs/operators';

import { MapService } from '../../services/map.service';
import { GeocodingService } from '../../services/geocoding.service';
import { RouteService } from '../../services/route.service';
import { Trip, TripDay, Coordinate } from '../../models/trip-map.models';
import { TripDaySelectorComponent } from '../../components/trip-day-selector/trip-day-selector.component';
import { TripSidebarComponent } from '../../components/trip-sidebar/trip-sidebar.component';
import { TripService } from '../../../../services/trip.service';

const DAY_COLORS = ['#0e7c7b', '#e76f51', '#5c7a02', '#f39c12', '#9b59b6'];
const ANIMATION_INTERVAL_MS = 50;

const KONKAN_TRIP: Trip = {
  id: 'konkan-1',
  title: 'Konkan Road Trip',
  days: [
    { day: 1, source: 'Pune', destination: 'Harihareshwar', color: DAY_COLORS[0] },
    { day: 2, source: 'Harihareshwar', destination: 'Dapoli', color: DAY_COLORS[1] },
    { day: 3, source: 'Dapoli', destination: 'Ratnagiri', color: DAY_COLORS[2] },
  ],
};

interface TripWaypoint {
  day: number;
  location: string;
  lat?: number;
  lng?: number;
}

@Component({
  selector: 'app-trip-map',
  standalone: true,
  imports: [CommonModule, TripDaySelectorComponent, TripSidebarComponent],
  templateUrl: './trip-map.component.html',
  styleUrl: './trip-map.component.css',
})
export class TripMapComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly mapService = inject(MapService);
  private readonly geocoding = inject(GeocodingService);
  private readonly routeService = inject(RouteService);
  private readonly tripService = inject(TripService);
  private readonly route = inject(ActivatedRoute);
  private readonly zone = inject(NgZone);

  trip = KONKAN_TRIP;
  selectedDayIndex = signal(0);
  loading = signal(false);
  progress = signal(0);
  sidebarOpen = signal(true);
  autoPlay = signal(false);
  errorMessage = signal('');
  isTripMode = signal(false);
  tripTitle = signal('');

  private animationTimer: ReturnType<typeof setInterval> | null = null;
  private routeSub: Subscription | null = null;
  private currentRouteCoords: Coordinate[] = [];
  private animationStep = 0;
  private waypoints: TripWaypoint[] = [];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isTripMode.set(true);
      this.tripService.getTripById(Number(id)).subscribe({
        next: apiTrip => {
          this.tripTitle.set(apiTrip.title);
          const sorted = [...apiTrip.tripDays].sort((a, b) => a.day - b.day);
          this.waypoints = sorted.map(d => ({
            day: d.day,
            location: d.location ?? `Day ${d.day}`,
            lat: d.latitude ?? undefined,
            lng: d.longitude ?? undefined,
          }));
          // Build sidebar-compatible trip model (source = day label, destination = location)
          this.trip = {
            id: String(apiTrip.id),
            title: apiTrip.title,
            days: sorted.map((d, i) => ({
              day: d.day,
              source: `Day ${d.day}`,
              destination: d.location ?? `Day ${d.day}`,
              color: DAY_COLORS[i % DAY_COLORS.length],
            })),
          };
          // Load the full route after the map is ready
          if (this.mapService.getMap()) {
            this.loadFullRoute();
          }
        },
        error: () => {
          this.errorMessage.set('Failed to load trip data.');
        },
      });
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.zone.runOutsideAngular(() => {
        this.mapService.initMap('trip-map-container');
      });
      if (this.isTripMode()) {
        if (this.waypoints.length) {
          this.loadFullRoute();
        }
      } else {
        this.loadDay(0);
      }
    }, 0);
  }

  ngOnDestroy(): void {
    this.stopAnimation();
    this.routeSub?.unsubscribe();
  }

  onDaySelected(index: number): void {
    if (this.isTripMode()) return; // no per-day selection in full-trip mode
    if (index === this.selectedDayIndex()) return;
    this.selectedDayIndex.set(index);
    this.stopAnimation();
    this.progress.set(0);
    this.loadDay(index);
  }

  toggleSidebar(): void {
    this.sidebarOpen.set(!this.sidebarOpen());
  }

  toggleAutoPlay(): void {
    if (this.autoPlay()) {
      this.autoPlay.set(false);
      this.stopAnimation();
    } else {
      this.autoPlay.set(true);
      this.startAnimation();
    }
  }

  playAnimation(): void {
    this.startAnimation();
  }

  pauseAnimation(): void {
    this.stopAnimation();
  }

  private loadFullRoute(): void {
    if (!this.waypoints.length) return;

    this.loading.set(true);
    this.errorMessage.set('');
    this.routeSub?.unsubscribe();

    const coordObs = from(this.waypoints).pipe(
      concatMap(wp => {
        if (wp.lat != null && wp.lng != null) {
          return of({ lat: wp.lat, lng: wp.lng } as Coordinate);
        }
        return this.geocoding.geocodeLocation(wp.location).pipe(delay(600));
      }),
      toArray(),
    );

    this.routeSub = coordObs.subscribe({
      next: coords => {
        this.zone.runOutsideAngular(() => {
          this.mapService.clearRouteLayers();
        });

        this.zone.runOutsideAngular(() => {
          this.mapService.addStartMarker(coords[0], this.waypoints[0].location);
          for (let i = 1; i < coords.length - 1; i++) {
            this.mapService.addDayWaypointMarker(coords[i], i + 1, this.waypoints[i].location);
          }
          if (coords.length > 1) {
            this.mapService.addEndMarker(
              coords[coords.length - 1],
              this.waypoints[coords.length - 1].location,
            );
          }
        });

        this.routeSub = this.routeService.getRoute(coords).subscribe(routeCoords => {
          this.currentRouteCoords = routeCoords;

          this.zone.runOutsideAngular(() => {
            this.mapService.drawPolyline(routeCoords, '#e76f51');
            this.mapService.fitBounds(coords);
            this.mapService.createVehicleMarker(routeCoords[0]);
          });

          this.loading.set(false);
        });
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load route. Please try again.');
      },
    });
  }

  private loadDay(index: number): void {
    const day = this.trip.days[index];
    if (!day) return;

    this.loading.set(true);
    this.errorMessage.set('');
    this.routeSub?.unsubscribe();

    const allLocations = [day.source, ...(day.stops ?? []), day.destination];

    const geocodeObs = from(allLocations).pipe(
      switchMap(loc => this.geocoding.geocodeLocation(loc)),
      toArray(),
    );

    this.routeSub = geocodeObs.subscribe({
      next: coords => {
        this.zone.runOutsideAngular(() => {
          this.mapService.clearRouteLayers();
        });

        const [sourceCoord, ...rest] = coords;
        const destCoord = rest[rest.length - 1];
        const stopCoords = rest.slice(0, rest.length - 1);

        day.sourceCoord = sourceCoord;
        day.destCoord = destCoord;
        day.stopsCoords = stopCoords;

        this.zone.runOutsideAngular(() => {
          this.mapService.addStartMarker(sourceCoord, day.source);
          stopCoords.forEach((sc, i) => {
            this.mapService.addStopMarker(sc, (day.stops ?? [])[i] ?? `Stop ${i + 1}`);
          });
          this.mapService.addEndMarker(destCoord, day.destination);
        });

        this.routeSub = this.routeService.getRoute(coords).subscribe(routeCoords => {
          day.routeCoordinates = routeCoords;
          this.currentRouteCoords = routeCoords;

          this.zone.runOutsideAngular(() => {
            this.mapService.drawPolyline(routeCoords, day.color ?? DAY_COLORS[0]);
            this.mapService.fitBounds([sourceCoord, ...stopCoords, destCoord]);
            this.mapService.createVehicleMarker(routeCoords[0]);
          });

          this.loading.set(false);

          if (this.autoPlay()) {
            this.startAnimation();
          }
        });
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load route. Please try again.');
      },
    });
  }

  private startAnimation(): void {
    this.stopAnimation();
    if (!this.currentRouteCoords.length) return;

    this.animationStep = 0;
    const total = this.currentRouteCoords.length;

    this.animationTimer = setInterval(() => {
      if (this.animationStep >= total - 1) {
        this.stopAnimation();
        this.zone.run(() => {
          this.progress.set(100);
          if (this.autoPlay() && !this.isTripMode()) {
            this.advanceToNextDay();
          } else if (this.autoPlay()) {
            this.autoPlay.set(false);
          }
        });
        return;
      }

      this.animationStep++;
      const coord = this.currentRouteCoords[this.animationStep];
      this.mapService.moveVehicleMarker(coord);

      const p = Math.round((this.animationStep / (total - 1)) * 100);
      this.zone.run(() => this.progress.set(p));
    }, ANIMATION_INTERVAL_MS);
  }

  private stopAnimation(): void {
    if (this.animationTimer !== null) {
      clearInterval(this.animationTimer);
      this.animationTimer = null;
    }
  }

  private advanceToNextDay(): void {
    const next = this.selectedDayIndex() + 1;
    if (next < this.trip.days.length) {
      setTimeout(() => {
        this.onDaySelected(next);
      }, 800);
    } else {
      this.autoPlay.set(false);
    }
  }
}
