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
  readonly totalPages = 156;
  readonly totalSpecimens = 1248;

  isDossierOpen = false;
  isAmbientPlaying = false;

  selectedSpecimen: Specimen | null = null;

  readonly categories = [
    {
      label: 'Tất cả',
      count: 1248
    },
    {
      label: 'Thái Dương Hệ',
      count: 142
    },
    {
      label: 'Tinh vân & Sao',
      count: 418
    },
    {
      label: 'Thiên hà',
      count: 386
    },
    {
      label: 'Hố đen & Kỳ dị',
      count: 84
    },
    {
      label: 'Ngoại hành tinh',
      count: 218
    }
  ];

  specimens: Specimen[] = [
    {
      id: 1,
      name: 'Tinh vân Đầu Ngựa (Barnard 33 - Orion B)',
      badge: 'B33 • Tinh vân tối',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuBiAr6kzkAlUBunBzkBupXyabqvbQkxM--zi2sxwslmP1dvblKQwEMXVp80iLZ4tmEr5OMixoyftNs5_0djm_Yj-ehkPnxvS7oCfzDt4m_UvoO-H9NzQ7oBgggwwTs03Ld3GWIiA040qrOMSGxFbmSFV-17trgNWhqgl9iT6niTU4GrcMlP0D6Qhpb_VjdfdugS6SBM9ZjCHPbyekukM2adDusDb9E6xfdBX8as-UhNAgV8Svw2Om_jjpMzYVH5STQa8ERNTccRRQ',
      alt: 'An ethereal deep space photograph of the dark Horsehead Nebula Barnard 33 silhouetted against glowing crimson ionized hydrogen gas.',
      distance: 'Cách 1,375 năm ánh sáng • Chòm sao Lạp Hộ',
      description:
        'Đám mây bụi sẫm uốn lượn trước bức xạ rực lửa của ngôi sao Sigma Orionis, che chắn ánh sáng tạo hình bóng ngựa vĩ đại.',
      detail: 'Quang phổ: H-Alpha',
      categories: ['Tinh vân & Sao']
    },

    {
      id: 2,
      name: 'Thiên hà Tiên Nữ (Andromeda - Messier 31)',
      badge: 'M31 • Thiên hà xoắn ốc',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuDk53LFWdqFpbm70-UmoJOPUrTjdfsjTjAElRoZFlA3NMxwxVtGES705_XP37vTHHYvaLbkbrmzt-qfib6aIW-COwahB1it3nNsbZ9lDBC6NNH8Kovn6zfX_yhmJr9oOX8p2JStk3Z2l7TupOODO9NOxQSwi2Sn1dDJx-0iLKuDWFMrcA3iAiG7rvexerTxXFvHK4b2HFagfC325pvfyOxLRBDibp5vjRYXJ8XbgbSi9WeAXGyibolwIKUVi0bDwJeVtX8QaVGhew',
      alt: 'Magnificent spiral galaxy of Andromeda Messier 31 spinning in cosmic void with billions of twinkling stars and azure spiral dust lanes.',
      distance: 'Cách 2.5 triệu năm ánh sáng • Tiên Nữ',
      description:
        'Thiên hà láng giềng khổng lồ chứa hơn 1 nghìn tỷ ngôi sao đang tiến về Ngân Hà với vận tốc 110 km/s trong vũ điệu không gian.',
      detail: 'Độ lệch đỏ: -0.001',
      categories: ['Thiên hà']
    },

    {
      id: 3,
      name: 'Hố đen Siêu khối lượng Messier 87* (M87*)',
      badge: 'M87* • Hố đen siêu khối',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuBAFt-LkdZdOiEgfdb52efw7-uvKnZNNywamUFWDS0nM8wxxBgUMFZ45L-X3k1D1k5AyBLeoWgTlwcvPMVQ2j1rklOtKtNNCaUdHkOtCjXhRYg5Qo3OFTvlR1QX7EC2F4hcPlInMkDxbyusO8kaa44InMlCkhc0b0XC0UYdqOD4Md0-Wxk3tkjqc6qdeXSSkmEBSj3Lcd1o2YXoScdOAlGBMEUHYIAR4msqJ9a_iZ5V4yaq3c4OY7LCSKDhi2rGRSAI0UWx0TS-3Q',
      alt: 'Event Horizon Telescope capture visualization of Supermassive Black Hole Messier 87 star with a fiery ring of relativistic accretion disk.',
      distance: 'Cách 53.5 triệu năm ánh sáng • Chòm Xử Nữ',
      description:
        'Khối lượng gấp 6.5 tỷ lần Mặt Trời với đĩa bồi tụ bức xạ tương đối tính uốn cong không thời gian được mạng lưới EHT ghi lại.',
      detail: 'Bán kính: 120 AU',
      categories: ['Hố đen & Kỳ dị']
    },

    {
      id: 4,
      name: 'Cột Sáng Sáng Tạo (Pillars of Creation - M16)',
      badge: 'M16 • Vùng ươm mầm sao',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuBraD01LoGXd_uFn0dd6awdwie5t_aa9nhHRITc-0meVmx0DNFcS4ARrkOPCkLGB20M99--Fp_u_rMfYEMM6nWiNSBqup6MZBqcjECeLVF-UPp518BypvbuoLrXrbtrM2Ku7k2UUmqzakMhdBaeJvT1mhjH4_jHhQMF1LJ6GYerW5EkxU6uEgEzwNzSoTMlAWOaJ4RJuH5y888z-BJxstAl01I2s75ioWtu5xgjhGWNheQarLveYCpTLmnm1WLr2qCkqC2fKxFvgQ',
      alt: 'James Webb and Hubble composite view of Pillars of Creation in Eagle Nebula M16.',
      distance: 'Cách 6,500 năm ánh sáng • Chòm Cự Xà',
      description:
        'Những cột khí hydro lạnh ngắt và bụi dày đặc đang thai nghén hàng vạn ngôi sao sơ sinh dưới áp suất bức xạ cực quang lấp lánh.',
      detail: 'Cảm biến: NIRCam',
      categories: ['Tinh vân & Sao']
    },

    {
      id: 5,
      name: 'Sao Mộc & Đại Hồng Bào (Jupiter & Great Red Spot)',
      badge: 'JUPITER • Khí khổng lồ',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuDKBajQsm9VqijctjElCPrKBPWDDlRX4Vn6VMlhtBdHyBNF3lZCiAGEuiL0rIppgv0NPVRGxDJ79d-lWeDLqr8aJ2NSwoQVgbWSvCGYUNMUI_5Kqzv1Ia4VyJxZXTXDLF8h751cCgw0gV0fTmzFEl87W7zRVSOmzEGlfOwo6KGz5Jgsfl9zoR4zFIau5WNGqVuap4xTMbdYe2T3Iz4PBpCca3sdlHIBj6DG1KBN62Zvd2jdkDgwhQ1P8FaJabERBmEcbZFHzUEuyg',
      alt: 'Stunning close-up view of planet Jupiter displaying atmospheric swirling bands and the Great Red Spot.',
      distance: 'Cách 628 triệu km • Thái Dương Hệ',
      description:
        'Cơn bão xoáy nghịch khổng lồ đã cuộn trào hàng thế kỷ trên bề mặt khí quyển quyển xoắn với sức gió vượt 432 km/h.',
      detail: 'Đường kính: 1.3x TĐ',
      categories: ['Thái Dương Hệ']
    },

    {
      id: 6,
      name: 'Ngoại hành tinh TRAPPIST-1e',
      badge: 'TRAPPIST-1e • Ngoại hành tinh',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuAB3mLHdyZKc66IzrfhQ6i33A0erYZMkFPn8w0KVkFbHFQmp3frLjl4rjfkTCxqi1UzftZFbPEnhjMsYM7Q6T7yfCfhR5HfMQfdeUx68PwH4mc0-2S6_fiE_SSPtp5U55jQEi7BYuKdt633wuybWykPQlx_6CqRWkkLc9wRQACcqcNILF59ANgjcJqpf9CvO9kQR6V7b5z0huzhhs5_C9PUy8xES-bZG1KYsbp_xh6vxIjkH51dtclYmQakwGs3AMjfYwT3oPcEEw',
      alt: 'Artistic scientific rendering of Earth-sized exoplanet TRAPPIST-1e.',
      distance: 'Cách 39.5 năm ánh sáng • Chòm Bảo Bình',
      description:
        'Nằm trọn vẹn trong vùng có thể sống được (Habitable Zone) với tiềm năng lưu giữ nước lỏng và bầu khí quyển dày che chắn.',
      detail: 'Bán kính: 0.92 R⊕',
      categories: ['Ngoại hành tinh']
    },

    {
      id: 7,
      name: 'Tinh vân Chiếc Nhẫn (Ring Nebula - Messier 57)',
      badge: 'M57 • Tinh vân hành tinh',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuAlS_wA0-GGhokK3Y2CfIOWEOQ8XN4jGCFKqdqgnIsSkMyOW7F9jxZbimjuG5qYq1_Kk_xT-G49A9mdnXMJptRcl5R3Evp236w5Mkb9xAOnGTLrgGjEAmXh5sdMKi6FBX18KEtZr_uz2iA78xQsyfK3h-YJwYTcZPD0XYi3XEPwk34G38tWiKEDvUdgGE2uMqFSMq8GyJJufuhvrBeT8Gnz9hYHkAHE_s0Ecxdjp6KG0nwMufVQfCaNK1eLcKE-WTmC1kqjEc1yVw',
      alt: 'Ring Nebula Messier 57 captured in deep space with glowing concentric rings.',
      distance: 'Cách 2,570 năm ánh sáng • Chòm Thiên Cầm',
      description:
        'Vỏ bọc khí phát quang rực rỡ thoát ra từ một ngôi sao lùn trắng đang tàn lụi, để lộ cấu trúc hình xuyến mở rộng ngoạn mục.',
      detail: 'Bán kính: 1.3 ly',
      categories: ['Tinh vân & Sao']
    },

    {
      id: 8,
      name: 'Thiên hà Chong Chóng (Pinwheel - Messier 101)',
      badge: 'M101 • Xoắn ốc đối xứng',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuA8hPRT1vsjUY3_K5ZRAbbDJD0ByN_BcffJBa_JIGmuxaXIpIrCtFK5kI1W55rm0LJDKxbCcuiNaRAqob67ruFwQCKo635CSMl70Fz-SENHtlDEgCvAGR20karmucfhD8Kg6h4Fe1uVUmWUli9PYNDrTxkeR0LWLQu0hIzhxN3989lDlov2fIEnGlnJafsbNAFHMlMAlrVEWXb9fncV1OAvVXQxeMh6oVIWk7paaWMWUWSHIsXhjf3fxXdHGRTzQdnj8P0_M8fdQw',
      alt: 'Astrophotography of Pinwheel Galaxy Messier 101 face-on spiral view.',
      distance: 'Cách 21 triệu năm ánh sáng • Chòm Đại Hùng',
      description:
        'Đĩa xoắn ốc gần như đối xứng hoàn hảo với các cánh tay đầy ắp hydro ion hóa H II và các cụm sao trẻ siêu sáng rải rác.',
      detail: 'Đường kính: 170k ly',
      categories: ['Thiên hà']
    }
  ];

  ngOnInit() {
    this.discoveryService.search(
      undefined,
      undefined,
      1,
      20
    ).subscribe({
      next: result => {
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
        }
      },
      error: error => {
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