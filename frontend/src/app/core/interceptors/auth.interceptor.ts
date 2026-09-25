import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthSessionService } from '../../features/auth/services/auth-session.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

    const session = inject(AuthSessionService);

    const token = session.getAccessToken();

    if (!token) {
        return next(req);
    }

    const authReq = req.clone({
        setHeaders: {
            Authorization: `Bearer ${token}`
        }
    });

    return next(authReq);
};