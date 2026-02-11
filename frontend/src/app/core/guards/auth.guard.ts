import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Kullanıcının oturum açıp açmadığını kontrol eden guard.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Eğer kullanıcı zaten giriş yapmışsa devam et
  if (authService.isLoggedIn()) {
    return true;
  }

  // Hiçbir bilgi yoksa veya token geçersizse login sayfasına yönlendir
  router.navigate(['/auth/login']);
  return false;
};
