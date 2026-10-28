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
import { RegisterRequest } from '../models/register-request.model';

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

    register(request: RegisterRequest) {
        return this.api.register(request);
    }

    saveSession(response: Parameters<AuthSessionService['saveSession']>[0], rememberMe = false): void {
        this.session.saveSession(response, rememberMe);
    }

    logout() {
        return this.api.logout().pipe(
            finalize(() => this.session.clearSession())
        );
    }

    refreshSession() {
        if (!this.refreshRequest$) {
            this.refreshRequest$ = this.api.refresh().pipe(
                tap(response => this.session.saveSession(response, response.rememberMe)),
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

    get authenticated() {
        return this.session.authenticated;
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

    getEmail(): string | null {
        return this.session.getEmail();
    }

    hasPermission(permission: string): boolean {
        return this.session.hasPermission(permission);
    }
}