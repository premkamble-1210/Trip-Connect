import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileTabs } from './profile-tabs';
import { ProfileService, User } from './profile.service';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, ProfileTabs],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  user: User | null = null;
  isEditModalOpen: boolean = false;
  editForm = {
    email: '',
    phone: ''
  };

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

  openEditModal(): void {
    if (this.user) {
      this.editForm.email = this.user.email;
      this.editForm.phone = this.user.phone;
      this.isEditModalOpen = true;
    }
  }

  closeEditModal(): void {
    this.isEditModalOpen = false;
  }

  saveChanges(): void {
    if (this.user) {
      this.user.email = this.editForm.email;
      this.user.phone = this.editForm.phone;
      this.isEditModalOpen = false;
    }
  }
}
