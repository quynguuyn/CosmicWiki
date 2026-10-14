import { Injectable, signal } from '@angular/core';

import { LoginResponse } from '../models/login-response.model';

@Injectable({
    providedIn: 'root'
})
export class AuthSessionService {

    private readonly accessTokenKey = 'cosmicwiki_access_token';
<<<<<<< HEAD
    private readonly expiresAtKey = 'cosmicwiki_access_token_expires_at';
    readonly authenticated = signal(this.hasValidSession());
=======
>>>>>>> 47a0bfc14876280edb5425edba4381ef5de4fc91

    saveSession(response: LoginResponse): void {
        localStorage.setItem(
            this.accessTokenKey,
            response.accessToken
        );
<<<<<<< HEAD

        localStorage.setItem(
            this.expiresAtKey,
            expiresAt.toString()
        );
        this.authenticated.set(true);
=======
>>>>>>> 47a0bfc14876280edb5425edba4381ef5de4fc91
    }

    getAccessToken(): string | null {
        return localStorage.getItem(
            this.accessTokenKey
        );
    }

    getCustomerId(): string | null {
        return this.getTokenPayload()?.sub ?? null;
    }

    getEmail(): string | null {
        return this.getTokenPayload()?.email ?? null;
    }

    clearSession(): void {
        localStorage.removeItem(this.accessTokenKey);
<<<<<<< HEAD
        localStorage.removeItem(this.expiresAtKey);
        this.authenticated.set(false);
=======
>>>>>>> 47a0bfc14876280edb5425edba4381ef5de4fc91
    }

    isAuthenticated(): boolean {
        return this.getAccessToken() !== null;
    }

    private hasValidSession(): boolean {
        const token = localStorage.getItem(this.accessTokenKey);
        const expiresAt = Number(localStorage.getItem(this.expiresAtKey));
        return Boolean(token && expiresAt && Date.now() < expiresAt);
    }

    hasPermission(permission: string): boolean {
        const permissions = this.getTokenPayload()?.permission;
        return Array.isArray(permissions)
            ? permissions.includes(permission)
            : permissions === permission;
    }

    private getTokenPayload(): { sub?: string; email?: string; unique_name?: string; permission?: string | string[] } | null {
        const token = this.getAccessToken();
        if (!token) {
            return null;
        }

        try {
            return JSON.parse(atob(token.split('.')[1]));
        } catch {
            return null;
        }
    }
}