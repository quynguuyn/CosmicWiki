import { Injectable, inject } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { AstronomicalImage, AstronomicalObject, Star } from '../models/star.model';

@Injectable({ providedIn: 'root' })
export class StarCatalogApi {
  private readonly api = inject(ApiService);

  getStar(id: string) {
    return this.api.get<Star>(`/api/catalog/stars/${encodeURIComponent(id)}`);
  }

  getImages(id: string) {
    return this.api.get<AstronomicalImage[]>(
      `/api/catalog/astronomical-objects/${encodeURIComponent(id)}/images`
    );
  }

  getAstronomicalObject(id: string) {
    return this.api.get<AstronomicalObject>(
      `/api/catalog/objects/${encodeURIComponent(id)}`
    );
  }
}
