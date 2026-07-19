import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TexttailPipe } from '../../pip/texttail-pipe';
export interface ItineraryDay {
  day: string;
  location?: string;
  dateString: string;
  description?: string;
  image?: string;
}

@Component({
  selector: 'app-trip-itinerary',
  imports: [CommonModule,TexttailPipe],
  templateUrl: './trip-itinerary.html',
  styleUrls: ['./trip-itinerary.css']
})
export class TripItinerary {
  @Input() itinerary: ItineraryDay[] = [];

  get displayItinerary(): ItineraryDay[] {
    return this.itinerary;
  }
}
