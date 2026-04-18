import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProfileService } from '../../profile.service';
import { AuthService } from '../../../../services/auth.service';
import { TripResponseDto } from '../../../../models/api.types';

@Component({
  selector: 'app-hosted-trips',
  imports: [CommonModule],
  templateUrl: './hosted-trips.html',
})
export class HostedTrips implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  trips: TripResponseDto[] = [];

  ngOnInit(): void {
    const userId = this.authService.getCurrentUserId();
    this.profileService.getHostedTrips(userId).subscribe({
      next: (data) => { this.trips = data; },
      error: () => {}
    });
  }

  onclick(tripId: number): void {
    this.router.navigate(['/request', tripId]);
  }
}
