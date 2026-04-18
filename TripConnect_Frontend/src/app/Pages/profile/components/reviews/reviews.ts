import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService } from '../../profile.service';
import { AuthService } from '../../../../services/auth.service';
import { RatingResponseDto } from '../../../../models/api.types';

@Component({
  selector: 'app-reviews',
  imports: [CommonModule],
  templateUrl: './reviews.html',
})
export class Reviews implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);

  reviews: RatingResponseDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  ngOnInit(): void {
    const userId = this.authService.getCurrentUserId();
    this.profileService.getReviews(userId).subscribe({
      next: (data) => { 
        this.reviews = data; 
        this.isLoading = false;
        console.log('Reviews loaded:', data);
      },
      error: (err) => {
        console.error('Error loading reviews:', err);
        this.errorMessage = 'Failed to load reviews';
        this.isLoading = false;
      }
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
