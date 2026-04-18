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
        this.isLoading.set(false);
        if (res.success) {
          this.router.navigate(['/explore']);
        } else {
          this.errorMessage.set(res.message || 'Login failed. Please try again.');
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('Invalid username or password.');
      }
    });
  }
}
