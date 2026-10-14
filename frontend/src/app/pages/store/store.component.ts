import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CartService } from '../../core/services/cart.service';
import { Product, ProductCategory } from '../../features/product/models/product.model';
import { ProductService } from '../../features/product/services/product.service';

interface StoreProduct {
  id: string;
  title: string;
  category: string;
  categoryLabel: string;
  origin: string;
  epoch: string;
  code: string;
  price: number;
  rarity: number;
  badge: string;
  image: string;
  url?: string;
  urls?: string[];
  images?: string[];
  desc: string;
  stockQuantity: number;
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
  private readonly route = inject(ActivatedRoute);

  products: StoreProduct[] = [];
  contentError = '';

  categories: Array<{ value: string; label: string }> = [
    { value: 'all', label: 'Tất cả hiện vật' }
  ];

  activeCategory = 'all';
  sortMode = 'newest';
  searchTerm = '';

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
    this.route.queryParamMap.subscribe(params => this.searchTerm = params.get('search')?.trim() ?? '');
    this.products = [];
    forkJoin({
      products: this.productService.getProducts(),
      categories: this.productService.getCategories()
    }).subscribe({
      next: ({ products, categories }) => {
        this.categories = [
          { value: 'all', label: 'Tất cả hiện vật' },
          ...categories.map(category => ({
            value: category.id,
            label: category.name
          }))
        ];
        if (products.items.length > 0) {
          this.products = products.items.map(product =>
            this.mapProduct(product, categories));
        } else {
          this.products = [];
          this.contentError = 'Hiện chưa có hiện vật nào trong cửa hàng.';
        }
      },
      error: error => {
        this.products = [];
        this.contentError = 'Không thể tải dữ liệu cửa hàng.';
        console.error('Product API error:', error);
      }
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
    if (this.searchTerm) {
      const query = this.searchTerm.toLowerCase();
      result = result.filter(product =>
        `${product.title} ${product.code} ${product.categoryLabel} ${product.desc}`.toLowerCase().includes(query));
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

  selectProductImage(image: string): void {
    if (this.selectedProduct) {
      this.selectedProduct.image = image;
    }
  }

  get detailImages(): string[] {
    return this.selectedProduct?.images?.slice(0, 5) ?? [];
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
        stockQuantity: product.stockQuantity,
        status: 'Active',
        categoryId: product.category,
        urls: [product.image],
        images: [{ id: product.id, url: product.image, displayOrder: 0 }]
      }))[0];
  }

  private mapProduct(
    product: Product,
    categories: ProductCategory[] = []): StoreProduct {
    const orderedImages = product.images
        .sort((a, b) => a.displayOrder - b.displayOrder)
        .map(image => image.url);
    const urls = product.urls?.length ? product.urls : orderedImages;
    const url = orderedImages[0] ?? urls[0] ?? 'assets/images/cosmic-placeholder.svg';

    return {
      id: product.id,
      title: product.name,
      category: product.categoryId,
      categoryLabel: categories.find(category => category.id === product.categoryId)?.name
        ?? 'Hiện vật thiên văn',
      origin: 'CosmicWiki Collection',
      epoch: 'Tư liệu hiện đại',
      code: product.sku,
      price: product.salePrice,
      rarity: 3,
      badge: product.status,
      image: url,
      url,
      urls,
      images: urls.length > 0 ? urls : [url],
      desc: product.description ?? 'Hiện vật được tuyển chọn trong bộ sưu tập CosmicWiki.',
      stockQuantity: product.stockQuantity
    };
  }

}