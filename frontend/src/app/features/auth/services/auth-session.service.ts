import { Injectable } from '@angular/core';

import { LoginResponse } from '../models/login-response.model';

@Injectable({
    providedIn: 'root'
})
export class AuthSessionService {

    private readonly accessTokenKey = 'cosmicwiki_access_token';

    saveSession(response: LoginResponse): void {
        localStorage.setItem(
            this.accessTokenKey,
            response.accessToken
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
    }

    isAuthenticated(): boolean {
        return this.getAccessToken() !== null;
    }
}