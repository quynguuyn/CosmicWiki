import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';

export abstract class FeatureApi {

    protected constructor(
        protected readonly api: ApiService,
        protected readonly basePath: string
    ) { }

    protected get<T>(
        endpoint: string,
        params?: HttpParams
    ) {
        return this.api.get<T>(
            `${this.basePath}${endpoint}`,
            params
        );
    }

    protected post<TRequest, TResponse>(
        endpoint: string,
        body: TRequest
    ) {
        return this.api.post<TRequest, TResponse>(
            `${this.basePath}${endpoint}`,
            body
        );
    }

    protected put<TRequest, TResponse>(
        endpoint: string,
        body: TRequest
    ) {
        return this.api.put<TRequest, TResponse>(
            `${this.basePath}${endpoint}`,
            body
        );
    }

    protected delete<T>(endpoint: string) {
        return this.api.delete<T>(
            `${this.basePath}${endpoint}`
        );
    }
}