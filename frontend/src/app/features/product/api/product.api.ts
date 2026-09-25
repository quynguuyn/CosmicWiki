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

  getProducts(page = 1, pageSize = 50) {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.get<PagedResult<Product>>('/products', params);
  }

  getCategories() {
    return this.get<ProductCategory[]>('/categories');
  }
}
