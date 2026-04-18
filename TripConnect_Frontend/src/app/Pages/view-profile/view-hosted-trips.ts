import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService } from '../profile/profile.service';
import { TripResponseDto } from '../../models/api.types';

@Component({
  selector: 'app-view-hosted-trips',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './view-hosted-trips.html',
})
export class ViewHostedTrips implements OnInit {
  @Input() userId!: number;

  private readonly profileService = inject(ProfileService);

  trips: TripResponseDto[] = [];

  ngOnInit(): void {
    if (this.userId) {
      this.profileService.getHostedTrips(this.userId).subscribe({
        next: (data) => { this.trips = data; },
        error: () => {}
      });
    }
  }
}
