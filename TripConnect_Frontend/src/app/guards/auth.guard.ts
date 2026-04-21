import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const token = localStorage.getItem('tc_token');
  console.log('🛡️ AuthGuard checking access to:', state.url);
  console.log('🔐 Token exists:', token ? 'YES' : 'NO');
  if (token) {
    console.log('✅ AuthGuard: Access granted');
    return true;
  }
  console.log('❌ AuthGuard: Access denied, redirecting to /login');
  return inject(Router).createUrlTree(['/login']);
};
