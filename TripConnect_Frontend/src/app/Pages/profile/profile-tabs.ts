import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HostedTrips } from './components/hosted-trips/hosted-trips';
import { JoinedTrips } from './components/joined-trips/joined-trips';
import { Reviews } from './components/reviews/reviews';

@Component({
  selector: 'app-profile-tabs',
  imports: [CommonModule, HostedTrips, JoinedTrips, Reviews],
  templateUrl: './profile-tabs.html',
})
export class ProfileTabs implements OnInit {
  activeTab: string = 'hosted';

  ngOnInit(): void {
    // Tab initialization if needed
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
}
