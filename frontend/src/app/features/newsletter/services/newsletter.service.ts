import { Injectable, inject } from '@angular/core';

import { NewsletterApi } from '../api/newsletter.api';

@Injectable({ providedIn: 'root' })
export class NewsletterService {
  private readonly api = inject(NewsletterApi);

  subscribe(email: string) {
    return this.api.subscribe(email);
  }
}
