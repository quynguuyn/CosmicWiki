import { Injectable } from '@angular/core';

import { LoginResponse } from '../models/login-response.model';

@Injectable({
    providedIn: 'root'
})
export class AuthSessionService {

    private readonly accessTokenKey = 'cosmicwiki_access_token';
    private readonly expiresAtKey = 'cosmicwiki_access_token_expires_at';

    saveSession(response: LoginResponse): void {
        const expiresAt =
            Date.now() + response.expiresIn * 1000;

        localStorage.setItem(
            this.accessTokenKey,
            response.accessToken
        );

        localStorage.setItem(
            this.expiresAtKey,
            expiresAt.toString()
        );
    }

    getAccessToken(): string | null {
        return localStorage.getItem(
            this.accessTokenKey
        );
    }

    getCustomerId(): string | null {
        const token = this.getAccessToken();
        if (!token) {
            return null;
        }

        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.sub ?? null;
        } catch {
            return null;
        }
    }

    clearSession(): void {
        localStorage.removeItem(this.accessTokenKey);
        localStorage.removeItem(this.expiresAtKey);
    }

    isAuthenticated(): boolean {
        const token = this.getAccessToken();

        if (!token) {
            return false;
        }

        const expiresAt = Number(
            localStorage.getItem(this.expiresAtKey)
        );

        if (!expiresAt || Date.now() >= expiresAt) {
            this.clearSession();
            return false;
        }

        return true;
    }
}