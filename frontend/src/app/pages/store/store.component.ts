import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { Product, ProductCategory } from '../../features/product/models/product.model';
import { ProductService } from '../../features/product/services/product.service';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';

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
  stockQuantity: number;
}

@Component({
  selector: 'app-store',
  standalone: true,
  imports: [
    CommonModule,
    PaginationComponent
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
  productsLoading = false;
  productPage = 1;
  readonly productPageSize = 12;
  productTotalCount = 0;
  productTotalPages = 0;

  categories: Array<{ value: string; label: string }> = [
    { value: 'all', label: 'Tất cả hiện vật' }
  ];
  private productCategoryEntities: ProductCategory[] = [];
  private categorySlugs: Array<{ value: string; slug: string }> = [];
  private routeCategorySlug: string | null = null;
  private categoriesLoaded = false;

  activeCategory = 'all';
  sortMode = 'newest';
  searchTerm = '';

  selectedProduct: StoreProduct | null = null;

  isQuickViewOpen = false;
  isCartOpen = false;
  isSuccessOpen = false;

  constructor() {
    this.route.queryParamMap.subscribe(params => {
      this.searchTerm = params.get('search')?.trim() ?? '';
      this.productPage = 1;
      if (this.categoriesLoaded) this.loadProducts();
    });
    this.route.paramMap.subscribe(params => {
      this.routeCategorySlug = params.get('categorySlug');
      this.applyRouteCategory();
      this.productPage = 1;
      if (this.categoriesLoaded) this.loadProducts();
    });
    this.products = [];
    this.productService.getCategories().subscribe({
      next: categories => {
        this.productCategoryEntities = categories;
        this.categories = [
          { value: 'all', label: 'Tất cả hiện vật' },
          ...categories.map(category => ({ value: category.id, label: category.name }))
        ];
        this.categorySlugs = categories.map(category => ({
          value: category.id,
          slug: category.slug
        }));
        this.categoriesLoaded = true;
        this.applyRouteCategory();
        this.loadProducts();
      },
      error: () => {
        this.products = [];
        this.contentError = 'Không thể tải danh mục cửa hàng.';
      }
    });
  }

  private loadProducts(): void {
    this.productsLoading = true;
    this.contentError = '';
    const categoryId = this.activeCategory === 'all' ? undefined : this.activeCategory;
    this.productService.getProducts(
      this.productPage,
      this.productPageSize,
      this.searchTerm || undefined,
      categoryId
    ).subscribe({
      next: result => {
        this.products = result.items.map(product =>
          this.mapProduct(product, this.productCategoryEntities));
        this.productTotalCount = result.totalCount;
        this.productTotalPages = result.totalPages;
        this.productsLoading = false;
        if (this.productTotalCount === 0)
          this.contentError = 'Hiện chưa có hiện vật phù hợp trong cửa hàng.';
      },
      error: () => {
        this.products = [];
        this.productTotalCount = 0;
        this.productTotalPages = 0;
        this.productsLoading = false;
        this.contentError = 'Không thể tải dữ liệu cửa hàng.';
      }
    });
  }

  get cart(): CartItem[] {
    return this.cartService.lines().map(line => ({
      productId: line.product.id,
      title: line.product.name,
      price: line.product.salePrice,
      image: line.product.images[0]?.url
        ?? line.product.urls?.[0]
        ?? 'assets/images/cosmic-placeholder.svg',
      qty: line.quantity,
      stockQuantity: line.product.stockQuantity
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
    return this.cartService.selectedTotal;
  }

  get selectedCartCount(): number {
    return this.cartService.selectedCount;
  }

  get allCartItemsSelected(): boolean {
    return this.cartService.areAllSelected();
  }

  get someCartItemsSelected(): boolean {
    return this.cartService.selectedProductIds().length > 0 && !this.allCartItemsSelected;
  }

  toggleSelectAllCartItems(event: Event): void {
    this.cartService.selectAll((event.target as HTMLInputElement).checked);
  }

  toggleCartItemSelection(productId: string, event: Event): void {
    this.cartService.setSelected(productId, (event.target as HTMLInputElement).checked);
  }

  isCartItemSelected(productId: string): boolean {
    return this.cartService.isSelected(productId);
  }

  clearSelectedCartItems(): void {
    this.cartService.removeSelected();
  }

  changeCartItemQuantity(productId: string, quantity: number): void {
    this.cartService.setQuantity(productId, quantity);
  }

  get categoryCount(): number {
    return this.productTotalCount;
  }

  get activeCategoryLabel(): string {
    const category = this.categories.find(item => item.value === this.activeCategory);
    return category && category.value !== 'all'
      ? category.label
      : 'Cửa Hàng Quà Lưu Niệm Bảo Tàng Vũ Trụ';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN').format(value) + ' ₫';
  }

  setCategory(category: string): void {
    this.productPage = 1;
    const slug = this.categorySlugs.find(item => item.value === category)?.slug;
    if (slug) {
      this.router.navigate(['/store/category', slug]);
      return;
    }
    this.router.navigate(['/store']);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.productTotalPages || page === this.productPage) return;
    this.productPage = page;
    this.loadProducts();
  }

  private applyRouteCategory(): void {
    if (!this.routeCategorySlug) {
      this.activeCategory = 'all';
      return;
    }

    if (!this.categoriesLoaded) return;

    const category = this.categorySlugs.find(item => item.slug === this.routeCategorySlug);
    if (!category) {
      this.router.navigate(['/store'], { replaceUrl: true });
      return;
    }
    this.activeCategory = category.value;
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

  submitCheckout(): void {
    if (this.cartService.selectedLines().length === 0) {
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