import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

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
}
