import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated()
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return !auth.isAuthenticated()
    ? router.createUrlTree(['/login'])
    : auth.hasPermission('shipping.write')
      ? true
      : router.createUrlTree(['/forbidden']);
};

export const deliveryGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return !auth.isAuthenticated()
    ? router.createUrlTree(['/login'])
    : auth.hasPermission('delivery.write')
      ? true
      : router.createUrlTree(['/forbidden']);
};
