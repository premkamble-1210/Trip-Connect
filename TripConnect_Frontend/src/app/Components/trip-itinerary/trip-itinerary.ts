import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ItineraryDay {
  day: string;
  location?: string;
  dateString: string;
  description?: string;
  image?: string;
}

@Component({
  selector: 'app-trip-itinerary',
  imports: [CommonModule],
  templateUrl: './trip-itinerary.html',
  styleUrls: ['./trip-itinerary.css']
})
export class TripItinerary {
  @Input() itinerary: ItineraryDay[] = [];

  get displayItinerary(): ItineraryDay[] {
    return this.itinerary;
  }
}
