import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ViewProfileTabs } from './view-profile-tabs';
import { ProfileService, User } from '../profile/profile.service';

@Component({
  selector: 'app-view-profile',
  standalone: true,
  imports: [CommonModule, ViewProfileTabs],
  templateUrl: './view-profile.html',
})
export class ViewProfile implements OnInit {
  user: User | null = null;

  constructor(private profileService: ProfileService) {}

  ngOnInit(): void {
    this.user = this.profileService.getUserProfile();
  }

  renderStars(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 !== 0;
    let stars = '★'.repeat(fullStars);
    if (hasHalfStar) {
      stars += '☆';
    }
    stars += '☆'.repeat(5 - Math.ceil(rating));
    return stars;
  }
}
