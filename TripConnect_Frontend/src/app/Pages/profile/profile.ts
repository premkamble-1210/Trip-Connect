import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileTabs } from './profile-tabs';
import { ProfileService } from './profile.service';
import { AuthService } from '../../services/auth.service';
import { UserResponseDto } from '../../models/api.types';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, ProfileTabs],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  user: UserResponseDto | null = null;
  isEditModalOpen = false;
  isLoading = true;
  errorMessage: string | null = null;
  editForm = { name: '', phone: '' };

  ngOnInit(): void {
    console.log('Profile component initialized');
    if (!this.authService.isAuthenticated()) {
      this.errorMessage = 'Please log in to view your profile';
      this.isLoading = false;
      this.cdr.markForCheck();
      console.log('Not authenticated');
      return;
    }

    const userId = this.authService.getCurrentUserId();
    console.log('Current userId:', userId);
    
    if (!userId || userId === 0) {
      this.errorMessage = 'Invalid user ID. Please log in again';
      this.isLoading = false;
      this.cdr.markForCheck();
      console.log('Invalid userId');
      return;
    }

    console.log('About to fetch profile...');
    this.profileService.getUserById(userId).subscribe({
      next: (u) => { 
        console.log('Profile data received:', u);
        this.user = u; 
        this.isLoading = false;
        this.cdr.markForCheck();
        console.log('isLoading set to false, user:', this.user?.name);
      },
      error: (err) => { 
        console.error('Failed to load profile:', err);
        this.errorMessage = 'Failed to load profile. Please try again.';
        this.isLoading = false;
        this.cdr.markForCheck();
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

  openEditModal(): void {
    if (this.user) {
      this.editForm.name = this.user.name;
      this.editForm.phone = this.user.phone;
      this.isEditModalOpen = true;
    }
  }

  closeEditModal(): void {
    this.isEditModalOpen = false;
  }

  saveChanges(): void {
    if (!this.user) return;
    const userId = this.authService.getCurrentUserId();
    this.profileService.updateUser(userId, {
      name: this.editForm.name,
      phone: this.editForm.phone
    }).subscribe({
      next: (updated) => {
        this.user = updated;
        this.isEditModalOpen = false;
      },
      error: () => {}
    });
  }
}
