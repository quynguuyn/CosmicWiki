import { Injectable, inject } from '@angular/core';

import { AuthApi } from '../api/auth.api';
import { AuthSessionService } from './auth-session.service';
import { LoginRequest } from '../models/login-request.model';

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private readonly api = inject(AuthApi);
    private readonly session = inject(AuthSessionService);

    login(request: LoginRequest) {
        return this.api.login(request);
    }

    saveSession(response: Parameters<
        AuthSessionService['saveSession']
    >[0]): void {
        this.session.saveSession(response);
    }

    logout(): void {
        this.session.clearSession();
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