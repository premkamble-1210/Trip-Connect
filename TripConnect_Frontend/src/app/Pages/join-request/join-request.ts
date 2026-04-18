import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { JoinRequestService } from '../../services/join-request.service';
import { TripService } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';
import { JoinRequestResponseDto } from '../../models/api.types';

export interface FilterOption {
  label: string;
  value: string;
  selected: boolean;
}

@Component({
  selector: 'app-join-request',
  imports: [CommonModule],
  templateUrl: './join-request.html',
  styleUrl: './join-request.css',
})
export class JoinRequest implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly joinRequestService = inject(JoinRequestService);
  private readonly tripService = inject(TripService);
  private readonly authService = inject(AuthService);

  tripId!: number;
  tripName = 'Loading...';
  requests: JoinRequestResponseDto[] = [];
  isLoading = false;

  profile = {
    name: '',
    username: '',
    avatar: ''
  };

  filters: FilterOption[] = [
    { label: 'Pending', value: 'Pending', selected: true },
    { label: 'Accepted', value: 'Accepted', selected: false },
    { label: 'Rejected', value: 'Rejected', selected: false },
    { label: 'Cancelled', value: 'Cancelled', selected: false },
  ];

  selectedFilter: string = 'Pending';

  ngOnInit(): void {
    this.tripId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCurrentUser();
    this.loadTripTitle();
    this.loadRequests();
  }

  private loadCurrentUser(): void {
    const raw = localStorage.getItem('tc_user');
    if (raw) {
      const u = JSON.parse(raw);
      this.profile = {
        name: u.name || '',
        username: '@' + (u.username || ''),
        avatar: u.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(u.name || 'User')}`
      };
    }
  }

  private loadTripTitle(): void {
    this.tripService.getTripById(this.tripId).subscribe({
      next: (trip) => { this.tripName = trip.title; },
      error: () => { this.tripName = `Trip #${this.tripId}`; }
    });
  }

  loadRequests(): void {
    this.isLoading = true;
    this.joinRequestService.getRequestsForTrip(this.tripId).subscribe({
      next: (data) => {
        this.requests = data;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  get pendingCount(): number { return this.requests.filter(r => r.status === 'Pending').length; }
  get acceptedCount(): number { return this.requests.filter(r => r.status === 'Accepted').length; }
  get rejectedCount(): number { return this.requests.filter(r => r.status === 'Rejected').length; }
  get cancelledCount(): number { return this.requests.filter(r => r.status === 'Cancelled').length; }
  get totalCount(): number { return this.requests.length; }

  get filteredRequests(): JoinRequestResponseDto[] {
    return this.requests.filter(r => r.status === this.selectedFilter);
  }

  selectFilter(filter: FilterOption): void {
    this.filters.forEach(f => f.selected = false);
    filter.selected = true;
    this.selectedFilter = filter.value;
  }

  acceptRequest(request: JoinRequestResponseDto): void {
    const hostId = this.authService.getCurrentUserId();
    this.joinRequestService.acceptRequest(request.id, hostId).subscribe({
      next: () => { request.status = 'Accepted'; },
      error: () => {}
    });
  }

  declineRequest(request: JoinRequestResponseDto): void {
    const hostId = this.authService.getCurrentUserId();
    this.joinRequestService.rejectRequest(request.id, hostId).subscribe({
      next: () => { request.status = 'Rejected'; },
      error: () => {}
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending': return 'bg-orange-100 text-orange-600';
      case 'Accepted': return 'bg-green-100 text-green-600';
      case 'Rejected': return 'bg-red-100 text-red-600';
      case 'Cancelled': return 'bg-gray-100 text-gray-600';
      default: return 'bg-gray-100 text-gray-600';
    }
  }
}
