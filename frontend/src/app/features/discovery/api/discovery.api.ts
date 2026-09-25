import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';

import { ApiService } from '../../../core/services/api.service';
import { FeatureApi } from '../../../core/services/feature-api';
import { PagedResult } from '../../../core/models/paged-result.model';
import { SearchResultResponse } from '../models/search-result-response.model';

@Injectable({
    providedIn: 'root'
})
export class DiscoveryApi extends FeatureApi {

    constructor(api: ApiService) {
        super(api, '/api/discovery');
    }

    search(
        search?: string,
        sourceType?: string,
        page: number = 1,
        pageSize: number = 20
    ) {
        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);

        if (search) {
            params = params.set('search', search);
        }

        if (sourceType) {
            params = params.set('sourceType', sourceType);
        }

        return this.get<PagedResult<SearchResultResponse>>('/search', params);
    }
}