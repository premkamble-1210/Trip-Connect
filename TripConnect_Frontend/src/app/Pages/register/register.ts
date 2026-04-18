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
          this.isLoading.set(false);
          if (res.success) {
            this.router.navigate(['/login']);
          } else {
            this.errorMessage.set(res.message || 'Registration failed. Please try again.');
          }
        },
        error: () => {
          this.isLoading.set(false);
          this.errorMessage.set('Registration failed. Username or email may already be in use.');
        }
      });
  }
}
