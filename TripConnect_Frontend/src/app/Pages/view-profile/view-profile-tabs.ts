import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ViewHostedTrips } from './view-hosted-trips';
import { JoinedTrips } from '../profile/components/joined-trips/joined-trips';
import { Reviews } from '../profile/components/reviews/reviews';

@Component({
  selector: 'app-view-profile-tabs',
  standalone: true,
  imports: [CommonModule, ViewHostedTrips, JoinedTrips, Reviews],
  templateUrl: './view-profile-tabs.html',
})
export class ViewProfileTabs {
  activeTab: string = 'hosted';

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
}
