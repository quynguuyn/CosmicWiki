import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';

import { API_CONFIG } from '../config/api.config';
import { AuthService } from '../../features/auth/services/auth.service';
import { AuthSessionService } from '../../features/auth/services/auth-session.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const apiUrl = new URL(API_CONFIG.baseUrl);
    const requestUrl = new URL(req.url, apiUrl);

    if (requestUrl.origin !== apiUrl.origin) {
        return next(req);
    }

    const authService = inject(AuthService);
    const session = inject(AuthSessionService);
    const token = session.getAccessToken();
    const isAuthEndpoint = /\/api\/auth\/(login|refresh|logout)(?:$|\?)/.test(
        req.url
    );

    const authReq = req.clone({
        withCredentials: true,
        setHeaders: token && !isAuthEndpoint
            ? { Authorization: `Bearer ${token}` }
            : {}
    });

    return next(authReq).pipe(
        catchError(error => {
            if (
                !(error instanceof HttpErrorResponse)
                || error.status !== 401
                || isAuthEndpoint
            ) {
                return throwError(() => error);
            }

            return authService.refreshSession().pipe(
                switchMap(response => next(
                    req.clone({
                        withCredentials: true,
                        setHeaders: {
                            Authorization: `Bearer ${response.accessToken}`
                        }
                    })
                )),
                catchError(() => throwError(() => error))
            );
        })
    );
};
