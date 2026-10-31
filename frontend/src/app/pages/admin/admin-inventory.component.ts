import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { API_CONFIG } from '../../core/config/api.config';
import { PagedResult } from '../../core/models/paged-result.model';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../core/components/confirm-dialog/confirm-dialog.component';

interface Product {
  id: string; sku: string; name: string; slug: string; description?: string;
  originalPrice: number; salePrice: number; stockQuantity: number; status: string; categoryId: string;
  images?: ProductImage[];
}
interface ProductImage { id?: string; url: string; displayOrder: number; }
interface Category { id: string; name: string; slug: string; description?: string; }

@Component({
  selector: 'app-admin-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PaginationComponent, ConfirmDialogComponent],
  templateUrl: './admin-inventory.component.html',
  styleUrl: './admin-crud.component.scss'
})
export class AdminInventoryComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly base = API_CONFIG.baseUrl;
  tab = 'products'; error = ''; editingProduct?: Product; editingCategory?: Category;
  products: Product[] = []; categories: Category[] = []; allCategories: Category[] = [];
  productSearch = ''; categorySearch = '';
  page = 1; readonly pageSize = 10; totalCount = 0; totalPages = 0; productLoading = false;
  categoryPage = 1; readonly categoryPageSize = 10; categoryTotalCount = 0; categoryTotalPages = 0; categoryLoading = false;
  productForm: any = this.emptyProduct(); categoryForm: any = this.emptyCategory();
  pendingDelete?: { type: 'product' | 'category'; item: Product | Category };
  get filteredProducts() { return this.products; }
  get filteredCategories() { return this.categories; }
  ngOnInit() { this.load(); }
  load() {
    this.error = '';
    this.loadProducts();
    this.http.get<Category[]>(`${this.base}/api/product/categories`).subscribe({ next: r => this.allCategories = r, error: () => this.error = 'Không thể tải danh mục.' });
    this.loadCategories();
  }
  private loadProducts() {
    this.productLoading = true;
    const params: Record<string, string> = { page: String(this.page), pageSize: String(this.pageSize) };
    if (this.productSearch.trim()) params['search'] = this.productSearch.trim();
    this.http.get<PagedResult<Product>>(`${this.base}/api/product/products`, { params }).subscribe({
      next: result => {
        this.products = result.items;
        this.page = result.page;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.productLoading = false;
        if (this.totalPages > 0 && this.page > this.totalPages) {
          this.page = this.totalPages;
          this.loadProducts();
        }
      },
      error: () => { this.error = 'Không thể tải sản phẩm.'; this.productLoading = false; }
    });
  }
  onProductSearchChanged() { this.page = 1; this.loadProducts(); }
  private loadCategories() {
    this.categoryLoading = true;
    const params: Record<string, string> = { page: String(this.categoryPage), pageSize: String(this.categoryPageSize) };
    if (this.categorySearch.trim()) params['search'] = this.categorySearch.trim();
    this.http.get<PagedResult<Category>>(`${this.base}/api/product/categories/page`, { params }).subscribe({
      next: result => {
        this.categories = result.items;
        this.categoryPage = result.page;
        this.categoryTotalCount = result.totalCount;
        this.categoryTotalPages = result.totalPages;
        this.categoryLoading = false;
        if (this.categoryTotalPages > 0 && this.categoryPage > this.categoryTotalPages) {
          this.categoryPage = this.categoryTotalPages;
          this.loadCategories();
        }
      },
      error: () => { this.error = 'Không thể tải danh mục.'; this.categoryLoading = false; }
    });
  }
  onCategorySearchChanged() { this.categoryPage = 1; this.loadCategories(); }
  goToCategoryPage(page: number) {
    if (page < 1 || page > this.categoryTotalPages || page === this.categoryPage) return;
    this.categoryPage = page;
    this.loadCategories();
  }
  goToPage(page: number) {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.page = page;
    this.loadProducts();
  }
  saveProduct() {
    this.error = '';
    const originalPrice = Number(this.productForm.originalPrice);
    const salePrice = Number(this.productForm.salePrice);
    if (salePrice > originalPrice) {
      this.error = 'Giá bán không thể lớn hơn giá gốc.';
      return;
    }
    const imageUrls = this.productForm.imageUrls
      .split(/\r?\n/)
      .map((url: string) => url.trim())
      .filter(Boolean);
    const request = {
      sku: this.productForm.sku.trim(),
      name: this.productForm.name.trim(),
      slug: this.productForm.slug.trim(),
      description: this.productForm.description.trim() || null,
      originalPrice,
      salePrice,
      stockQuantity: Number(this.productForm.stockQuantity),
      categoryId: this.productForm.categoryId,
      // ASP.NET serializes ProductStatus as its numeric enum value (1/2/3).
      status: Number(this.productForm.status),
      images: imageUrls.map((url: string, displayOrder: number) => ({ url, displayOrder }))
    };
    const call = this.editingProduct ? this.http.put(`${this.base}/api/product/products/${this.editingProduct.id}`, request) : this.http.post(`${this.base}/api/product/products`, request);
    call.subscribe({
      next: () => { this.resetProduct(); this.load(); },
      error: response => this.error = response.error?.detail ?? 'Không thể lưu sản phẩm.'
    });
  }
  deleteProduct(product: Product) { this.pendingDelete = { type: 'product', item: product }; }
  editProduct(product: Product) {
    this.editingProduct = product;
    this.productForm = {
      ...product,
      imageUrls: (product.images ?? []).map(image => image.url).join('\n')
    };
  }
  resetProduct() { this.editingProduct = undefined; this.productForm = this.emptyProduct(); }
  saveCategory() {
    const call = this.editingCategory ? this.http.put(`${this.base}/api/product/categories/${this.editingCategory.id}`, this.categoryForm) : this.http.post(`${this.base}/api/product/categories`, this.categoryForm);
    call.subscribe({ next: () => { this.resetCategory(); this.load(); }, error: () => this.error = 'Không thể lưu danh mục.' });
  }
  deleteCategory(category: Category) { this.pendingDelete = { type: 'category', item: category }; }
  confirmDelete() {
    const pending = this.pendingDelete;
    if (!pending) return;
    const endpoint = pending.type === 'product' ? 'products' : 'categories';
    this.http.delete(`${this.base}/api/product/${endpoint}/${pending.item.id}`).subscribe({
      next: () => { this.pendingDelete = undefined; this.load(); },
      error: () => { this.pendingDelete = undefined; this.error = `Không thể xóa ${pending.type === 'product' ? 'sản phẩm' : 'danh mục'}.`; }
    });
  }
  editCategory(category: Category) { this.editingCategory = category; this.categoryForm = { ...category }; }
  resetCategory() { this.editingCategory = undefined; this.categoryForm = this.emptyCategory(); }
  private emptyProduct() { return { sku: '', name: '', slug: '', description: '', originalPrice: 0, salePrice: 0, stockQuantity: 0, status: 1, categoryId: '', imageUrls: '' }; }
  private emptyCategory() { return { name: '', slug: '', description: '' }; }
}
