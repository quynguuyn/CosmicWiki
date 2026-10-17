import { Injectable } from '@angular/core';

import { ApiService } from '../../../core/services/api.service';
import { FeatureApi } from '../../../core/services/feature-api';

import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';
import { RegisterRequest } from '../models/register-request.model';

@Injectable({
    providedIn: 'root'
})
export class AuthApi extends FeatureApi {

    constructor(api: ApiService) {
        super(api, '/api/auth');
    }

    login(request: LoginRequest) {
        return this.post<LoginRequest, LoginResponse>(
            '/login',
            request,
            true
        );
    }

    register(request: RegisterRequest) {
        return this.post<RegisterRequest, { userId: string; username: string; email: string }>(
            '/register',
            request,
            true
        );
    }

    refresh() {
        return this.post<Record<string, never>, LoginResponse>(
            '/refresh',
            {},
            true
        );
    }

    logout() {
        return this.post<Record<string, never>, void>(
            '/logout',
            {},
            true
        );
    }
}