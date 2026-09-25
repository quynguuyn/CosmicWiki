import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { Product } from '../../features/product/models/product.model';
import { ProductService } from '../../features/product/services/product.service';

interface StoreProduct {
  id: string;
  title: string;
  category: 'meteorite' | 'map' | 'instrument' | 'badge';
  categoryLabel: string;
  origin: string;
  epoch: string;
  code: string;
  price: number;
  rarity: number;
  badge: string;
  image: string;
  desc: string;
}

interface CartItem {
  productId: string;
  title: string;
  price: number;
  image: string;
  qty: number;
}

@Component({
  selector: 'app-store',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './store.component.html',
  styleUrl: './store.component.scss'
})
export class StoreComponent {

  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  products: StoreProduct[] = [
    {
      id: 'sample-0',
      title: 'Mảnh Thiên thạch Gibeon Cổ đại (Namibia - Hợp kim sắt-niken 4.5 tỷ năm)',
      category: 'meteorite',
      categoryLabel: 'Mảnh Thiên thạch',
      origin: 'Namibia, Châu Phi',
      epoch: '4.5 Tỷ Năm (Hệ Mặt Trời sơ khai)',
      code: 'GB-4500',
      price: 3850000,
      rarity: 5,
      badge: 'Niên đại 4.5 Tỷ năm',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCPGZmvjgpmLd2Kxhwplzvo1hp1XOjJXd7hsmuGAfKwBEn8SMWEnDrSahVfln0OumbqHSKvj6yvooVkrJxXvTEgdK-bWRD1rMVRYS103HxuI02NBifurGsBGZ1dyfNEr4MmQBF7zmfqic51ilOLA5x1WXTJx6shD4xQ1ob5PCDLbIJB-yr_SOte04ERcYfn-rRk2d_Kxu2NRBHvdvvEmhx-ly8FAAzXjzayw0lmO9AplnD6_9UOzeiCkns6kKZHmsD_S3wLFplMJw',
      desc: 'Mảnh vỡ tự nhiên của thiên thạch Gibeon với cấu trúc Widmanstätten độc nhất vô nhị. Cấu trúc tinh thể dạng lưới bát diện này không thể nhân tạo trên Trái Đất vì cần hàng triệu năm để nguội lạnh trong môi trường vi trọng lực.'
    },
    {
      id: 'sample-1',
      title: 'Bản đồ Sao Thiên Văn Cổ Galileo (Bản in phục chế 1610 trên giấy da)',
      category: 'map',
      categoryLabel: 'Bản đồ & Bản họa Cổ',
      origin: 'Florence, Ý',
      epoch: 'Năm 1610 (Thế kỷ 17)',
      code: 'GL-1610',
      price: 1250000,
      rarity: 3,
      badge: 'Ấn bản Phục chế 1610',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCIBZEOrc8zSK-2kWGkZIKeX1pfKmGeVNGlAD0PlBZz6CHcIRay8ovboxjAmpyQq4NnFzCe9eaOH_5gxisC6efIbVoXTb2uQvwxDtp9-IRIhbuISMGmRpDt-lqd_dA6rY9fBXbJakqYYWojg3BNBKCYq3MZFmgLQ_yIKZMcAbCPldvSPtEFKBtqeIFK08Fb5dBKOBvJ2IXmTmg3jUJME3I2odYyhWEBuWKsIpUcKQ4XGCMKfGRKVusRh_LomSoftuQbLnrAq8Wr2Q',
      desc: 'In thủ công trên giấy da cổ già hóa sinh học, tái hiện các quan sát vĩ đại đầu tiên của Galileo Galilei bằng kính thiên văn, bao gồm bốn mặt trăng Galilean quay quanh Sao Mộc.'
    },
    {
      id: 'sample-2',
      title: 'Thước Trắc Tinh Đồng Thau (Astrolabe Thế kỷ 16 khắc hoa văn Ả Rập)',
      category: 'instrument',
      categoryLabel: 'Khí cụ Thiên văn Đồng thau',
      origin: 'Trung Đông Cổ đại',
      epoch: 'Thế kỷ 16',
      code: 'AST-1540',
      price: 5400000,
      rarity: 5,
      badge: 'Chế tác Thủ công',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDq-K8VzkxDDZNzJZ8glN7T1Y9XC5Mxo9EgwGafiCXEl6wKwG4En5373g-v8eR_ofDKDVoJk3eGzy4deezIsjCKhiRgsjoWA8P7LgPyQvlZkItiqjAptNex2Wn5bKUfr10o-EL9PUUNMaI9GBmFuWitk4VyXLh1wP7akpCb89VCtfLVi-_8xJ8Plx-lq1PnXl_Hxrs0984YmnwUVbVB9t-P_Du1ee_bXnwIGoFIoQps-WJ9Xa0x1S572nverUnOHsE4nM_Wi1SEcg',
      desc: 'Khí cụ định vị thiên văn bằng đồng thau đúc nguyên khối với các đĩa xoay có thể tính toán thời gian, vĩ độ và vị trí của hơn 30 ngôi sao sáng nhất trên bầu trời đêm.'
    },
    {
      id: 'sample-3',
      title: 'Quả cầu Thiên thể Armillary Sphere Đồng thau mạ vàng',
      category: 'instrument',
      categoryLabel: 'Khí cụ Thiên văn Đồng thau',
      origin: 'Châu Âu Phục Hưng',
      epoch: 'Thế kỷ 17 - 18',
      code: 'ARM-1720',
      price: 4200000,
      rarity: 4,
      badge: 'Mạ Vàng & Gỗ Mun',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAP14M9pQ1MLcN2wOxZ6H2VCeo7rDJk4o3VHdbeFs0tQ3wLTXDtYDEL04QnY93ZXcHzGD2bdbrpUm8cyUvUqqfsul7_tdYefWUVG_S7tOMeOirS59rKkpaoAPlZgz7gmZRPZPmuVv9gwAKU-2uA2wDR9FOwe4_4z65WFLnwVRyryOoOhfu6sBemjh9FVMlZetDNCTGiHLBqMbfylU0aGgf01VhvrRDtFRjCunbKRX0AhX21pRWGzF7gKqpXo-FFDggKazRN6pB8mw',
      desc: 'Khí cụ biểu diễn vũ trụ địa tâm với các vòng đai đồng thau chạm khắc 12 chòm sao hoàng đạo, đặt trên giá đỡ gỗ mun được đánh bóng thủ công theo phong cách quý tộc.'
    },
    {
      id: 'sample-4',
      title: 'Mảnh Đá Mặt Trăng NWA 11273 (Được kiểm định viện địa chất)',
      category: 'meteorite',
      categoryLabel: 'Mảnh Thiên thạch',
      origin: 'Tây Bắc Châu Phi',
      epoch: 'Bản địa Mặt Trăng',
      code: 'NWA-11273',
      price: 8900000,
      rarity: 5,
      badge: 'Cực Kỳ Quý Hiếm',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCz8eUPTmKzFGhLtGYo3B5GyNvv7S6isKzWqEeEJ2KgqvxjnX6mUpo1N1MjEF46WR4vafvgIzLCT0axvNUb5ck5gZOGEc4dyxCvCpyZ7KpyXk_f_gg6tWWezlxtX2ucGD_S6vmNf1Aipqxa0DqYxK7vb-YLPnCXYhdzW4KSf9Q1halJkiN_JKkk93RC_HaUqrygRrQPqccTsGE_EgHv19ADmBF-WvfXWfrvdqMYOEozLSRQz0hleBu4RnGHxjc9jPhTo2wnQ99yCw',
      desc: 'Lát cắt cực kỳ hiếm từ thiên thạch mặt trăng NWA 11273 (Feldspathic Regolith Breccia). Được phong bế trong khung kính acrylic tinh khiết kèm tem kiểm định của Meteoritical Society.'
    },
    {
      id: 'sample-5',
      title: 'Kính Thiên Văn Galileo Cổ Điển Bọc Da Khâu Tay',
      category: 'instrument',
      categoryLabel: 'Khí cụ Thiên văn Đồng thau',
      origin: 'Padua, Ý',
      epoch: 'Phục dựng Thế kỷ 17',
      code: 'TEL-1609',
      price: 2950000,
      rarity: 4,
      badge: 'Da Bò Thật Khâu Tay',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAmwoESWSseEgvlr1xLGbxqt4vD6-MdST5kke9lQa9J1obQFMXsihhxssCbeZIjLff-EO_407AhhUtY2-9K6tAFg7W8Jx8hg-bDyFcok-ks_b7KzPyOu1ac5Suez-_iuuZY0Kozm1USCfbBJYtnJUKhP6RlfUw6PWeO1eKeU9H4EISIFX8JstyZgErVyzzFdw-ICGkXTTqhV3JHCH65uVkQ5a3B8xFdcQAMKBuESnmW5kXGcD_PKKKdhqFfpLHF-sec6XjFCLQQzw',
      desc: 'Kính thiên văn khúc xạ bằng đồng thau bọc da thuộc thảo mộc với đường chỉ may tay thủ công tinh tế. Bao gồm bộ giá đỡ ba chân bằng đồng thau có thể điều chỉnh góc nâng.'
    },
    {
      id: 'sample-6',
      title: 'Bản Khắc Gỗ Dürer Constellation Map (Bản in giới hạn 1515)',
      category: 'map',
      categoryLabel: 'Bản đồ & Bản họa Cổ',
      origin: 'Nuremberg, Đức',
      epoch: 'Năm 1515',
      code: 'DUR-1515',
      price: 1800000,
      rarity: 4,
      badge: 'Bản In Giới Hạn 1515',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAlWLr1jhU6qFqg0LpY4d-4ibwRtQUxQUVBKAHHhuG1ljE4qL1BEdfY62SgHlpa9TSEUCHhsakeZ0XXutHjQbFOBsT8eiFz-DWdB91O19Si32RHjHGiTZLDWQVe7MZ6x_e5uqIdzePK73Wwc1Jwrn_l-y05B5191FjgNRIWei2oiY_6K9VJlXyw0QvE5Q11V0kI6ci2buANfcvnfouMknrOHXYUxk5ghNnBo9U0J1FgEFYhtS0gS6PAO1PjmQhWkWJWA5KiVWNL1w',
      desc: 'Tác phẩm lịch sử hợp tác giữa Albrecht Dürer và các nhà thiên văn học, miêu tả 48 chòm sao Ptolemaic cùng chân dung bốn nhà thiên văn học cổ đại vĩ đại ở bốn góc.'
    },
    {
      id: 'sample-7',
      title: 'Bộ Huy Hiệu Thám Hiểm Apollo 11 & Voyager Golden Record',
      category: 'badge',
      categoryLabel: 'Huy hiệu Thám hiểm',
      origin: 'NASA Viện Thám Hiểm',
      epoch: 'Kỷ nguyên Không gian (1969-1977)',
      code: 'EXP-1977',
      price: 950000,
      rarity: 3,
      badge: 'Bộ Hộp Quà Gỗ Nhung',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAKat0GpmHCr5RzDyPzE8I7d7a0PhkZv6CkgEp2LEq-0-651groniQ-_YFACVMiKq-IioYdf1e1zuBpOPKLsSzrGOVM8CrtFy6RhKc4zJaWx8uU0VsQ993Upa2Kqm19K_IDhEffZtkIb1d4fnsm5r0CxptmxAMaoLz_mtq9ZRaQlPOUzyUx97UrB3rGsP9yV37VYhF03gogW4L7HiKUyxuIj7MmTLvTqTrKJQ1eu5-z4VUbA7RZfQaRBj4MbmdcucpNJvSG5CdrHw',
      desc: 'Bộ sưu tập giới hạn gồm huy hiệu thêu chính thức sứ mệnh Apollo 11 hạ cánh xuống Mặt Trăng năm 1969 và bản mô phỏng mạ vàng 24k đĩa hát vũ trụ Voyager mang thông điệp của Trái Đất.'
    }
  ];

  categories = [
    { value: 'all', label: 'Tất cả hiện vật' },
    { value: 'meteorite', label: 'Mảnh Thiên thạch' },
    { value: 'map', label: 'Bản đồ & Bản họa Cổ' },
    { value: 'instrument', label: 'Khí cụ Thiên văn Đồng thau' },
    { value: 'badge', label: 'Huy hiệu Thám hiểm' }
  ];

  activeCategory = 'all';
  sortMode = 'newest';

  selectedProduct: StoreProduct | null = null;

  isQuickViewOpen = false;
  isCartOpen = false;
  isSuccessOpen = false;
  isAudioPlaying = false;
  isCheckoutLoading = false;

  customerName = '';
  customerEmail = '';
  customerAddress = '';

  constructor() {
    this.productService.getProducts().subscribe({
      next: result => {
        if (result.items.length > 0) {
          this.products = result.items.map(product => this.mapProduct(product));
        }
      },
      error: error => console.error('Product API error:', error)
    });
  }

  get cart(): CartItem[] {
    return this.cartService.lines().map(line => ({
      productId: line.product.id,
      title: line.product.name,
      price: line.product.salePrice,
      image: line.product.images[0]?.url ?? this.products[0]?.image ?? '',
      qty: line.quantity
    }));
  }

  get filteredProducts(): StoreProduct[] {
    let result = [...this.products];

    if (this.activeCategory !== 'all') {
      result = result.filter(
        product => product.category === this.activeCategory
      );
    }

    switch (this.sortMode) {
      case 'price-asc':
        result.sort((a, b) => a.price - b.price);
        break;

      case 'price-desc':
        result.sort((a, b) => b.price - a.price);
        break;

      case 'rarity':
        result.sort((a, b) => b.rarity - a.rarity);
        break;

      case 'newest':
      default:
        result.sort((a, b) => a.title.localeCompare(b.title));
        break;
    }

    return result;
  }

  get cartCount(): number {
    return this.cart.reduce((sum, item) => sum + item.qty, 0);
  }

  get cartSubtotal(): number {
    return this.cart.reduce(
      (sum, item) => sum + item.price * item.qty,
      0
    );
  }

  get categoryCount(): number {
    if (this.activeCategory === 'all') {
      return this.products.length;
    }

    return this.products.filter(
      product => product.category === this.activeCategory
    ).length;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN').format(value) + ' ₫';
  }

  setCategory(category: string): void {
    this.activeCategory = category;
  }

  setSort(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sortMode = select.value;
  }

  openQuickView(product: StoreProduct): void {
    this.selectedProduct = product;
    this.isQuickViewOpen = true;
  }

  closeQuickView(): void {
    this.isQuickViewOpen = false;
    this.selectedProduct = null;
  }

  addToCart(product: StoreProduct): void {
    const source = this.findApiProduct(product.id);
    if (!source) {
      return;
    }

    this.cartService.add(source);
    this.isCartOpen = true;
    this.closeQuickView();
  }

  removeFromCart(index: number): void {
    const item = this.cart[index];
    if (item) {
      this.cartService.remove(item.productId);
    }
  }

  openCart(): void {
    this.isCartOpen = true;
  }

  closeCart(): void {
    this.isCartOpen = false;
  }

  toggleAudio(): void {
    this.isAudioPlaying = !this.isAudioPlaying;
  }

  submitCheckout(): void {
    if (this.cart.length === 0) {
      return;
    }

    this.router.navigate(['/ordering']);
  }

  closeSuccessModal(): void {
    this.isSuccessOpen = false;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isSuccessOpen) {
      this.closeSuccessModal();
      return;
    }

    if (this.isQuickViewOpen) {
      this.closeQuickView();
      return;
    }

    if (this.isCartOpen) {
      this.closeCart();
    }
  }

  private findApiProduct(id: string): Product | undefined {
    const line = this.cartService.lines().find(item => item.product.id === id);
    if (line) {
      return line.product;
    }

    return this.products
      .filter(product => product.id === id)
      .map(product => ({
        id: product.id,
        sku: product.code,
        name: product.title,
        slug: product.code.toLowerCase(),
        description: product.desc,
        originalPrice: product.price,
        salePrice: product.price,
        stockQuantity: 1,
        status: 'Active',
        categoryId: product.category,
        images: [{ id: product.id, url: product.image, displayOrder: 0 }]
      }))[0];
  }

  private mapProduct(product: Product): StoreProduct {
    return {
      id: product.id,
      title: product.name,
      category: this.mapCategory(product.name, product.description),
      categoryLabel: 'Hiện vật thiên văn',
      origin: 'CosmicWiki Collection',
      epoch: 'Tư liệu hiện đại',
      code: product.sku,
      price: product.salePrice,
      rarity: 3,
      badge: product.status,
      image: product.images[0]?.url ?? 'assets/images/cosmic-placeholder.svg',
      desc: product.description ?? 'Hiện vật được tuyển chọn trong bộ sưu tập CosmicWiki.'
    };
  }

  private mapCategory(name: string, description: string | null): StoreProduct['category'] {
    const text = `${name} ${description ?? ''}`.toLowerCase();
    if (text.includes('bản đồ') || text.includes('map')) {
      return 'map';
    }
    if (text.includes('huy hiệu') || text.includes('badge')) {
      return 'badge';
    }
    if (text.includes('kính') || text.includes('thước') || text.includes('instrument')) {
      return 'instrument';
    }
    return 'meteorite';
  }
}