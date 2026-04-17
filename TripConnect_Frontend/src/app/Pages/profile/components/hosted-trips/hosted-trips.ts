import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, Trip } from '../../profile.service';

@Component({
  selector: 'app-hosted-trips',
  imports: [CommonModule],
  templateUrl: './hosted-trips.html',
})
export class HostedTrips implements OnInit {
  trips: Trip[] = [];

  constructor(private profileService: ProfileService) {}

  ngOnInit(): void {
    this.trips = this.profileService.getHostedTrips();
  }
}
