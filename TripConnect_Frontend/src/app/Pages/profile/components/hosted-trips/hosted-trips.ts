import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, Trip } from '../../profile.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-hosted-trips',
  imports: [CommonModule],
  templateUrl: './hosted-trips.html',
})
export class HostedTrips implements OnInit {
  trips: Trip[] = [];

  constructor(private profileService: ProfileService, private route: Router) {}

  ngOnInit(): void {
    this.trips = this.profileService.getHostedTrips();
  }

  onclick(tripId: string | number): void {
    this.route.navigate(['request/', tripId]);
  }
}
