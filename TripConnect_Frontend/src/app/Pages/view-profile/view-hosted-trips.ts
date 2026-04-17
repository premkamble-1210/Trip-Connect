import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, Trip } from '../profile/profile.service';

@Component({
  selector: 'app-view-hosted-trips',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './view-hosted-trips.html',
})
export class ViewHostedTrips implements OnInit {
  trips: Trip[] = [];

  constructor(private profileService: ProfileService) {}

  ngOnInit(): void {
    this.trips = this.profileService.getHostedTrips();
  }
}
