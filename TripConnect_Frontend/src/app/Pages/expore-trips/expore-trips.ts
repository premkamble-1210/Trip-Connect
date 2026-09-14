import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { TripCard } from '../../Components/trip-card/trip-card';
import { TripService } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';
import { TripResponseDto } from '../../models/api.types';
import { Observable, Subject, debounceTime, finalize, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

interface DateFilterOption {
  label: string;
  days: number;
  checked: boolean;
}

@Component({
  selector: 'app-expore-trips',
  imports: [CommonModule, TripCard],
  templateUrl: './expore-trips.html',
  styleUrl: './expore-trips.css'
})
export class ExporeTrips implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  location = signal<string>('');
  budgetMin = signal<number>(30);
  budgetMax = signal<number>(1400);
  tripType = signal<string>('');
  startDate = signal<string | undefined>(undefined);
  endDate = signal<string | undefined>(undefined);

  trips = signal<TripResponseDto[]>([]);
  recommendedTrips = signal<TripResponseDto[]>([]);
  isLoading = signal(false);
  isLoadingMore = signal(false);
  hasMore = signal(true);
  private readonly authService = inject(AuthService);

  private budgetFilterActive = false;
  private page = 1;
  private readonly pageSize = 20;

  private readonly refreshTrips$ = new Subject<void>();

  locations: string[] = ['', 'Bali', 'Alps', 'New York City', 'Tokyo', 'Paris'];
  tripTypes: string[] = ['', 'Business', 'Leisure', 'Adventure', 'Cultural', 'Wellness'];
  datesFilter: DateFilterOption[] = [
    { label: 'Next 7 days', days: 7, checked: false },
    { label: 'Next 30 days', days: 30, checked: false }
  ];

  constructor(private readonly tripService: TripService) {}

  ngOnInit(): void {
    this.refreshTrips$
      .pipe(
        debounceTime(250),
        switchMap(() => this.fetchPage()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (data) => this.applyPage(data),
        error: (_error: HttpErrorResponse) => {
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
          this.trips.set([]);
          this.hasMore.set(false);
        }
      });

    this.reload();
    this.loadRecommendations();
  }

  private loadRecommendations(): void {
    const userId = this.authService.getCurrentUserId();
    if (!userId) return;
    this.tripService.getRecommendations(userId).subscribe({
      next: (trips) => this.recommendedTrips.set(trips),
      error: (_error) => this.recommendedTrips.set([])
    });
  }

  private reload(): void {
    this.page = 1;
    this.trips.set([]);
    this.hasMore.set(true);
    this.refreshTrips$.next();
  }

  loadMore(): void {
    if (this.isLoading() || this.isLoadingMore() || !this.hasMore()) return;
    this.page += 1;
    this.refreshTrips$.next();
  }

  private fetchPage(): Observable<TripResponseDto[]> {
    this.isLoading.set(this.page === 1);
    this.isLoadingMore.set(this.page > 1);

    const request$ = this.hasActiveFilters()
      ? this.tripService.searchTrips(this.buildFilters())
      : this.tripService.getAllTrips(this.page, this.pageSize);

    return request$.pipe(
      finalize(() => {
        this.isLoading.set(false);
        this.isLoadingMore.set(false);
      })
    );
  }

  private applyPage(data: TripResponseDto[]): void {
    const fresh = data;
    this.trips.update(existing => {
      if (this.page === 1) return fresh;
      const seen = new Set(existing.map(trip => trip.id));
      const appended = fresh.filter(trip => !seen.has(trip.id));
      return [...existing, ...appended];
    });
    this.hasMore.set(fresh.length >= this.pageSize);
  }

  private hasActiveFilters(): boolean {
    return !!(this.location() || this.tripType() || this.startDate() || this.budgetFilterActive);
  }

  private buildFilters() {
    return {
      location: this.location() || undefined,
      startDate: this.startDate(),
      endDate: this.endDate(),
      minBudget: this.budgetFilterActive ? this.budgetMin() : undefined,
      maxBudget: this.budgetFilterActive ? this.budgetMax() : undefined,
      travelType: this.tripType() || undefined
    };
  }

  updateLocation(event: Event): void {
    this.location.set((event.target as HTMLSelectElement).value);
    this.reload();
  }

  updateTripType(event: Event): void {
    this.tripType.set((event.target as HTMLSelectElement).value);
    this.reload();
  }

  updateBudget(event: Event, which: 'min' | 'max'): void {
    const target = event.target as HTMLInputElement;
    const val = parseInt(target.value, 10);
    if (which === 'min' && val <= this.budgetMax()) {
      this.budgetMin.set(val);
    } else if (which === 'min') {
      target.value = this.budgetMin().toString();
      return;
    } else if (which === 'max' && val >= this.budgetMin()) {
      this.budgetMax.set(val);
    } else {
      target.value = this.budgetMax().toString();
      return;
    }
    this.budgetFilterActive = true;
    this.reload();
  }

  updateDates(event: Event, index: number): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.datesFilter.forEach((option, i) => {
      option.checked = i === index ? checked : false;
    });

    if (checked) {
      const option = this.datesFilter[index];
      const from = new Date();
      const to = new Date();
      to.setDate(to.getDate() + option.days);
      this.startDate.set(this.toDateString(from));
      this.endDate.set(this.toDateString(to));
    } else {
      this.startDate.set(undefined);
      this.endDate.set(undefined);
    }
    this.reload();
  }

  // Map TripResponseDto to the shape TripCard expects
  toCardTrip(trip: TripResponseDto) {
    return {
      id: trip.id,
      title: trip.title,
      image: trip.imgUrl ?? '',
      dates: `${new Date(trip.startDate).toLocaleDateString()} - ${new Date(trip.endDate).toLocaleDateString()}`,
      status: trip.status,
      price: trip.budget,
      hostName: trip.hostName ?? 'Organizer',
      memberCount: trip.memberCount ?? 0,
      openSeats: trip.openSeats ?? Math.max(0, trip.seats - (trip.filledSeats ?? 0))
    };
  }

  private toDateString(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}