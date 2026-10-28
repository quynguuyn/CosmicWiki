import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, forkJoin, of } from 'rxjs';
import { StarCatalogApi } from '../../features/catalog/api/star-catalog.api';
import { AstronomicalImage, Star } from '../../features/catalog/models/star.model';

@Component({
  selector: 'app-star-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './star-detail.component.html',
  styleUrl: './star-detail.component.scss'
})
export class StarDetailComponent implements OnChanges {
  private readonly catalogApi = inject(StarCatalogApi);
  private readonly destroyRef = inject(DestroyRef);

  @Input({ required: true }) starId!: string;
  @Output() closed = new EventEmitter<void>();

  star: Star | null = null;
  images: AstronomicalImage[] = [];
  selectedImageUrl: string | null = null;
  loading = true;
  error = '';
  imagesError = '';

  get heroImage(): string | null {
    return this.images[0]?.url ?? null;
  }

  get alternateImageUrls(): string[] {
    const primary = this.heroImage;
    return [...new Set(this.images.flatMap(image => image.urls))]
      .filter(url => !!url && url !== primary)
      .slice(0, 5);
  }

  selectImage(url: string): void {
    this.selectedImageUrl = url;
  }

  close(): void {
    this.closed.emit();
  }

  get hasPhysicalMeasurements(): boolean {
    const item = this.star;
    return !!item && (
      item.parallaxes.length > 0 ||
      item.radialVelocities.length > 0 ||
      item.distances.length > 0 ||
      item.spectralClassifications.length > 0 ||
      item.atmosphericParameters.length > 0 ||
      item.diameters.length > 0 ||
      item.projectedRotationalVelocities.length > 0
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['starId'] || !this.starId)
      return;

    this.loading = true;
    this.error = '';
    this.star = null;
    this.images = [];
    this.selectedImageUrl = null;
    this.imagesError = '';
    forkJoin({
      star: this.catalogApi.getStar(this.starId),
      images: this.catalogApi.getImages(this.starId).pipe(
        catchError(() => {
          this.imagesError = 'Không thể tải ảnh của sao; các thông số vẫn được hiển thị.';
          return of([] as AstronomicalImage[]);
        })
      )
    }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: result => {
        this.star = result.star;
        this.images = result.images;
        this.selectedImageUrl = result.images[0]?.url ?? null;
        if (result.images.length === 0 && !this.imagesError) {
          this.imagesError = 'Sao này chưa có ảnh trong Catalog.';
        }
        this.loading = false;
      },
      error: () => {
        this.error = 'Không thể truy xuất Star entity từ Catalog. Hãy kiểm tra quyền truy cập hoặc thử lại.';
        this.loading = false;
      }
    });
  }
}
