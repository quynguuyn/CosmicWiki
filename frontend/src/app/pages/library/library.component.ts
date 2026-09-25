import { Component, inject, ElementRef, HostListener, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DiscoveryService } from '../../features/discovery/services/discovery.service';

interface Specimen {
  id: number;
  name: string;
  badge: string;
  image: string;
  alt: string;
  distance: string;
  description: string;
  detail: string;
  categories: string[];
}

interface LibraryCategory {
  label: string;
  count: number;
}

@Component({
  selector: 'app-library',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './library.component.html',
  styleUrl: './library.component.scss'
})
export class LibraryComponent {

  private readonly discoveryService = inject(DiscoveryService);

  @ViewChild('searchInput')
  searchInput?: ElementRef<HTMLInputElement>;

  searchQuery = '';
  selectedCategory = 'Tất cả';

  currentPage = 1;
  totalPages = 0;
  totalSpecimens = 0;

  isDossierOpen = false;
  isAmbientPlaying = false;

  selectedSpecimen: Specimen | null = null;

  categories: LibraryCategory[] = [];

  specimens: Specimen[] = [];
  contentError = '';

  ngOnInit() {
    this.specimens = [];
    this.discoveryService.search(
      undefined,
      undefined,
      1,
      20
    ).subscribe({
      next: result => {
        this.totalPages = result.totalPages;
        this.totalSpecimens = result.totalCount;
        this.categories = Array.from(
          result.items.reduce((counts, item) => {
            const label = this.getDiscoveryCategory(item.sourceType);
            counts.set(label, (counts.get(label) ?? 0) + 1);
            return counts;
          }, new Map<string, number>())
        ).map(([label, count]) => ({ label, count }));
        if (result.items.length > 0) {
          this.specimens = result.items.map((item, index) => ({
            id: index + 1,
            name: item.name,
            badge: `${item.sourceType} • CosmicWiki`,
            image: item.imageUrl ?? 'assets/images/cosmic-placeholder.svg',
            alt: item.name,
            distance: 'Dữ liệu được lập chỉ mục bởi CosmicWiki',
            description: item.description ?? 'Chưa có mô tả cho hiện vật này.',
            detail: `Mã nguồn: ${item.sourceId}`,
            categories: [this.getDiscoveryCategory(item.sourceType)]
          }));
        } else {
          this.specimens = [];
          this.contentError = 'Chưa có dữ liệu hiện vật trong thư viện.';
        }
      },
      error: error => {
        this.specimens = [];
        this.contentError = 'Không thể tải dữ liệu thư viện.';
        console.error('Discovery error:', error);
      }
    });
  }

  private getDiscoveryCategory(sourceType: string): string {
    const type = sourceType.toLowerCase();
    if (type.includes('galaxy')) {
      return 'Thiên hà';
    }
    if (type.includes('planet')) {
      return 'Ngoại hành tinh';
    }
    if (type.includes('black')) {
      return 'Hố đen & Kỳ dị';
    }
    if (type.includes('solar')) {
      return 'Thái Dương Hệ';
    }
    return 'Tinh vân & Sao';
  }

  get filteredSpecimens(): Specimen[] {
    const keyword = this.searchQuery.trim().toLowerCase();

    return this.specimens.filter(specimen => {
      const matchCategory =
        this.selectedCategory === 'Tất cả' ||
        specimen.categories.includes(this.selectedCategory);

      if (!matchCategory) {
        return false;
      }

      if (!keyword) {
        return true;
      }

      const searchableText = [
        specimen.name,
        specimen.badge,
        specimen.distance,
        specimen.description,
        specimen.detail
      ]
        .join(' ')
        .toLowerCase();

      return searchableText.includes(keyword);
    });
  }

  selectCategory(category: string): void {
    this.selectedCategory = category;
    this.currentPage = 1;
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery = input.value;
    this.currentPage = 1;
  }

  openDossier(specimen: Specimen): void {
    this.selectedSpecimen = specimen;
    this.isDossierOpen = true;
  }

  closeDossier(): void {
    this.isDossierOpen = false;
    this.selectedSpecimen = null;
  }

  toggleAmbientAudio(): void {
    this.isAmbientPlaying = !this.isAmbientPlaying;
  }

  setPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }

    this.currentPage = page;
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

    if (event.key === 'Escape' && this.isDossierOpen) {
      this.closeDossier();
    }
  }

  trackById(
    _index: number,
    specimen: Specimen
  ): number {
    return specimen.id;
  }
}