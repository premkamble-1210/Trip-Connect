import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  name = signal('');
  username = signal('');
  email = signal('');
  phone = signal('');
  password = signal('');
  errorMessage = signal('');
  isLoading = signal(false);

  register(): void {
    this.errorMessage.set('');
    if (!this.name() || !this.username() || !this.email() || !this.phone() || !this.password()) {
      this.errorMessage.set('Please fill in all fields.');
      return;
    }
    this.isLoading.set(true);
    this.authService
      .register(this.name(), this.username(), this.email(), this.phone(), this.password())
      .subscribe({
        next: (res) => {
          console.log('📝 Register response received:', res);
          this.isLoading.set(false);
          if (res.success) {
            console.log('✅ Registration successful');
            console.log('🔐 Token in localStorage:', localStorage.getItem('tc_token') ? 'EXISTS' : 'MISSING');
            if (localStorage.getItem('tc_token')) {
              console.log('✅ Token was stored, navigating to /profile');
              this.router.navigate(['/profile']);
            } else {
              console.log('⚠️ Token not stored after registration, navigating to /login');
              this.router.navigate(['/login']);
            }
          } else {
            console.log('❌ Registration returned success=false');
            this.errorMessage.set(res.message || 'Registration failed. Please try again.');
          }
        },
        error: (error) => {
          console.log('❌ Registration error:', error);
          this.isLoading.set(false);
          this.errorMessage.set('Registration failed. Username or email may already be in use.');
        }
      });
  }
}
