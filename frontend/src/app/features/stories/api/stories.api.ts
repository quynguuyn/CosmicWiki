import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';

import { ApiService } from '../../../core/services/api.service';
import { FeatureApi } from '../../../core/services/feature-api';
import { StoriesResult, Story } from '../models/story.model';

@Injectable({ providedIn: 'root' })
export class StoriesApi extends FeatureApi {
  constructor(api: ApiService) {
    super(api, '/api/stories');
  }

  getStories(search?: string, category?: string, sort = 'newest') {
    let params = new HttpParams().set('page', 1).set('pageSize', 50).set('sort', sort);
    if (search) params = params.set('search', search);
    if (category && category !== 'all') params = params.set('category', category);
    return this.get<StoriesResult>('', params);
  }

  getCategories() {
    return this.get<string[]>('/categories');
  }

  getBySlug(slug: string) {
    return this.get<Story>(`/slug/${encodeURIComponent(slug)}`);
  }
}
