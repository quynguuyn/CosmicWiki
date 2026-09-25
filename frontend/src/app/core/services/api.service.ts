import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { API_CONFIG } from '../config/api.config';

@Injectable({
    providedIn: 'root'
})
export class ApiService {

    private readonly http = inject(HttpClient);
    private readonly baseUrl = API_CONFIG.baseUrl;

    get<T>(
        endpoint: string,
        params?: HttpParams
    ) {
        return this.http.get<T>(
            `${this.baseUrl}${endpoint}`,
            { params }
        );
    }

    post<TRequest, TResponse>(
        endpoint: string,
        body: TRequest
    ) {
        return this.http.post<TResponse>(
            `${this.baseUrl}${endpoint}`,
            body
        );
    }

    put<TRequest, TResponse>(
        endpoint: string,
        body: TRequest
    ) {
        return this.http.put<TResponse>(
            `${this.baseUrl}${endpoint}`,
            body
        );
    }

    delete<T>(endpoint: string) {
        return this.http.delete<T>(
            `${this.baseUrl}${endpoint}`
        );
    }
}