import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const profileGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.requiresProfileCompletion()) {
    return router.createUrlTree(['/auth/complete-profile']);
  }

  return true;
};
