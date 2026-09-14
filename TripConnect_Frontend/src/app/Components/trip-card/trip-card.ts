import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

export interface Trip {
  id: number;
  title: string;
  image: string;
  dates: string;
  status: string;
  price: number;
  hostName: string;
  memberCount: number;
  openSeats: number;
  isNew?: boolean;
}

@Component({
  selector: 'app-trip-card',
  imports: [CommonModule],
  templateUrl: './trip-card.html',
  styleUrl: './trip-card.css'
})
export class TripCard {
  @Input() trip!: Trip;
  private readonly router = inject(Router);

  hostAvatar(): string {
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(this.trip.hostName || 'Organizer')}`;
  }

  navigateToTrip(): void {
    this.router.navigate(['/trip', this.trip.id]);
  }

  navigateToMap(event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/map', this.trip.id]);
  }
}
