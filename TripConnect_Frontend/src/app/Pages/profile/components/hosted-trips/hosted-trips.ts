import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ProfileService } from '../../profile.service';
import { AuthService } from '../../../../services/auth.service';
import { TripResponseDto } from '../../../../models/api.types';

@Component({
  selector: 'app-hosted-trips',
  standalone: true,
  imports: [DatePipe, DecimalPipe, CommonModule, RouterLink],
  templateUrl: './hosted-trips.html',
})
export class HostedTrips implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  trips: TripResponseDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  ngOnInit(): void {
    const userId = this.authService.getCurrentUserId();
    this.profileService.getHostedTrips(userId).subscribe({
      next: (data) => { 
        this.trips = data; 
        this.isLoading = false;
        console.log('Hosted trips loaded:', data);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading hosted trips:', err);
        this.errorMessage = 'Failed to load hosted trips';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onclick(tripId: number): void {
    this.router.navigate(['/request', tripId]);
  }
}
