import { Component, DestroyRef, ElementRef, HostListener, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject, Subscription } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DiscoveryService } from '../../features/discovery/services/discovery.service';
import { SearchResultResponse } from '../../features/discovery/models/search-result-response.model';
import { StarCatalogApi } from '../../features/catalog/api/star-catalog.api';
import { AstronomicalObject } from '../../features/catalog/models/star.model';
import { StarDetailComponent } from './star-detail.component';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';

interface Specimen {
  id: string;
  sourceType: string;
  name: string;
  badge: string;
  image: string;
  alt: string;
  distance: string;
  description: string;
  detail: string | null;
  categories: string[];
}

interface LibraryCategory {
  id: string;
  label: string;
}

@Component({
  selector: 'app-library',
  standalone: true,
  imports: [CommonModule, StarDetailComponent, PaginationComponent],
  templateUrl: './library.component.html',
  styleUrl: './library.component.scss'
})
export class LibraryComponent implements OnInit, OnDestroy {

  private readonly discoveryService = inject(DiscoveryService);
  private readonly catalogApi = inject(StarCatalogApi);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchInput$ = new Subject<string>();
  private searchSubscription?: Subscription;
  private objectSubscription?: Subscription;
  private relatedSubscription?: Subscription;
  private lastRequestedSearch = '';

  @ViewChild('searchInput')
  searchInput?: ElementRef<HTMLInputElement>;

  searchQuery = '';
  selectedCategory = 'all';

  currentPage = 1;
  totalPages = 0;
  totalSpecimens = 0;

  isDossierOpen = false;
  selectedSpecimen: Specimen | null = null;
  selectedStarId: string | null = null;
  selectedAstronomicalObject: AstronomicalObject | null = null;
  relatedSpecimens: Specimen[] = [];
  detailLoading = false;
  detailError = '';
  relatedError = '';
  loading = false;

  categories: LibraryCategory[] = [
    { id: 'all', label: 'Tất cả' },
    { id: 'star', label: 'Sao' },
    { id: 'galaxy', label: 'Thiên hà' },
    { id: 'planet', label: 'Hành tinh' },
    { id: 'other', label: 'Khác' }
  ];

  specimens: Specimen[] = [];
  contentError = '';
  viewMode: 'grid' | 'list' = 'grid';

  get firstVisibleSpecimen(): number {
    return this.totalSpecimens === 0 ? 0 : (this.currentPage - 1) * 8 + 1;
  }

  get lastVisibleSpecimen(): number {
    return Math.min(this.currentPage * 8, this.totalSpecimens);
  }

  ngOnInit(): void {
    this.searchInput$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(query => {
      if (query === this.lastRequestedSearch) return;
      this.searchQuery = query;
      this.currentPage = 1;
      this.loadSpecimens();
    });

    this.route.queryParamMap.subscribe(params => {
      this.searchQuery = params.get('search')?.trim() ?? '';
      this.currentPage = 1;
      this.searchInput$.next(this.searchQuery);
      this.loadSpecimens();
    });
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    this.objectSubscription?.unsubscribe();
    this.relatedSubscription?.unsubscribe();
  }

  private loadSpecimens(): void {
    this.contentError = '';
    this.loading = true;
    this.lastRequestedSearch = this.searchQuery;
    this.searchSubscription?.unsubscribe();
    this.searchSubscription = this.discoveryService.search(
      this.searchQuery || undefined,
      this.selectedCategory === 'all' ? undefined : this.selectedCategory,
      this.currentPage,
      8
    ).subscribe({
      next: result => {
        this.loading = false;
        this.totalPages = result.totalPages;
        this.totalSpecimens = result.totalCount;
        this.specimens = result.items.map(item => this.mapSpecimen(item));
        if (this.totalSpecimens === 0)
          this.contentError = 'Chưa có dữ liệu hiện vật phù hợp trong thư viện.';
      },
      error: () => {
        this.loading = false;
        this.totalPages = 0;
        this.totalSpecimens = 0;
        this.specimens = [];
        this.contentError = 'Không thể tải dữ liệu thư viện.';
      }
    });
  }

  private mapSpecimen(item: SearchResultResponse): Specimen {
    const sourceType = item.sourceType.toLowerCase();
    return {
      id: item.sourceId,
      sourceType,
      name: item.name,
      badge: sourceType === 'star'
        ? 'Star entity • Catalog'
        : `${this.getDiscoveryCategory(sourceType)} • Catalog`,
      image: item.imageUrl ?? '',
      alt: item.name,
      distance: 'Dữ liệu được lập chỉ mục bởi CosmicWiki',
      description: item.description ?? 'Chưa có mô tả cho hiện vật này.',
      detail: item.catalog && item.catalogValue
        ? `${item.catalog} ${item.catalogValue}`
        : null,
      categories: [this.getDiscoveryCategory(sourceType)]
    };
  }

  private getDiscoveryCategory(sourceType: string): string {
    const type = sourceType.toLowerCase();
    if (type.includes('star')) {
      return 'Sao';
    }
    if (type.includes('galaxy')) {
      return 'Thiên hà';
    }
    if (type.includes('planet')) {
      return 'Ngoại hành tinh';
    }
    if (type === 'other') {
      return 'Khác';
    }
    return sourceType;
  }

  get filteredSpecimens(): Specimen[] {
    return this.specimens;
  }

  selectCategory(category: string): void {
    this.selectedCategory = category;
    this.currentPage = 1;
    this.loadSpecimens();
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery = input.value;
    this.searchInput$.next(this.searchQuery);
  }

  openDossier(specimen: Specimen): void {
    if (specimen.sourceType === 'star') {
      this.closeDossier();
      this.selectedStarId = specimen.id;
      return;
    }

    this.selectedSpecimen = specimen;
    this.isDossierOpen = true;
    this.selectedAstronomicalObject = null;
    this.detailLoading = true;
    this.detailError = '';
    this.relatedSpecimens = [];
    this.relatedError = '';
    this.objectSubscription?.unsubscribe();
    this.relatedSubscription?.unsubscribe();
    this.objectSubscription = this.catalogApi.getAstronomicalObject(specimen.id).subscribe({
      next: object => {
        this.selectedAstronomicalObject = object;
        this.detailLoading = false;
      },
      error: () => {
        this.detailError = 'Không thể tải thông tin chi tiết từ Catalog.';
        this.detailLoading = false;
      }
    });
    this.relatedSubscription = this.discoveryService.search(
      undefined,
      specimen.sourceType,
      1,
      5
    ).subscribe({
      next: result => {
        this.relatedSpecimens = result.items
          .filter(item => item.sourceId !== specimen.id)
          .map(item => this.mapSpecimen(item));
      },
      error: () => {
        this.relatedError = 'Không thể tải các thiên thể cùng loại.';
      }
    });
  }

  closeStarDetail(): void {
    this.selectedStarId = null;
  }

  closeDossier(): void {
    this.isDossierOpen = false;
    this.selectedSpecimen = null;
    this.objectSubscription?.unsubscribe();
    this.relatedSubscription?.unsubscribe();
    this.selectedAstronomicalObject = null;
    this.relatedSpecimens = [];
  }

  setPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) {
      return;
    }

    this.currentPage = page;
    this.loadSpecimens();
  }

  setViewMode(mode: 'grid' | 'list'): void {
    this.viewMode = mode;
  }

  exportDossier(): void {
    if (!this.selectedSpecimen) return;

    const dossier = {
      name: this.selectedSpecimen.name,
      categories: this.selectedSpecimen.categories,
      source: this.selectedSpecimen.badge,
      imageUrl: this.selectedSpecimen.image,
      description: this.selectedSpecimen.description,
      sourceReference: this.selectedSpecimen.detail,
      catalog: this.selectedAstronomicalObject?.catalog,
      catalogValue: this.selectedAstronomicalObject?.catalogValue,
      objectType: this.selectedAstronomicalObject?.objectType,
      rightAscensionDegrees: this.selectedAstronomicalObject?.rightAscensionDegrees,
      declinationDegrees: this.selectedAstronomicalObject?.declinationDegrees,
      exportedAtUtc: new Date().toISOString()
    };
    const blobUrl = URL.createObjectURL(
      new Blob([JSON.stringify(dossier, null, 2)], { type: 'application/json' })
    );
    const link = document.createElement('a');
    link.href = blobUrl;
    link.download = `cosmicwiki-${this.selectedSpecimen.name.toLowerCase().replace(/[^a-z0-9]+/g, '-')}.json`;
    link.click();
    URL.revokeObjectURL(blobUrl);
  }

  @HostListener('document:keydown', ['$event'])
  handleKeyboard(event: KeyboardEvent): void {
    if (
      (event.ctrlKey || event.metaKey) &&
      event.key.toLowerCase() === 'k'
    ) {
      event.preventDefault();
      this.searchInput?.nativeElement.focus();
    }

    if (event.key === 'Escape') {
      if (this.selectedStarId)
        this.closeStarDetail();
      else if (this.isDossierOpen)
        this.closeDossier();
    }
  }

  trackById(
    _index: number,
    specimen: Specimen
  ): string {
    return specimen.id;
  }
}