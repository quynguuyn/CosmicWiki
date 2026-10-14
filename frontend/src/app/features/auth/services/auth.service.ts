import { Injectable, inject } from '@angular/core';
import {
    catchError,
    finalize,
    Observable,
    shareReplay,
    tap,
    throwError
} from 'rxjs';

import { AuthApi } from '../api/auth.api';
import { AuthSessionService } from './auth-session.service';
import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private readonly api = inject(AuthApi);
    private readonly session = inject(AuthSessionService);
    private refreshRequest$: Observable<LoginResponse> | null = null;

    login(request: LoginRequest) {
        return this.api.login(request);
    }

    saveSession(response: Parameters<
        AuthSessionService['saveSession']
    >[0]): void {
        this.session.saveSession(response);
    }

    logout() {
        return this.api.logout().pipe(
            finalize(() => this.session.clearSession())
        );
    }

    refreshSession() {
        if (!this.refreshRequest$) {
            this.refreshRequest$ = this.api.refresh().pipe(
                tap(response => this.session.saveSession(response)),
                catchError(error => {
                    this.session.clearSession();
                    return throwError(() => error);
                }),
                finalize(() => {
                    this.refreshRequest$ = null;
                }),
                shareReplay({ bufferSize: 1, refCount: false })
            );
        }

        return this.refreshRequest$;
    }

    isAuthenticated(): boolean {
        return this.session.isAuthenticated();
    }

    getAccessToken(): string | null {
        return this.session.getAccessToken();
    }

    getCustomerId(): string | null {
        return this.session.getCustomerId();
    }
}