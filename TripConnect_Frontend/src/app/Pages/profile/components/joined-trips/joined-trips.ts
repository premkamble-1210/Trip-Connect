import { Component, OnInit, inject, ChangeDetectorRef, Input } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProfileService } from '../../profile.service';
import { AuthService } from '../../../../services/auth.service';
import { TripResponseDto } from '../../../../models/api.types';

@Component({
  selector: 'app-joined-trips',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './joined-trips.html',
})
export class JoinedTrips implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input() userId?: number;

  trips: TripResponseDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  ngOnInit(): void {
    const userId = this.userId ?? this.authService.getCurrentUserId();
    this.profileService.getJoinedTrips(userId).subscribe({
      next: (data) => { 
        this.trips = data; 
        this.isLoading = false;
        console.log('Joined trips loaded:', data);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading joined trips:', err);
        this.errorMessage = 'Failed to load joined trips';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
