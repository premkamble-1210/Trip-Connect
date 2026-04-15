import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-new-trip',
  imports: [FormsModule],
  templateUrl: './new-trip.html',
  styleUrl: './new-trip.css',
})
export class NewTrip {
  title = signal('');
  description = signal('');
  location = signal('');
  budget = signal<number | null>(null);
  
  // Date configuration
  today = new Date().toISOString().split('T')[0];
  startDate = signal('');
  endDate = signal('');
  
  // Enforce end date must be on or after start date
  minEndDate = computed(() => this.startDate() || this.today);

  seats = signal<number | null>(null);
  imageUrl = signal('');
  travelType = signal('');

  // Validation state
  errors = signal<Record<string, string>>({});

  // Dynamic JSON options for Travel Type
  travelTypes = signal([
    { value: 'Business', label: 'Business' },
    { value: 'Leisure', label: 'Leisure' },
    { value: 'Adventure', label: 'Adventure' },
    { value: 'Cultural', label: 'Cultural' },
    { value: 'Wellness', label: 'Wellness' }
  ]);

  createTrip() {
    const errs: Record<string, string> = {};

    if (!this.title().trim()) errs['title'] = 'Title is required';
    if (!this.location().trim()) errs['location'] = 'Location is required';
    if (!this.startDate()) errs['startDate'] = 'Start Date is required';
    if (!this.endDate()) errs['endDate'] = 'End Date is required';
    
    if (this.startDate() && this.endDate()) {
      const start = new Date(this.startDate());
      const end = new Date(this.endDate());
      if (end < start) {
        errs['endDate'] = 'End Date must be after or equal to Start Date';
      }
    }

    if (!this.description().trim()) errs['description'] = 'Description is required';
    if (this.budget() === null || this.budget() === undefined) errs['budget'] = 'Budget is required';
    if (!this.travelType()) errs['travelType'] = 'Travel Type is required';
    if (this.seats() === null || this.seats() === undefined) errs['seats'] = 'Number of seats is required';
    if (!this.imageUrl().trim()) errs['imageUrl'] = 'Image URL is required';

    this.errors.set(errs);

    if (Object.keys(errs).length === 0) {
      console.log('New Trip Data:', {
        Title: this.title(),
        Description: this.description(),
        Location: this.location(),
        Budget: this.budget(),
        StartDate: this.startDate(),
        EndDate: this.endDate(),
        Seats: this.seats(),
        ImageUrl: this.imageUrl(),
        TravelType: this.travelType(),
      });
      
      alert('Trip created successfully!');
    }
  }
}
