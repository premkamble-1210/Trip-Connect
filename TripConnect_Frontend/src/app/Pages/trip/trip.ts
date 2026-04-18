import { Component, OnInit, inject, signal, computed, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TripMemberList } from '../../Components/trip-member-list/trip-member-list';
import { TripItinerary } from '../../Components/trip-itinerary/trip-itinerary';
import { TripService } from '../../services/trip.service';
import { JoinRequestService } from '../../services/join-request.service';
import { AuthService } from '../../services/auth.service';
import { TripResponseDto, TripMemberResponseDto } from '../../models/api.types';

@Component({
  selector: 'app-trip',
  imports: [CommonModule, TripMemberList, TripItinerary],
  templateUrl: './trip.html',
  styleUrl: './trip.css'
})
export class Trip implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tripService = inject(TripService);
  private readonly joinRequestService = inject(JoinRequestService);
  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  tripData = signal<TripResponseDto | null>(null);
  isLoading = signal(true);
  joinError = signal('');
  joinSuccess = signal(false);
  joinLoading = signal(false);
  isMember = signal(false);

  isHost = computed(() => {
    const trip = this.tripData();
    return trip ? trip.hostId === this.authService.getCurrentUserId() : false;
  });

  // Placeholder arrays until itinerary/members endpoints are available
  itinerary: { day: string; dateString: string; image: string }[] = [];
  members: { userId: number; name: string; role: string; avatar: string }[] = [];

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    const userId = this.authService.getCurrentUserId();

    this.tripService.getTripById(id).subscribe({
      next: (trip) => {
        this.tripData.set(trip);
        this.isLoading.set(false);
        this.buildItinerary(trip);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });

    // Check if user is a member of this trip
    this.tripService.getJoinedTrips(userId).subscribe({
      next: (trips) => {
        this.isMember.set(trips.some(t => t.id === id));
      },
      error: () => {}
    });

    // Fetch trip members
    this.tripService.getTripMembers(id).subscribe({
      next: (members) => {
        this.members = members.map(m => ({
          userId: m.userId,
          name: m.userName,
          role: m.role,
          avatar: `https://ui-avatars.com/api/?name=${encodeURIComponent(m.userName)}`
        }));
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  buildItinerary(trip: TripResponseDto): void {
    const start = new Date(trip.startDate);
    const end = new Date(trip.endDate);
    const days: typeof this.itinerary = [];
    let current = new Date(start);
    let dayNum = 1;
    while (current <= end && dayNum <= 10) {
      days.push({
        day: `Day ${dayNum}`,
        dateString: current.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }),
        image: trip.imgUrl || 'https://via.placeholder.com/400x200?text=Itinerary+Image'
      });
      current.setDate(current.getDate() + 1);
      dayNum++;
    }
    this.itinerary = days;
  }

  requestToJoin(): void {
    this.joinError.set('');
    const userId = this.authService.getCurrentUserId();
    const tripId = this.tripData()?.id;
    if (!tripId) return;

    this.joinLoading.set(true);
    this.joinRequestService.sendJoinRequest(userId, tripId).subscribe({
      next: () => {
        this.joinLoading.set(false);
        this.joinSuccess.set(true);
      },
      error: () => {
        this.joinLoading.set(false);
        this.joinError.set('Failed to send join request. You may have already requested.');
      }
    });
  }

  goToManageRequests(): void {
    this.router.navigate(['/request', this.tripData()?.id]);
  }
}
