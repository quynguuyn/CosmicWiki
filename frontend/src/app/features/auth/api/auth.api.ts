import { Injectable } from '@angular/core';

import { ApiService } from '../../../core/services/api.service';
import { FeatureApi } from '../../../core/services/feature-api';

import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';

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
            request
        );
    }
}