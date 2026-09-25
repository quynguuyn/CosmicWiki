import { CommonModule } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface Story {
  id: number;
  category: string;
  era: string;
  code: string;
  title: string;
  description: string;
  author: string;
  readTime: number;
  image: string;
  alt: string;
}

@Component({
  selector: 'app-stories',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './stories.component.html',
  styleUrl: './stories.component.scss'
})
export class StoriesComponent {

  // ============================================================
  // UI STATE
  // ============================================================

  selectedCategory = 'all';
  searchTerm = '';
  sortMode = 'newest';

  isModalOpen = false;
  isHeroAudioPlaying = false;
  isModalAudioPlaying = false;
  isAmbientOn = false;

  isSubscribing = false;
  isSubscribed = false;

  selectedStory: Story | null = null;

  // ============================================================
  // CATEGORY DATA
  // ============================================================

  categories = [
    {
      id: 'all',
      label: 'Tất cả biên niên sử',
      count: 24
    },
    {
      id: 'exploration',
      label: 'Lịch sử Thám hiểm',
      count: null
    },
    {
      id: 'astrophysics',
      label: 'Vật lý Thiên văn',
      count: null
    },
    {
      id: 'philosophy',
      label: 'Triết học Vũ trụ',
      count: null
    },
    {
      id: 'modern-observation',
      label: 'Quan sát Hiện đại',
      count: null
    },
    {
      id: 'ancient',
      label: 'Biên niên sử Cổ đại',
      count: null
    }
  ];

  // ============================================================
  // HERO STORY
  // ============================================================

  heroStory: Story = {
    id: 0,
    category: 'exploration',
    era: 'KỶ NGUYÊN VIỄN CHINH 1977',
    code: 'VOYAGER-1977',
    title: 'Đĩa Vàng Voyager: Lời Chào Từ Trái Đất Gửi Đến Bờ Bến Ngân Hà',
    description:
      'Bản sao lưu ký ức vĩ đại nhất của loài người đang lướt đi trong bóng tối liên sao với vận tốc 61,000 km/h, mang theo tiếng sóng vỗ, tiếng chim hót và bản giao hưởng của Beethoven qua khoảng cách hơn 24 tỷ kilômét...',
    author: 'GS. Viện Thiên Văn Học Carl Sagan Archives',
    readTime: 12,
    image:
      'https://thumb.wikimedia.org/wikipedia/commons/thumb/7/7b/The_Sounds_of_Earth_-_GPN-2000-001976.jpg/960px-The_Sounds_of_Earth_-_GPN-2000-001976.jpg?utm_source=vi.wikipedia.org&utm_campaign=imageinfo&utm_content=thumbnail',
    alt:
      'Curated collector badge set of Apollo 11 mission patch and miniature gold plated Voyager Golden Record replica in velvet lined cosmic wooden gift box'
  };

  // ============================================================
  // STORIES
  // ============================================================

  stories: Story[] = [
    {
      id: 1,
      category: 'astrophysics',
      era: 'KỶ NGUYÊN TÁI TỔ HỢP NGUYÊN TỬ',
      code: 'T-380,000Y',
      title:
        'Tiếng Vọng 380,000 Năm Sau Big Bang: Bức Xạ Nền Vi Ba Và Bản Đồ Sơ Khai',
      description:
        'Những hạt photon cổ xưa nhất được giải phóng khi vũ trụ hạ nhiệt, mang theo dấu ấn sơ khai về cấu trúc mạng nhện khổng lồ tạo nên các thiên hà ngày nay.',
      author: 'GS. David Trần',
      readTime: 15,
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuB4yVY2s04bdJnquUaH2m6F7J3Em3vRHXWvrU5ic0lp6NWwfomzHlgl2t-vXqNBFaQd0CStzCT0oNfSPuDpXDLsZfByxKs-lmhKS6bxi4wC1qJHxfql0M86yON2Wbzvf1YASE7v3yefd6Uw_XpT5aAWviqDuUgCoZ6xLTYKtfq1pPHlwmG4fcszhhGmG4xA9OJu8Xv5x9ujoDGcPI3OxFspubJUGHFWcTkl4uIwm7R1gH9KK924BhYtveIUyY59QzhoSqzu99R_Ug',
      alt:
        'Astrophysics visualization of the cosmic microwave background radiation map depicting subtle temperature ripples of the primordial universe'
    },
    {
      id: 2,
      category: 'modern-observation',
      era: 'KỶ NGUYÊN HỒNG NGOẠI L2',
      code: 'JWST-L2',
      title:
        'Kính Viễn Vọng James Webb Và Bí Ẩn Về Những Thiên Hà Quá Lớn Quá Sớm',
      description:
        'Những phát hiện mới từ kính James Webb đang thách thức mô hình chuẩn: Làm thế nào các thiên hà khổng lồ có thể hình thành chỉ vài trăm triệu năm sau vụ nổ Big Bang?',
      author: 'TS. Elena Vũ',
      readTime: 10,
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuCuF3L-htdx_BCSDNgkkjAAHjsPREANEb-BpQHYANxAMlpWG6LyhsRGhjlMn6TWMaAmh5nsjUiW-2iF0d1i0bc07_LxwF3coHCmNbXK6X5uApsrgsIqUVxOzjZcVEFtZ7rMdfC3udgP3_LZjRVcMKxFpW8A9gXx2GLnKVopCcw53d3jjw2GO-V_a_xtZ6N1p9k9gyItIhv2kxZaPP8zs_Ot2yXkpyDMU4FxfD8s1mWWYVCaN4nKydJaDPjpIQZqOOGiF2W4vjwEqQ',
      alt:
        'High precision astronomical photograph of deep field galaxies through an infrared space telescope'
    },
    {
      id: 3,
      category: 'exploration',
      era: 'KẾT THÚC GRAND FINALE',
      code: 'CASSINI-2017',
      title:
        'Vũ Điệu Tự Sát Của Tàu Cassini: Những Giây Cuối Cùng Trong Khí Quyển Sao Thổ',
      description:
        'Để bảo vệ các mặt trăng có thể nuôi dưỡng sự sống khỏi ô nhiễm sinh học, con tàu 20 năm tuổi đã hy sinh thân mình trong ngọn lửa rực sáng ở tầng khí quyển Sao Thổ.',
      author: 'Kỹ sư Marcus Vance',
      readTime: 14,
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuBV4mjcELyvTBOmrqhQ2-0H63us-Qe2iGG-7BIR9BlggTPaW6XEBF7Hq2U96UzYWdt4k5TXWEXMpe4Ur_zuN-xNnicnoZL83kr6hHffddYKlO1Oq1t7NRlvLwjpjOIzZakR_7vBrLOxd_rFX6SoMcVu_aoX3KLCVzA5_w-xb4oTejhHjOTZZtkOxH3B24VvmfRvr3ABFU_pPeLMqST3WNefT6ar2i2wC8vwVls5i2wMgjespqbzoDcRKGBwRT4ckOvI5XQrU3dtLQ',
      alt:
        'Dramatic spacecraft diving into Saturn golden cloud decks'
    },
    {
      id: 4,
      category: 'philosophy',
      era: 'BIÊN GIỚI LƯỢNG TỬ',
      code: 'QUANTUM-HAWKING',
      title:
        'Nghịch Lý Mất Mát Thông Tin Hố Đen: Stephen Hawking Và Trận Chiến Lượng Tử',
      description:
        'Khi vật chất rơi vào hố đen và bốc hơi qua bức xạ Hawking, liệu mọi lịch sử và thông tin cấu tạo có thực sự biến mất vĩnh viễn khỏi đa vũ trụ?',
      author: 'GS. Akira Sato',
      readTime: 18,
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuBenPCX0VTD4N5j_98PSa92snZ5r7gxXyOq1IAbZIUoGSN6xDc3TxbPKf36S-UJZRjQWgMTVAfT6ZBFOkAwgBR1aNkf0qAFva9icPH48hMyi4AK9G4l-dRSDgYCfqo1pQvrLhE1X8Bm_MmtUbVZnlgtFZLO044VDzJVhgmaQC-i5Wp06FpQdeDLjRJrjR3bZAcK0tDCpCRC3WZGh939LVxFbW--VKgZqg_hroGCg5hQChhakj-PnqwLhb1EpTcuiiNxQ5YNakJjSQ',
      alt:
        'Theoretical physics visualization of a rotating supermassive black hole event horizon'
    },
    {
      id: 5,
      category: 'ancient',
      era: 'KHO BÁU KHẢO CỔ THIÊN VĂN',
      code: 'CIRCA-15,000 BCE',
      title:
        'Bản Đồ Sao Hang Động Lascaux: Khi Người Tiền Sử Vẽ Bầu Trời Đêm',
      description:
        'Những chấm màu đất son trên vách đá 17,000 năm trước hé lộ loài người đã nhận diện chòm sao Kim Ngưu và chùm sao Thất Nữ từ thuở bình minh của tư duy.',
      author: 'Nhà khảo cổ Jean-Luc Bernard',
      readTime: 9,
      image:
        'https://lh3.googleusercontent.com/aida/AEtjO1USpgh-MdmpLaNiZjXx9GjFYBHre6fVE5pT1pj9dfdnqdhbDaABx_NjPe2_d3CSI7ipnQoGIPmr8Gyf69m3tKohB6_rh1uvdmGX-GXpuzhcVjTHiB4qmiJ2NaszAGMe1CGCyqblqy2jacdSM6bBUAqZ9287tBMC_VQwxDOwBs3fK4GczsTYp1TjZqkP54AAIe4Qfyz7hKxzYwqA5bQjGx-JGDVtiITdvR7Qbf9g7b0kQYi1VYbZLQSU',
      alt:
        'Rare Albrecht Durer northern celestial hemisphere woodcut star map print on aged museum parchment'
    },
    {
      id: 6,
      category: 'astrophysics',
      era: 'SINH HỌC NGOÀI TRÁI ĐẤT',
      code: 'EUROPA-CLIPPER',
      title:
        'Đại Dương Ngầm Mặt Trăng Europa: Nơi Nào Trong Hệ Mặt Trời Có Thể Có Sự Sống?',
      description:
        'Dưới lớp băng vĩnh cửu dày 20 km của Europa là một đại dương ấm áp có thể chứa lượng nước gấp đôi toàn bộ các đại dương trên Trái Đất cộng lại.',
      author: 'TS. Sarah Lin',
      readTime: 11,
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuBYYkP5TMB4E5PJFqjo3WdOAcpxJo4Y9AGKDCKYG4DDafPohK8ENbzVf1xbaExXxemNURMrhxceJ48kAgIxPw7pDe1TKK_Tadbzlf6bSmKY15QsHFS9GOeDh4PtQnnkABM4PSqsOan2F1wjarbSpnyLZ2UDK1FyZFhkpyzfKJYVPzXI3brKGI_4MCwT1b6u0YK2SUTlfUOYYJj8TnxQnIHezdy5WLVe4EWkySVhOU2g8jW96o7djho867ZrXrXRIOW2Z7AU-DO8tw',
      alt:
        'Astrophotography cutaway illustration of Jupiter icy moon Europa showing cracked blue frozen surface'
    }
  ];

  // ============================================================
  // GET CATEGORY LABEL
  // ============================================================

  getCategoryLabel(category: string): string {
    const found = this.categories.find(item => item.id === category);
    return found?.label ?? category;
  }

  // ============================================================
  // FILTERED STORIES
  // ============================================================

  get filteredStories(): Story[] {
    let result = [...this.stories];

    if (this.selectedCategory !== 'all') {
      result = result.filter(
        story => story.category === this.selectedCategory
      );
    }

    const keyword = this.searchTerm.trim().toLowerCase();

    if (keyword) {
      result = result.filter(story =>
        story.title.toLowerCase().includes(keyword) ||
        story.description.toLowerCase().includes(keyword) ||
        story.author.toLowerCase().includes(keyword) ||
        story.era.toLowerCase().includes(keyword) ||
        story.code.toLowerCase().includes(keyword)
      );
    }

    switch (this.sortMode) {
      case 'popular':
        return [...result].sort((a, b) => a.readTime - b.readTime);

      case 'duration':
        return [...result].sort((a, b) => b.readTime - a.readTime);

      case 'newest':
      default:
        return result;
    }
  }

  // ============================================================
  // CATEGORY
  // ============================================================

  selectCategory(category: string): void {
    this.selectedCategory = category;
  }

  // ============================================================
  // SEARCH
  // ============================================================

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm = input.value;
  }

  // ============================================================
  // SORT
  // ============================================================

  onSortChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sortMode = select.value;
  }

  // ============================================================
  // MODAL
  // ============================================================

  openModal(story: Story = this.heroStory): void {
    this.selectedStory = story;
    this.isModalOpen = true;
    this.isModalAudioPlaying = false;

    document.body.style.overflow = 'hidden';
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.isModalAudioPlaying = false;

    document.body.style.overflow = 'auto';
  }

  // ============================================================
  // AUDIO
  // ============================================================

  toggleHeroAudio(): void {
    this.isHeroAudioPlaying = !this.isHeroAudioPlaying;
  }

  toggleModalAudio(): void {
    this.isModalAudioPlaying = !this.isModalAudioPlaying;
  }

  toggleAmbient(): void {
    this.isAmbientOn = !this.isAmbientOn;
  }

  // ============================================================
  // NEWSLETTER
  // ============================================================

  subscribe(email: string): void {
    if (!email || this.isSubscribing) {
      return;
    }

    this.isSubscribing = true;
    this.isSubscribed = false;

    setTimeout(() => {
      this.isSubscribing = false;
      this.isSubscribed = true;
    }, 700);

    setTimeout(() => {
      this.isSubscribed = false;
    }, 4700);
  }

  // ============================================================
  // MODAL BACKDROP
  // ============================================================

  onModalBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeModal();
    }
  }

  // ============================================================
  // KEYBOARD
  // ============================================================

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isModalOpen) {
      this.closeModal();
    }
  }

  // ============================================================
  // CLEANUP
  // ============================================================

  ngOnDestroy(): void {
    document.body.style.overflow = 'auto';
  }
}