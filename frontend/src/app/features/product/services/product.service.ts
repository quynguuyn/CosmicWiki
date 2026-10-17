import { Injectable, inject } from '@angular/core';
import { ProductApi } from '../api/product.api';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly api = inject(ProductApi);

  getProducts(page = 1, pageSize = 50, search?: string, categoryId?: string) {
    return this.api.getProducts(page, pageSize, search, categoryId);
  }

  getCategories() {
    return this.api.getCategories();
  }
}
