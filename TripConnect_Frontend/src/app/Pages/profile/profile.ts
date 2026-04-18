import { Component, OnInit, inject } from '@angular/core';
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

  user: UserResponseDto | null = null;
  isEditModalOpen = false;
  editForm = { name: '', phone: '' };

  ngOnInit(): void {
    const userId = this.authService.getCurrentUserId();
    this.profileService.getUserById(userId).subscribe({
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
