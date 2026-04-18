import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService } from '../../profile.service';
import { AuthService } from '../../../../services/auth.service';
import { TripResponseDto } from '../../../../models/api.types';

@Component({
  selector: 'app-joined-trips',
  imports: [CommonModule],
  templateUrl: './joined-trips.html',
})
export class JoinedTrips implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);

  trips: TripResponseDto[] = [];

  ngOnInit(): void {
    const userId = this.authService.getCurrentUserId();
    this.profileService.getJoinedTrips(userId).subscribe({
      next: (data) => { this.trips = data; },
      error: () => {}
    });
  }
}
