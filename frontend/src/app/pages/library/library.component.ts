import { Component, inject, ElementRef, HostListener, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { DiscoveryService } from '../../features/discovery/services/discovery.service';
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
export class LibraryComponent {

  private readonly discoveryService = inject(DiscoveryService);
  private readonly route = inject(ActivatedRoute);

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

  ngOnInit() {
    this.route.queryParamMap.subscribe(params => {
      this.searchQuery = params.get('search')?.trim() ?? '';
      this.currentPage = 1;
      this.loadSpecimens();
    });
  }

  private loadSpecimens(): void {
    this.contentError = '';
    this.discoveryService.search(
      this.searchQuery || undefined,
      this.selectedCategory === 'all' ? undefined : this.selectedCategory,
      this.currentPage,
      8
    ).subscribe({
      next: result => {
        this.totalPages = result.totalPages;
        this.totalSpecimens = result.totalCount;
        this.specimens = result.items.map(item => ({
            id: item.sourceId,
            sourceType: item.sourceType.toLowerCase(),
            name: item.name,
            badge: item.sourceType.toLowerCase() === 'star'
              ? 'Star entity • Catalog'
              : `${this.getDiscoveryCategory(item.sourceType)} • Catalog`,
            image: item.imageUrl ?? '',
            alt: item.name,
            distance: 'Dữ liệu được lập chỉ mục bởi CosmicWiki',
            description: item.description ?? 'Chưa có mô tả cho hiện vật này.',
            detail: null,
            categories: [this.getDiscoveryCategory(item.sourceType)]
          }));
        if (this.totalSpecimens === 0)
          this.contentError = 'Chưa có dữ liệu hiện vật phù hợp trong thư viện.';
      },
      error: () => {
        this.specimens = [];
        this.contentError = 'Không thể tải dữ liệu thư viện.';
      }
    });
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
    this.currentPage = 1;
    this.loadSpecimens();
  }

  openDossier(specimen: Specimen): void {
    if (specimen.sourceType === 'star') {
      this.selectedStarId = specimen.id;
      return;
    }

    this.selectedSpecimen = specimen;
    this.isDossierOpen = true;
  }

  closeStarDetail(): void {
    this.selectedStarId = null;
  }

  closeDossier(): void {
    this.isDossierOpen = false;
    this.selectedSpecimen = null;
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