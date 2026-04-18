import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ViewProfileTabs } from './view-profile-tabs';
import { ProfileService } from '../profile/profile.service';
import { UserResponseDto } from '../../models/api.types';

@Component({
  selector: 'app-view-profile',
  standalone: true,
  imports: [CommonModule, ViewProfileTabs],
  templateUrl: './view-profile.html',
})
export class ViewProfile implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly profileService = inject(ProfileService);

  user: UserResponseDto | null = null;
  targetUserId!: number;

  ngOnInit(): void {
    this.targetUserId = Number(this.route.snapshot.paramMap.get('id'));
    this.profileService.getUserById(this.targetUserId).subscribe({
      next: (u) => { this.user = u; },
      error: () => {}
    });
  }

  renderStars(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 !== 0;
    let stars = '★'.repeat(fullStars);
    if (hasHalfStar) stars += '☆';
    stars += '☆'.repeat(5 - Math.ceil(rating));
    return stars;
  }
}
