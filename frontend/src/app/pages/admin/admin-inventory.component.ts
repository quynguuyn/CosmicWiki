import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { API_CONFIG } from '../../core/config/api.config';
import { PagedResult } from '../../core/models/paged-result.model';

interface Product {
  id: string; sku: string; name: string; slug: string; description?: string;
  originalPrice: number; salePrice: number; stockQuantity: number; status: string; categoryId: string;
}
interface Category { id: string; name: string; slug: string; description?: string; }

@Component({
  selector: 'app-admin-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <main class="admin-crud-page">
      <a routerLink="/admin" class="back-link">← Admin Dashboard</a>
      <header><p class="eyebrow">COSMICWIKI · INVENTORY</p><h1>Kho hiện vật</h1><p>Quản lý danh mục, giá, tồn kho và trạng thái sản phẩm.</p></header>
      <nav class="tabs"><button [class.active]="tab === 'products'" (click)="tab='products'">Sản phẩm</button><button [class.active]="tab === 'categories'" (click)="tab='categories'">Danh mục</button></nav>
      <p class="feedback" *ngIf="error">{{ error }}</p>
      <section *ngIf="tab === 'products'">
        <form class="editor cosmic-glass" (ngSubmit)="saveProduct()">
          <h2>{{ editingProduct ? 'Sửa sản phẩm' : 'Thêm sản phẩm' }}</h2>
          <input [(ngModel)]="productForm.sku" name="sku" placeholder="SKU" required>
          <input [(ngModel)]="productForm.name" name="name" placeholder="Tên sản phẩm" required>
          <input [(ngModel)]="productForm.slug" name="slug" placeholder="Slug" required>
          <input [(ngModel)]="productForm.originalPrice" name="originalPrice" type="number" placeholder="Giá gốc" required>
          <input [(ngModel)]="productForm.salePrice" name="salePrice" type="number" placeholder="Giá bán" required>
          <input [(ngModel)]="productForm.stockQuantity" name="stockQuantity" type="number" placeholder="Tồn kho" required>
          <select [(ngModel)]="productForm.categoryId" name="categoryId" required><option value="">Chọn danh mục</option><option *ngFor="let category of allCategories" [value]="category.id">{{ category.name }}</option></select>
          <select [(ngModel)]="productForm.status" name="status"><option value="Draft">Nháp</option><option value="Published">Đang bán</option><option value="Archived">Lưu trữ</option></select>
          <textarea [(ngModel)]="productForm.description" name="description" placeholder="Mô tả"></textarea>
          <div><button class="primary" type="submit">{{ editingProduct ? 'Lưu thay đổi' : 'Tạo sản phẩm' }}</button><button type="button" (click)="resetProduct()">Xóa form</button></div>
        </form>
        <div class="search-bar cosmic-glass"><input [(ngModel)]="productSearch" (ngModelChange)="onProductSearchChanged()" name="productSearch" placeholder="Tìm theo SKU, tên hoặc slug"><span>{{ totalCount }} sản phẩm</span></div>
        <p class="loading" *ngIf="productLoading">Đang tải sản phẩm...</p>
        <div class="table-wrap cosmic-glass" *ngIf="!productLoading"><table><thead><tr><th>Sản phẩm</th><th>Giá</th><th>Tồn kho</th><th>Trạng thái</th><th></th></tr></thead><tbody><tr *ngFor="let product of filteredProducts"><td><b>{{ product.name }}</b><small>{{ product.sku }}</small></td><td>{{ product.salePrice | currency:'USD' }}</td><td>{{ product.stockQuantity }}</td><td>{{ product.status }}</td><td><button (click)="editProduct(product)">Sửa</button><button class="danger" (click)="deleteProduct(product)">Xóa</button></td></tr></tbody></table><p class="empty" *ngIf="products.length === 0">Không có sản phẩm phù hợp.</p></div>
        <nav class="admin-pagination" aria-label="Phân trang sản phẩm" *ngIf="!productLoading && totalPages > 1"><button type="button" [disabled]="page <= 1" (click)="goToPage(page - 1)">Trước</button><span>Trang {{ page }} / {{ totalPages }} · {{ totalCount }} sản phẩm</span><button type="button" [disabled]="page >= totalPages" (click)="goToPage(page + 1)">Sau</button></nav>
      </section>
      <section *ngIf="tab === 'categories'">
        <form class="editor cosmic-glass" (ngSubmit)="saveCategory()"><h2>{{ editingCategory ? 'Sửa danh mục' : 'Thêm danh mục' }}</h2><input [(ngModel)]="categoryForm.name" name="categoryName" placeholder="Tên danh mục" required><input [(ngModel)]="categoryForm.slug" name="categorySlug" placeholder="Slug" required><textarea [(ngModel)]="categoryForm.description" name="categoryDescription" placeholder="Mô tả"></textarea><button class="primary" type="submit">Lưu danh mục</button><button type="button" (click)="resetCategory()">Xóa form</button></form>
        <div class="search-bar cosmic-glass"><input [(ngModel)]="categorySearch" (ngModelChange)="onCategorySearchChanged()" name="categorySearch" placeholder="Tìm danh mục theo tên hoặc slug"><span>{{ categoryTotalCount }} danh mục</span></div>
        <p class="loading" *ngIf="categoryLoading">Đang tải danh mục...</p>
        <div class="table-wrap cosmic-glass" *ngIf="!categoryLoading"><table><thead><tr><th>Tên</th><th>Slug</th><th></th></tr></thead><tbody><tr *ngFor="let category of filteredCategories"><td>{{ category.name }}</td><td>{{ category.slug }}</td><td><button (click)="editCategory(category)">Sửa</button><button class="danger" (click)="deleteCategory(category)">Xóa</button></td></tr></tbody></table><p class="empty" *ngIf="filteredCategories.length === 0">Không có danh mục phù hợp.</p></div>
        <nav class="admin-pagination" aria-label="Phân trang danh mục" *ngIf="!categoryLoading && categoryTotalPages > 1"><button type="button" [disabled]="categoryPage <= 1" (click)="goToCategoryPage(categoryPage - 1)">Trước</button><span>Trang {{ categoryPage }} / {{ categoryTotalPages }} · {{ categoryTotalCount }} danh mục</span><button type="button" [disabled]="categoryPage >= categoryTotalPages" (click)="goToCategoryPage(categoryPage + 1)">Sau</button></nav>
      </section>
    </main>
  `,
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
    const request = { ...this.productForm, originalPrice: Number(this.productForm.originalPrice), salePrice: Number(this.productForm.salePrice), stockQuantity: Number(this.productForm.stockQuantity), categoryId: this.productForm.categoryId };
    const call = this.editingProduct ? this.http.put(`${this.base}/api/product/products/${this.editingProduct.id}`, request) : this.http.post(`${this.base}/api/product/products`, request);
    call.subscribe({ next: () => { this.resetProduct(); this.load(); }, error: () => this.error = 'Không thể lưu sản phẩm.' });
  }
  deleteProduct(product: Product) { if (confirm(`Xóa ${product.name}?`)) this.http.delete(`${this.base}/api/product/products/${product.id}`).subscribe({ next: () => this.load(), error: () => this.error = 'Không thể xóa sản phẩm.' }); }
  editProduct(product: Product) { this.editingProduct = product; this.productForm = { ...product }; }
  resetProduct() { this.editingProduct = undefined; this.productForm = this.emptyProduct(); }
  saveCategory() {
    const call = this.editingCategory ? this.http.put(`${this.base}/api/product/categories/${this.editingCategory.id}`, this.categoryForm) : this.http.post(`${this.base}/api/product/categories`, this.categoryForm);
    call.subscribe({ next: () => { this.resetCategory(); this.load(); }, error: () => this.error = 'Không thể lưu danh mục.' });
  }
  deleteCategory(category: Category) { if (confirm(`Xóa ${category.name}?`)) this.http.delete(`${this.base}/api/product/categories/${category.id}`).subscribe({ next: () => this.load(), error: () => this.error = 'Không thể xóa danh mục.' }); }
  editCategory(category: Category) { this.editingCategory = category; this.categoryForm = { ...category }; }
  resetCategory() { this.editingCategory = undefined; this.categoryForm = this.emptyCategory(); }
  private emptyProduct() { return { sku: '', name: '', slug: '', description: '', originalPrice: 0, salePrice: 0, stockQuantity: 0, status: 'Draft', categoryId: '' }; }
  private emptyCategory() { return { name: '', slug: '', description: '' }; }
}
