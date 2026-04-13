import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-trip-itinerary',
  imports: [CommonModule],
  templateUrl: './trip-itinerary.html',
  styleUrl: './trip-itinerary.css'
})
export class TripItinerary {
  @Input() itinerary: any[] = [];
}
