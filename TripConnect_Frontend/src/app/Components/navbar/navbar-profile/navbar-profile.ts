import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-navbar-profile',
  imports: [],
  templateUrl: './navbar-profile.html',
  styleUrl: './navbar-profile.css',
})
export class NavbarProfile {
  private readonly authService = inject(AuthService);

  userAvatar = computed(() => {
    const raw = localStorage.getItem('tc_user');
    if (raw) {
      const user = JSON.parse(raw);
      if (user?.avatarUrl) return user.avatarUrl;
      if (user?.name) return `https://ui-avatars.com/api/?name=${encodeURIComponent(user.name)}&background=random`;
    }
    return 'https://ui-avatars.com/api/?name=User&background=random';
  });
}
