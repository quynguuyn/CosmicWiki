import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { API_CONFIG } from '../../core/config/api.config';
import { PagedResult } from '../../core/models/paged-result.model';

interface Account { userId: string; username: string; email: string; status: string; createdAt: string; roles: string[]; }
interface Role { roleId: string; name: string; description?: string; }
interface Permission { permissionId: string; code: string; name: string; description?: string; }

@Component({
  selector: 'app-admin-accounts',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <main class="admin-crud-page">
      <a routerLink="/admin" class="back-link">← Admin Dashboard</a>
      <header><p class="eyebrow">COSMICWIKI · ACCESS CONTROL</p><h1>Tài khoản</h1><p>Tạo tài khoản thông thường trước, sau đó chọn bộ lọc User và cấp role Delivery để mở luồng giao hàng.</p><a routerLink="/register">Tạo tài khoản mới</a></header>
      <nav class="tabs"><button *ngFor="let filter of roleFilters" [class.active]="roleFilter === filter" (click)="setRoleFilter(filter)">{{ filter || 'Nhân viên' }}</button></nav>
      <p class="feedback" *ngIf="error">{{ error }}</p>
      <section class="account-layout">
        <div>
          <p class="loading" *ngIf="loading">Đang tải tài khoản...</p>
          <div class="table-wrap cosmic-glass" *ngIf="!loading"><table><thead><tr><th>Tài khoản</th><th>Vai trò</th><th>Trạng thái</th><th>Ngày tạo</th><th></th></tr></thead><tbody><tr *ngFor="let account of accounts"><td><b>{{ account.username }}</b><small>{{ account.email }}</small></td><td><span *ngFor="let role of account.roles" class="tag">{{ role }}</span></td><td>{{ account.status }}</td><td>{{ account.createdAt | date:'short' }}</td><td><button (click)="selected = account">Phân quyền</button></td></tr></tbody></table><p class="empty" *ngIf="accounts.length === 0">Không có tài khoản phù hợp.</p></div>
          <nav class="admin-pagination" aria-label="Phân trang tài khoản" *ngIf="!loading && totalPages > 1"><button type="button" [disabled]="page <= 1" (click)="goToPage(page - 1)">Trước</button><span>Trang {{ page }} / {{ totalPages }} · {{ totalCount }} tài khoản</span><button type="button" [disabled]="page >= totalPages" (click)="goToPage(page + 1)">Sau</button></nav>
        </div>
        <aside class="editor cosmic-glass" *ngIf="selected"><h2>{{ selected.username }}</h2><p>{{ selected.email }}</p><label>Thêm vai trò</label><select [(ngModel)]="selectedRole"><option value="">Chọn role</option><option *ngFor="let role of roles" [value]="role.roleId">{{ role.name }}</option></select><button class="primary" (click)="assignRole()">Gán role</button><hr><h3>Permission của role</h3><select [(ngModel)]="selectedPermission"><option value="">Chọn permission</option><option *ngFor="let permission of permissions" [value]="permission.permissionId">{{ permission.code }}</option></select><button class="primary" (click)="assignPermission()">Gán permission</button><div class="permission-list"><span *ngFor="let permission of rolePermissions" class="tag">{{ permission.code }}</span></div></aside>
      </section>
    </main>
  `,
  styleUrl: './admin-crud.component.scss'
})
export class AdminAccountsComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly base = API_CONFIG.baseUrl;
  accounts: Account[] = []; roles: Role[] = []; permissions: Permission[] = []; rolePermissions: Permission[] = [];
  selected?: Account; selectedRole = ''; selectedPermission = ''; roleFilter = ''; error = '';
  page = 1; readonly pageSize = 10; totalCount = 0; totalPages = 0; loading = false;
  readonly roleFilters = ['', 'User', 'Admin', 'Delivery'];

  ngOnInit() { this.loadAccounts(); this.http.get<Role[]>(`${this.base}/api/auth/admin/roles`).subscribe({ next: r => this.roles = r }); this.http.get<Permission[]>(`${this.base}/api/auth/admin/permissions`).subscribe({ next: p => this.permissions = p }); }
  loadAccounts() {
    this.loading = true;
    this.error = '';
    const params: Record<string, string> = { page: String(this.page), pageSize: String(this.pageSize) };
    if (this.roleFilter) params['role'] = this.roleFilter;
    this.http.get<PagedResult<Account>>(`${this.base}/api/auth/admin/accounts`, { params }).subscribe({
      next: result => {
        this.accounts = result.items;
        this.page = result.page;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        if (this.selected) this.selected = this.accounts.find(account => account.userId === this.selected?.userId) ?? this.selected;
        this.loading = false;
      },
      error: () => { this.error = 'Không thể tải tài khoản.'; this.loading = false; }
    });
  }
  setRoleFilter(role: string) { this.roleFilter = role; this.page = 1; this.loadAccounts(); }
  goToPage(page: number) {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.page = page;
    this.loadAccounts();
  }
  assignRole() { if (!this.selected || !this.selectedRole) return; this.http.post(`${this.base}/api/auth/users/${this.selected.userId}/roles/${this.selectedRole}`, {}).subscribe({ next: () => this.loadAccounts(), error: () => this.error = 'Không thể gán role.' }); }
  assignPermission() {
    if (!this.selectedRole || !this.selectedPermission) return;
    this.http.put(`${this.base}/api/auth/admin/roles/${this.selectedRole}/permissions/${this.selectedPermission}`, {}).subscribe({ next: () => this.loadPermissions(), error: () => this.error = 'Không thể gán permission.' });
  }
  loadPermissions() { if (!this.selectedRole) return; this.http.get<Permission[]>(`${this.base}/api/auth/admin/roles/${this.selectedRole}/permissions`).subscribe({ next: p => this.rolePermissions = p }); }
}
