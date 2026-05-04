import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TripService } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';
import { ImageService } from '../../services/image.service';
import { TripDayDto } from '../../models/api.types';

@Component({
  selector: 'app-edit-trip',
  imports: [FormsModule, CommonModule],
  templateUrl: './edit-trip.html',
  styleUrl: './edit-trip.css',
})
export class EditTrip implements OnInit {
  private readonly tripService = inject(TripService);
  private readonly authService = inject(AuthService);
  private readonly imageService = inject(ImageService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  tripId = 0;
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
  isUploadingImage = signal(false);
  isFetching = signal(true);

  travelTypes = signal([
    { value: 'Business', label: 'Business' },
    { value: 'Leisure', label: 'Leisure' },
    { value: 'Adventure', label: 'Adventure' },
    { value: 'Cultural', label: 'Cultural' },
    { value: 'Wellness', label: 'Wellness' }
  ]);

  ngOnInit(): void {
    this.tripId = Number(this.route.snapshot.paramMap.get('id'));
    this.tripService.getTripById(this.tripId).subscribe({
      next: (trip) => {
        this.title.set(trip.title);
        this.description.set(trip.description);
        this.location.set(trip.location);
        this.budget.set(trip.budget);
        this.startDate.set(trip.startDate.split('T')[0]);
        this.endDate.set(trip.endDate.split('T')[0]);
        this.seats.set(trip.seats);
        this.travelType.set(trip.travelType);
        this.imageUrl.set(trip.imgUrl || '');
        this.tripDays.set(
          (trip.tripDays ?? [])
            .slice()
            .sort((a, b) => a.day - b.day)
            .map(td => ({
              day: td.day,
              location: td.location ?? '',
              date: td.date ?? '',
              description: td.description ?? '',
              imgUrl: td.imgUrl ?? ''
            }))
        );
        this.isFetching.set(false);
      },
      error: () => {
        this.isFetching.set(false);
        this.errors.set({ general: 'Failed to load trip data.' });
      }
    });
  }

  addTripDay(): void {
    const existing = this.tripDays();
    this.tripDays.set([...existing, { day: existing.length + 1, location: '', date: '', description: '', imgUrl: '' }]);
  }

  removeTripDay(index: number): void {
    const updated = this.tripDays().filter((_, i) => i !== index);
    this.tripDays.set(updated.map((d, i) => ({ ...d, day: i + 1 })));
  }

  updateTripDay(index: number, field: keyof TripDayDto, value: string | number): void {
    const days = [...this.tripDays()];
    days[index] = { ...days[index], [field]: value };
    this.tripDays.set(days);
  }

  updateTrip(): void {
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

    this.tripService.updateTrip(this.tripId, userId, {
      title: this.title(),
      description: this.description(),
      location: this.location(),
      budget: this.budget()!,
      startDate: this.startDate(),
      endDate: this.endDate(),
      seats: this.seats()!,
      travelType: this.travelType(),
      ImgUrl: this.imageUrl() || undefined,
      tripDays: this.tripDays().length > 0 ? this.tripDays() : undefined
    }).subscribe({
      next: (trip) => {
        this.isLoading.set(false);
        this.router.navigate(['/trip', trip.id]);
      },
      error: () => {
        this.isLoading.set(false);
        this.errors.set({ general: 'Failed to update trip. Please try again.' });
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/trip', this.tripId]);
  }
  deleteTrip(): void {
    if (!confirm('Are you sure you want to delete this trip?')) return;

    const userId = this.authService.getCurrentUserId();
    this.isLoading.set(true);

    this.tripService.deleteTrip(this.tripId, userId).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/explore']);
      },
      error: () => {
        this.isLoading.set(false);
        this.errors.set({ general: 'Failed to delete trip. Please try again.' });
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    // Validate file type - only allow images
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp', 'image/svg+xml'];
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp', '.svg'];
    
    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    
    if (!allowedTypes.includes(file.type) || !allowedExtensions.includes(fileExtension)) {
      // Clear the file input
      input.value = '';
      
      // Set error message
      const currentErrors = this.errors();
      this.errors.set({
        ...currentErrors,
        imageUrl: 'Please select a valid image file (JPG, PNG, GIF, WebP, BMP, SVG)'
      });
      return;
    }

    // Clear any previous error
    const currentErrors = this.errors();
    if (currentErrors['imageUrl']) {
      const { imageUrl, ...restErrors } = currentErrors;
      this.errors.set(restErrors);
    }

    // Check file size (limit to 5MB)
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      input.value = '';
      const currentErrors = this.errors();
      this.errors.set({
        ...currentErrors,
        imageUrl: 'Image file size must be less than 5MB'
      });
      return;
    }

    // Upload to server
    this.isUploadingImage.set(true);
    this.imageService.uploadTripImage(this.tripId, file).subscribe({
      next: (response) => {
        this.isUploadingImage.set(false);
        if (response.success) {
          this.imageUrl.set(response.imageUrl);
          // Clear any previous error
          const currentErrors = this.errors();
          if (currentErrors['imageUrl']) {
            const { imageUrl, ...restErrors } = currentErrors;
            this.errors.set(restErrors);
          }
        } else {
          const currentErrors = this.errors();
          this.errors.set({
            ...currentErrors,
            imageUrl: response.message || 'Failed to upload image'
          });
        }
      },
      error: (error) => {
        this.isUploadingImage.set(false);
        input.value = '';
        const currentErrors = this.errors();
        this.errors.set({
          ...currentErrors,
          imageUrl: 'Failed to upload image. Please try again.'
        });
      }
    });
  }

  onDayFileSelected(event: Event, dayIndex: number): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    // Validate file type - only allow images
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp', 'image/svg+xml'];
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp', '.svg'];
    
    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    
    if (!allowedTypes.includes(file.type) || !allowedExtensions.includes(fileExtension)) {
      // Clear the file input
      input.value = '';
      alert('Please select a valid image file (JPG, PNG, GIF, WebP, BMP, SVG)');
      return;
    }

    // Check file size (limit to 5MB)
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      input.value = '';
      alert('Image file size must be less than 5MB');
      return;
    }

    // Upload to server and update the day's imgUrl
    this.imageService.uploadTripImage(this.tripId, file).subscribe({
      next: (response) => {
        if (response.success) {
          this.updateTripDay(dayIndex, 'imgUrl', response.imageUrl);
        } else {
          input.value = '';
          alert(response.message || 'Failed to upload image. Please try again.');
        }
      },
      error: (error) => {
        input.value = '';
        alert('Failed to upload image. Please try again.');
      }
    });
  }
}
