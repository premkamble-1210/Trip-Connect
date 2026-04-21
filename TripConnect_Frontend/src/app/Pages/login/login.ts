import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  username = signal('');
  password = signal('');
  errorMessage = signal('');
  isLoading = signal(false);

  login(): void {
    this.errorMessage.set('');
    if (!this.username() || !this.password()) {
      this.errorMessage.set('Please enter your username and password.');
      return;
    }
    this.isLoading.set(true);
    this.authService.login(this.username(), this.password()).subscribe({
      next: (res) => {
        console.log('📱 Login component received response:', res);
        this.isLoading.set(false);
        if (res.success) {
          console.log('✅ Login successful, navigating to /explore');
          console.log('🔐 Token in localStorage:', localStorage.getItem('tc_token') ? 'EXISTS' : 'MISSING');
          this.router.navigate(['/explore']);
        } else {
          console.log('❌ Login returned success=false');
          this.errorMessage.set(res.message || 'Login failed. Please try again.');
        }
      },
      error: (error) => {
        console.log('❌ Login error:', error);
        this.isLoading.set(false);
        this.errorMessage.set('Invalid username or password.');
      }
    });
  }
}
