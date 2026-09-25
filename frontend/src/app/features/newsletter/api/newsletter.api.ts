import { Injectable } from '@angular/core';

import { ApiService } from '../../../core/services/api.service';
import { FeatureApi } from '../../../core/services/feature-api';

@Injectable({ providedIn: 'root' })
export class NewsletterApi extends FeatureApi {
  constructor(api: ApiService) {
    super(api, '/api/newsletter');
  }

  subscribe(email: string) {
    return this.post<{ email: string }, { message: string }>(
      '/subscriptions',
      { email }
    );
  }
}
