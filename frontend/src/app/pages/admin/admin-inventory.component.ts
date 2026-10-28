import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { API_CONFIG } from '../../core/config/api.config';
import { PagedResult } from '../../core/models/paged-result.model';
import { PaginationComponent } from '../../core/components/pagination/pagination.component';

interface Product {
  id: string; sku: string; name: string; slug: string; description?: string;
  originalPrice: number; salePrice: number; stockQuantity: number; status: string; categoryId: string;
}
interface Category { id: string; name: string; slug: string; description?: string; }

@Component({
  selector: 'app-admin-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PaginationComponent],
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
