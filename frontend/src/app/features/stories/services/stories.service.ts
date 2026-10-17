import { Injectable, inject } from '@angular/core';

import { StoriesApi } from '../api/stories.api';

@Injectable({ providedIn: 'root' })
export class StoriesService {
  private readonly api = inject(StoriesApi);

  getStories(search?: string, category?: string, sort = 'newest') {
    return this.api.getStories(search, category, sort);
  }

  getCategories() {
    return this.api.getCategories();
  }

  getBySlug(slug: string) {
    return this.api.getBySlug(slug);
  }
}
