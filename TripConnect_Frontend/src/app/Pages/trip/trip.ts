import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripMemberList } from '../../Components/trip-member-list/trip-member-list';
import { TripItinerary } from '../../Components/trip-itinerary/trip-itinerary';

export enum TripStatus {
  Planning = 'Planning',
  Available = 'Available',
  Ongoing = 'Ongoing',
  Completed = 'Completed'
}

export interface TripData {
  Id: number;
  Title: string;
  Description: string;
  Location: string;
  Budget: number;
  StartDate: Date;
  EndDate: Date;
  Seats: number;
  TravelType: string;
  Status: TripStatus;
  HostId: number;
  CreatedAt: Date;
}

@Component({
  selector: 'app-trip',
  imports: [CommonModule, TripMemberList, TripItinerary],
  templateUrl: './trip.html',
  styleUrl: './trip.css'
})
export class Trip {
  tripData: TripData = {
    Id: 1,
    Title: 'Explore Bali: Culture & Coastline',
    Description: 'Explore Bali Culture & Coastline. It is an amazing cultural experience mixing context to the high-road destinations, exceptional architecture and ancient rituals. Includes snorkeling options, market visits and expanded relationships to regional developers and expanded in-depth analysis.',
    Location: 'Bali, Indonesia',
    Budget: 1400,
    StartDate: new Date('2023-04-10T11:30:00'),
    EndDate: new Date('2023-06-15T11:30:00'),
    Seats: 45,
    TravelType: 'Guided Tour',
    Status: TripStatus.Available,
    HostId: 101,
    CreatedAt: new Date('2023-01-01')
  };

  itinerary = [
    { day: 'Day 1', dateString: 'April 10, 2023 - Sunday 11:30 pm', image: 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?auto=format&fit=crop&w=64&q=80' },
    { day: 'Day 2', dateString: 'April 11, 2023 - Monday 11:30 pm', image: 'https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?auto=format&fit=crop&w=64&q=80' },
    { day: 'Day 3', dateString: 'April 12, 2023 - Tuesday 11:30 pm', image: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80' },
    { day: 'Day 4', dateString: 'April 13, 2023 - Wednesday 11:30 pm', image: 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?auto=format&fit=crop&w=64&q=80' },
    { day: 'Day 5', dateString: 'April 14, 2023 - Thursday 11:30 pm', image: 'https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?auto=format&fit=crop&w=64&q=80' }
  ];

  members = [
    { name: 'Sarah P.', role: 'profile', avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=64&q=80' },
    { name: 'Emily Chen', role: 'profile', avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?auto=format&fit=crop&w=64&q=80' },
    { name: 'Michael Brown', role: 'profile', avatar: 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=64&q=80' },
    { name: 'James Doe', role: 'profile', avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80' },
    { name: 'Alicia Keys', role: 'profile', avatar: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=64&q=80' }
  ];
}
