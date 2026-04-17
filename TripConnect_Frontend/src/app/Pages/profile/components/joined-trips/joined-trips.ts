import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, Trip } from '../../profile.service';

@Component({
  selector: 'app-joined-trips',
  imports: [CommonModule],
  templateUrl: './joined-trips.html',
})
export class JoinedTrips implements OnInit {
  trips: Trip[] = [];

  constructor(private profileService: ProfileService) {}

  ngOnInit(): void {
    this.trips = this.profileService.getJoinedTrips();
  }
}
