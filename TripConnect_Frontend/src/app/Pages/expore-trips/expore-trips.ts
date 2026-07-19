import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { TripCard } from '../../Components/trip-card/trip-card';
import { TripService } from '../../services/trip.service';
import { TripResponseDto } from '../../models/api.types';
import { Subject, debounceTime, finalize, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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

  trips = signal<TripResponseDto[]>([]);
  isLoading = signal(false);

  private readonly refreshTrips$ = new Subject<void>();

  locations: string[] = ['', 'Bali', 'Alps', 'New York City', 'Tokyo', 'Paris'];
  tripTypes: string[] = ['', 'Business', 'Leisure', 'Adventure', 'Cultural', 'Wellness'];
  datesFilter = [
    { label: 'Next 7 days', checked: false },
    { label: 'Next 30 days', checked: false },
    { label: 'Custom range', checked: false }
  ];

  constructor(private readonly tripService: TripService) {}

  ngOnInit(): void {
    this.refreshTrips$
      .pipe(
        debounceTime(250),
        switchMap(() => {
          const hasFilters = this.location() || this.tripType();
          const request$ = hasFilters
            ? this.tripService.searchTrips({
                location: this.location() || undefined,
                maxBudget: this.budgetMax(),
                travelType: this.tripType() || undefined
              })
            : this.tripService.getAllTrips();

          this.isLoading.set(true);

          return request$.pipe(finalize(() => this.isLoading.set(false)));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (data) => {
          this.trips.set(data.filter(trip => trip.status !== 'Cancelled'));
        },
        error: (_error: HttpErrorResponse) => {
          this.trips.set([]);
        }
      });

    this.loadTrips();
  }

  loadTrips(): void {
    this.refreshTrips$.next();
  }

  updateLocation(event: Event): void {
    this.location.set((event.target as HTMLSelectElement).value);
    this.loadTrips();
  }

  updateTripType(event: Event): void {
    this.tripType.set((event.target as HTMLSelectElement).value);
    this.loadTrips();
  }

  updateBudgetMin(event: Event): void {
    const target = event.target as HTMLInputElement;
    const val = parseInt(target.value, 10);
    if (val <= this.budgetMax()) {
      this.budgetMin.set(val);
    } else {
      target.value = this.budgetMin().toString();
    }
  }

  updateBudgetMax(event: Event): void {
    const target = event.target as HTMLInputElement;
    const val = parseInt(target.value, 10);
    if (val >= this.budgetMin()) {
      this.budgetMax.set(val);
      this.loadTrips();
    } else {
      target.value = this.budgetMax().toString();
    }
  }

  // Map TripResponseDto to the shape TripCard expects
  toCardTrip(trip: TripResponseDto) {
    return {
      id: trip.id,
      title: trip.title,
      image: trip.imgUrl,
      dates: `${new Date(trip.startDate).toLocaleDateString()} - ${new Date(trip.endDate).toLocaleDateString()}`,
      status: trip.status,
      price: trip.budget,
      avatars: this.generateRandomAvatars()
    };
  }

  // Generate random avatars (1-4)
  private generateRandomAvatars(): string[] {
    const randomCount = Math.floor(Math.random() * 4) + 1; // 1 to 4 avatars
    const avatars: string[] = [];
    const names = ['Alice', 'Bob', 'Charlie', 'Diana', 'Eve', 'Frank', 'Grace', 'Henry', 'Iris', 'Jack'];
    
    for (let i = 0; i < randomCount; i++) {
      const randomName = names[Math.floor(Math.random() * names.length)];
      avatars.push(`https://ui-avatars.com/api/?name=${randomName}`);
    }
    
    return avatars;
  }
}
