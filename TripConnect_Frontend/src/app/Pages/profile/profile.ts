import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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
  private readonly route = inject(ActivatedRoute);

  user: UserResponseDto | null = null;
  isEditModalOpen = false;
  isLoading = true;
  errorMessage: string | null = null;
  editForm = { name: '', phone: '' };
  verificationMessage: string | null = null;
  verificationError: string | null = null;
  isRequestingVerification = false;

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

    // Handle redirect back from backend verify-email endpoint
    this.route.queryParams.subscribe(params => {
      if (params['emailVerified'] === 'true') {
        this.verificationMessage = 'Your email has been verified successfully!';
        this.profileService.getUserById(userId).subscribe({
          next: (u) => { this.user = u; this.cdr.markForCheck(); },
          error: () => {}
        });
      } else if (params['emailVerified'] === 'false') {
        const reason = params['reason'] ?? 'unknown_error';
        this.verificationError = reason === 'Verification token has expired'
          ? 'Your verification link has expired. Please request a new one.'
          : 'Email verification failed. Please try again.';
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
  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        console.log('🔓 Logout complete');
        window.location.reload();
      },
      error: (error) => {
        console.error('❌ Logout failed:', error);
        // Still clear local storage and redirect even if backend call fails
        localStorage.removeItem('tc_token');
        localStorage.removeItem('tc_refresh_token');
        localStorage.removeItem('tc_userId');
        this.authService['router'].navigate(['/login']);
      }
    });
  }

  requestVerification(): void {
    if (!this.user || this.isRequestingVerification) return;
    this.isRequestingVerification = true;
    this.verificationMessage = null;
    this.verificationError = null;
    const userId = this.authService.getCurrentUserId();
    this.profileService.requestEmailVerification(userId).subscribe({
      next: (res) => {
        this.verificationMessage = res.message;
        this.isRequestingVerification = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.verificationError = err.error?.message ?? 'Failed to send verification email.';
        this.isRequestingVerification = false;
        this.cdr.markForCheck();
      }
    });
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
