import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';

import { ApiService } from '../../../core/services/api.service';
import { FeatureApi } from '../../../core/services/feature-api';
import { PagedResult } from '../../../core/models/paged-result.model';
import { Product, ProductCategory } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductApi extends FeatureApi {
  constructor(api: ApiService) {
    super(api, '/api/product');
  }

  getProducts(page = 1, pageSize = 50, search?: string, categoryId?: string) {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    const queryParams = search ? params.set('search', search) : params;
    const filteredParams = categoryId
      ? queryParams.set('categoryId', categoryId)
      : queryParams;
    return this.get<PagedResult<Product>>('/products', filteredParams);
  }

  getCategories() {
    return this.get<ProductCategory[]>('/categories');
  }
}
