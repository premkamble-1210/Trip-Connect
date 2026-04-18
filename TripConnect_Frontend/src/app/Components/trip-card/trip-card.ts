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
  isNew?: boolean;
  avatars: string[];
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

  navigateToTrip(): void {
    this.router.navigate(['/trip', this.trip.id]);
  }
}
