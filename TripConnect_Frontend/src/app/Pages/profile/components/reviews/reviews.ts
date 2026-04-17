import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService, Review } from '../../profile.service';

@Component({
  selector: 'app-reviews',
  imports: [CommonModule],
  templateUrl: './reviews.html',
})
export class Reviews implements OnInit {
  reviews: Review[] = [];

  constructor(private profileService: ProfileService) {}

  ngOnInit(): void {
    this.reviews = this.profileService.getReviews();
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
