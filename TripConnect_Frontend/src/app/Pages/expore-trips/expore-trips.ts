import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripCard, Trip } from '../../Components/trip-card/trip-card';

@Component({
  selector: 'app-expore-trips',
  imports: [CommonModule, TripCard],
  templateUrl: './expore-trips.html',
  styleUrl: './expore-trips.css'
})
export class ExporeTrips {
  location = signal<string>('Location');
  budgetMin = signal<number>(30);
  budgetMax = signal<number>(1400);
  tripType = signal<string>('Tours None');

  // Dynamic filter arrays
  locations: string[] = ['Location', 'Bali', 'Alps', 'New York City'];
  tripTypes: string[] = ['Tours None', 'Guided Tours', 'Self-guided', 'Adventure'];
  datesFilter = [
    { label: 'Day 1 - 3n Jun', checked: false },
    { label: 'Day 1 - 3n Am', checked: false },
    { label: 'Day 1 - 50n Joe', checked: false }
  ];

  updateLocation(event: Event) {
    this.location.set((event.target as HTMLInputElement).value);
  }

  updateTripType(event: Event) {
    this.tripType.set((event.target as HTMLSelectElement).value);
  }

  updateBudgetMin(event: Event) {
    const target = event.target as HTMLInputElement;
    const val = parseInt(target.value, 10);
    if (val <= this.budgetMax()) {
      this.budgetMin.set(val);
    } else {
      target.value = this.budgetMin().toString();
    }
  }

  updateBudgetMax(event: Event) {
    const target = event.target as HTMLInputElement;
    const val = parseInt(target.value, 10);
    if (val >= this.budgetMin()) {
      this.budgetMax.set(val);
    } else {
      target.value = this.budgetMax().toString();
    }
  }
  trips: Trip[] = [
    {
      id: 1,
      title: 'Explore Bali: Culture & Coastline',
      image: 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?auto=format&fit=crop&w=800&q=80',
      dates: 'Dates - 2020 - Jan 2023',
      status: 'Organiser - Available',
      price: 1400,
      avatars: [
        'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80',
        'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=64&q=80',
        'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=64&q=80'
      ]
    },
    {
      id: 2,
      title: 'Alps Hiking Adventure',
      image: 'https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=800&q=80',
      dates: 'Dates - 2020 - Jan 2023',
      status: 'Organiser - Available',
      price: 1400,
      avatars: [
        'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80',
        'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=64&q=80'
      ]
    },
    {
      id: 3,
      title: 'New York City Explorer',
      image: 'https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?auto=format&fit=crop&w=800&q=80',
      dates: 'Dates - 2020 - Jun 2023',
      status: 'Organiser - Available',
      price: 1300,
      isNew: true,
      avatars: [
        'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80',
        'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=64&q=80'
      ]
    },
    {
      id: 4,
      title: 'Explore Culture & Coastline',
      image: 'https://images.unsplash.com/photo-1506929562872-bb421503ef21?auto=format&fit=crop&w=800&q=80',
      dates: 'Dates - 2020 - Jan 2023',
      status: 'Organiser - Available',
      price: 1400,
      avatars: [
        'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=64&q=80'
      ]
    },
    {
      id: 5,
      title: 'Alps Hiking Aps',
      image: 'https://images.unsplash.com/photo-1522206090980-4c8e50ebc721?auto=format&fit=crop&w=800&q=80',
      dates: 'Dates - 2020 - Jan 2023',
      status: 'Organiser - Available',
      price: 1400,
      avatars: [
        'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80',
        'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=64&q=80'
      ]
    },
    {
      id: 6,
      title: 'Bali Yoga Retreat',
      image: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=800&q=80',
      dates: 'Dates - 2020 - Jun 2023',
      status: 'Organiser - Available',
      price: 1300,
      avatars: [
        'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=64&q=80',
        'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=64&q=80'
      ]
    }
  ];
}
