import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TripService } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';
import { AiService } from '../../services/ai.service';
import { TripDayDto } from '../../models/api.types';
import { LocationAutocompleteComponent, LocationSelectedEvent } from '../../Components/location-autocomplete/location-autocomplete.component';

@Component({
  selector: 'app-new-trip',
  imports: [FormsModule, CommonModule, LocationAutocompleteComponent],
  templateUrl: './new-trip.html',
  styleUrl: './new-trip.css',
})
export class NewTrip {
  private readonly tripService = inject(TripService);
  private readonly authService = inject(AuthService);
  private readonly aiService = inject(AiService);
  private readonly router = inject(Router);

  title = signal('');
  description = signal('');
  location = signal('');
  budget = signal<number | null>(null);

  today = new Date().toISOString().split('T')[0];
  startDate = signal('');
  endDate = signal('');

  minEndDate = computed(() => this.startDate() || this.today);

  seats = signal<number | null>(null);
  imageUrl = signal('');
  travelType = signal('');

  tripDays = signal<TripDayDto[]>([]);
  errors = signal<Record<string, string>>({});
  isLoading = signal(false);
  aiLoading = signal<string | null>(null);
  aiError = signal('');

  travelTypes = signal([
    { value: 'Business', label: 'Business' },
    { value: 'Leisure', label: 'Leisure' },
    { value: 'Adventure', label: 'Adventure' },
    { value: 'Cultural', label: 'Cultural' },
    { value: 'Wellness', label: 'Wellness' }
  ]);

  addTripDay(): void {
    const existing = this.tripDays();
    this.tripDays.set([...existing, { day: existing.length + 1, location: '', date: '', description: '', imgUrl: '', latitude: undefined, longitude: undefined }]);
  }

  removeTripDay(index: number): void {
    const updated = this.tripDays().filter((_, i) => i !== index);
    this.tripDays.set(updated.map((d, i) => ({ ...d, day: i + 1 })));
  }

  updateTripDayLocation(index: number, e: LocationSelectedEvent): void {
    const days = [...this.tripDays()];
    days[index] = { ...days[index], location: e.name, latitude: e.lat, longitude: e.lng };
    this.tripDays.set(days);
  }

  updateTripDay(index: number, field: keyof TripDayDto, value: string | number): void {
    const days = [...this.tripDays()];
    days[index] = { ...days[index], [field]: value };
    this.tripDays.set(days);
  }

  private aiInputsCheck(): string {
    if (!this.location().trim()) return 'Location is required for AI generation';
    if (!this.startDate()) return 'Start Date is required for AI generation';
    if (!this.endDate()) return 'End Date is required for AI generation';
    if (!this.travelType()) return 'Travel Type is required for AI generation';
    return '';
  }

  generateContent(): void {
    const missing = this.aiInputsCheck();
    if (missing) {
      this.aiError.set(missing);
      return;
    }

    this.aiError.set('');
    this.aiLoading.set('content');
    this.aiService.generateTripContent({
      location: this.location().trim(),
      budget: this.budget() ?? 0,
      startDate: this.startDate(),
      endDate: this.endDate(),
      travelType: this.travelType(),
      seats: this.seats() ?? undefined
    }).subscribe({
      next: (result) => {
        this.title.set(result.title);
        this.description.set(result.description);
        this.aiLoading.set(null);
      },
      error: (err) => {
        this.aiLoading.set(null);
        this.aiError.set(err.error?.message ?? 'AI generation failed. Please try again.');
      }
    });
  }

  generateItinerary(): void {
    const missing = this.aiInputsCheck();
    if (missing) {
      this.aiError.set(missing);
      return;
    }

    this.aiError.set('');
    this.aiLoading.set('itinerary');
    this.aiService.generateItinerary({
      location: this.location().trim(),
      budget: this.budget() ?? 0,
      startDate: this.startDate(),
      endDate: this.endDate(),
      travelType: this.travelType()
    }).subscribe({
      next: (result) => {
        this.tripDays.set(result.tripDays ?? []);
        this.aiLoading.set(null);
      },
      error: (err) => {
        this.aiLoading.set(null);
        this.aiError.set(err.error?.message ?? 'AI itinerary generation failed. Please try again.');
      }
    });
  }

  generateBanner(): void {
    if (!this.location().trim()) {
      this.aiError.set('Location is required for banner generation');
      return;
    }

    this.aiError.set('');
    this.aiLoading.set('banner');
    this.aiService.generateBanner({
      title: this.title().trim(),
      location: this.location().trim(),
      description: this.description().trim()
    }).subscribe({
      next: (result) => {
        this.imageUrl.set(result.imgUrl);
        this.aiLoading.set(null);
      },
      error: (err) => {
        this.aiLoading.set(null);
        this.aiError.set(err.error?.message ?? 'Banner generation failed. Please try again.');
      }
    });
  }

  createTrip(): void {
    const errs: Record<string, string> = {};

    if (!this.title().trim()) errs['title'] = 'Title is required';
    if (!this.location().trim()) errs['location'] = 'Location is required';
    if (!this.startDate()) errs['startDate'] = 'Start Date is required';
    if (!this.endDate()) errs['endDate'] = 'End Date is required';

    if (this.startDate() && this.endDate()) {
      const start = new Date(this.startDate());
      const end = new Date(this.endDate());
      if (end < start) errs['endDate'] = 'End Date must be after or equal to Start Date';
    }

    if (!this.description().trim()) errs['description'] = 'Description is required';
    if (this.budget() === null || this.budget() === undefined) errs['budget'] = 'Budget is required';
    if (!this.travelType()) errs['travelType'] = 'Travel Type is required';
    if (this.seats() === null || this.seats() === undefined) errs['seats'] = 'Number of seats is required';

    this.errors.set(errs);
    if (Object.keys(errs).length > 0) return;

    const userId = this.authService.getCurrentUserId();
    this.isLoading.set(true);

    this.tripService.createTrip(userId, {
      title: this.title(),
      description: this.description(),
      location: this.location(),
      budget: this.budget()!,
      startDate: this.startDate(),
      endDate: this.endDate(),
      seats: this.seats()!,
      travelType: this.travelType(),
      ImgUrl: this.imageUrl(),
      tripDays: this.tripDays().length > 0 ? this.tripDays() : undefined
    }).subscribe({
      next: (trip) => {
        this.isLoading.set(false);
        this.router.navigate(['/trip', trip.id]);
      },
      error: () => {
        this.isLoading.set(false);
        this.errors.set({ general: 'Failed to create trip. Please try again.' });
      }
    });
  }
}
