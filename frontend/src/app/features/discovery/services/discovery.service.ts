import { Injectable, inject } from '@angular/core';

import { DiscoveryApi } from '../api/discovery.api';

@Injectable({
  providedIn: 'root'
})
export class DiscoveryService {

  private readonly api = inject(DiscoveryApi);

  search(
    search?: string,
    sourceType?: string,
    page: number = 1,
    pageSize: number = 20
  ) {
    return this.api.search(search, sourceType, page, pageSize);
  }
}